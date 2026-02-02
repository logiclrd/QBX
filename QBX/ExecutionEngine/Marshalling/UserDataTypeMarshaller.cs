using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware.Fonts;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Marshalling;

public class UserDataTypeMarshaller : Marshaller
{
	public static UserDataTypeMarshaller Construct(UserDataType userDataType, Type nativeType)
		=> new UserDataTypeMarshaller(userDataType, nativeType);

	UserDataTypeMarshaller(UserDataType userDataType, Type nativeType)
	{
		List<Func<ReadOnlySpan<byte>, object, int>> marshalOut = new List<Func<ReadOnlySpan<byte>, object, int>>();
		List<Func<object, Span<byte>, int>> marshalIn = new List<Func<object, Span<byte>, int>>();

		BuildMarshallers(userDataType, nativeType, marshalOut, marshalIn);

		_nativeType = nativeType;
		_fieldMarshallersOut = marshalOut.ToArray();
		_fieldMarshallersIn = marshalIn.ToArray();
	}

	Type _nativeType;

	Func<ReadOnlySpan<byte>, object, int>[] _fieldMarshallersOut;
	Func<object, Span<byte>, int>[] _fieldMarshallersIn;

	public override void Map(object from, ref object? to)
	{
		if (from is UserDataTypeVariable fromStructure)
		{
			if ((to != null)
			 && !_nativeType.IsAssignableFrom(to.GetType()))
				throw RuntimeException.TypeMismatch();

			var fromDataBuffer = new byte[fromStructure.DataType.ByteSize];

			fromStructure.Serialize(fromDataBuffer);

			var fromData = fromDataBuffer.AsSpan();

			Map(fromData, ref to);
		}
		else if (to is UserDataTypeVariable toStructure)
		{
			if ((from == null) || !_nativeType.IsAssignableFrom(from.GetType()))
				throw RuntimeException.TypeMismatch();

			var toDataBuffer = new byte[toStructure.DataType.ByteSize];

			var toData = toDataBuffer.AsSpan();

			Map(from, toData);

			toStructure.Deserialize(toDataBuffer);
		}
	}

	public void Map(ReadOnlySpan<byte> fromData, ref object? to)
	{
		if (to == null)
			to = Activator.CreateInstance(_nativeType)!;
		else if (!_nativeType.IsAssignableFrom(to.GetType()))
			throw RuntimeException.TypeMismatch();

		foreach (var fieldMarshaller in _fieldMarshallersOut)
			fromData = fromData.Slice(fieldMarshaller(fromData, to));
	}

	public int Map(object from, Span<byte> toData)
	{
		if ((from == null) || !_nativeType.IsAssignableFrom(from.GetType()))
			throw RuntimeException.TypeMismatch();

		int lengthAtStart = toData.Length;

		foreach (var fieldMarshaller in _fieldMarshallersIn)
			toData = toData.Slice(fieldMarshaller(from, toData));

		return lengthAtStart - toData.Length;
	}

	private static void BuildMarshallers(UserDataType userDataType, Type nativeType, List<Func<ReadOnlySpan<byte>, object, int>> marshalOut, List<Func<object, Span<byte>, int>> marshalIn)
	{
		foreach (var field in userDataType.Fields)
		{
			var members = nativeType.GetMember(field.Name, BindingFlags.Public | BindingFlags.Instance);

			if ((members == null) || (members.Length != 1))
				throw CompilerException.TypeMismatch();

			var member = members[0];

			Type memberType =
				member switch
				{
					FieldInfo fieldInfo => fieldInfo.FieldType,
					PropertyInfo propertyInfo => propertyInfo.PropertyType,

					_ => throw CompilerException.TypeMismatch()
				};

			if (field.Type.IsPrimitiveType)
			{
				switch (field.Type.PrimitiveType)
				{
					case PrimitiveDataType.Integer:
						marshalOut.Add(IntegerMarshal.BuildOut(member));
						marshalIn.Add(IntegerMarshal.BuildIn(member));
						break;
					case PrimitiveDataType.Long:
						marshalOut.Add(LongMarshal.BuildOut(member));
						marshalIn.Add(LongMarshal.BuildIn(member));
						break;
					case PrimitiveDataType.Single:
						marshalOut.Add(SingleMarshal.BuildOut(member));
						marshalIn.Add(SingleMarshal.BuildIn(member));
						break;
					case PrimitiveDataType.Double:
						marshalOut.Add(DoubleMarshal.BuildOut(member));
						marshalIn.Add(DoubleMarshal.BuildIn(member));
						break;
					case PrimitiveDataType.Currency:
						marshalOut.Add(CurrencyMarshal.BuildOut(member));
						marshalIn.Add(CurrencyMarshal.BuildIn(member));
						break;
					case PrimitiveDataType.String:
						marshalOut.Add(StringMarshal.BuildOut(member, fixedSize: field.Type.ByteSize));
						marshalIn.Add(StringMarshal.BuildIn(member, fixedSize: field.Type.ByteSize));
						break;

					default: throw new Exception("Internal error");
				}
			}
			else if (field.Type.IsArray)
			{
				if (field.ArraySubscripts == null)
					throw new Exception("Internal error: User data type array field with no subscripts");

				ArrayMarshal.BuildMarshallers(
					field.Type,
					field.ArraySubscripts,
					memberType,
					out var @out,
					out var @in);

				marshalOut.Add(@out);
				marshalIn.Add(@in);
			}
			else if (field.Type.IsUserType)
				BuildMarshallers(field.Type.UserType, memberType, marshalOut, marshalIn);
			else
				throw new Exception("Unable to map field type");
		}
	}

	static Action<object, object> BuildAssignment(MemberInfo memberInfo)
	{
		switch (memberInfo)
		{
			case FieldInfo fieldInfo:
			{
				var fieldType = fieldInfo.FieldType;

				if (fieldType.IsEnum)
					return (structure, value) => fieldInfo.SetValue(structure, Enum.ToObject(fieldType, value));
				else
					return (structure, value) => fieldInfo.SetValue(structure, UncheckedNumberConverter.ToType(value, fieldType));
			}
			case PropertyInfo propertyInfo:
			{
				var propertyType = propertyInfo.PropertyType;

				if (propertyType.IsEnum)
					return (structure, value) => propertyInfo.SetValue(structure, Enum.ToObject(propertyType, value));
				else
					return (structure, value) => propertyInfo.SetValue(structure, UncheckedNumberConverter.ToType(value, propertyType));
			}
		}

		throw new Exception("Internal error");
	}

	class IntegerMarshal
	{
		public static Func<ReadOnlySpan<byte>, object, int> BuildOut(MemberInfo memberInfo)
		{
			var primitiveMarshaller = new IntegerPrimitiveMarshaller();

			var assignment = BuildAssignment(memberInfo);

			return
				(data, target) =>
				{
					assignment(target, BinaryPrimitives.ReadInt16LittleEndian(data));
					return 2;
				};
		}

		public static Func<object, Span<byte>, int> BuildIn(MemberInfo memberInfo)
		{
			if (memberInfo is FieldInfo fieldInfo)
			{
				return
					(source, data) =>
					{
						BinaryPrimitives.WriteInt16LittleEndian(
							data,
							UncheckedNumberConverter.ToInt16Dynamic(fieldInfo.GetValue(source)!));
						return 2;
					};
			}
			else if (memberInfo is PropertyInfo propertyInfo)
			{
				return
					(source, data) =>
					{
						BinaryPrimitives.WriteInt16LittleEndian(
							data,
							UncheckedNumberConverter.ToInt16Dynamic(propertyInfo.GetValue(source)!));
						return 2;
					};
			}
			else
				throw CompilerException.TypeMismatch();
		}
	}

	class LongMarshal
	{
		public static Func<ReadOnlySpan<byte>, object, int> BuildOut(MemberInfo memberInfo)
		{
			var primitiveMarshaller = new LongPrimitiveMarshaller();

			var assignment = BuildAssignment(memberInfo);

			return
				(data, target) =>
				{
					assignment(target, BinaryPrimitives.ReadInt32LittleEndian(data));
					return 2;
				};
		}

		public static Func<object, Span<byte>, int> BuildIn(MemberInfo memberInfo)
		{
			if (memberInfo is FieldInfo fieldInfo)
			{
				return
					(source, data) =>
					{
						BinaryPrimitives.WriteInt32LittleEndian(
							data,
							UncheckedNumberConverter.ToInt32Dynamic(fieldInfo.GetValue(source)!));
						return 2;
					};
			}
			else if (memberInfo is PropertyInfo propertyInfo)
			{
				return
					(source, data) =>
					{
						BinaryPrimitives.WriteInt32LittleEndian(
							data,
							UncheckedNumberConverter.ToInt32Dynamic(propertyInfo.GetValue(source)!));
						return 2;
					};
			}
			else
				throw CompilerException.TypeMismatch();
		}
	}

	class SingleMarshal
	{
		public static Func<ReadOnlySpan<byte>, object, int> BuildOut(MemberInfo memberInfo)
		{
			var primitiveMarshaller = new SinglePrimitiveMarshaller();

			var assignment = BuildAssignment(memberInfo);

			return
				(data, target) =>
				{
					assignment(target, BinaryPrimitives.ReadSingleLittleEndian(data));
					return 2;
				};
		}

		public static Func<object, Span<byte>, int> BuildIn(MemberInfo memberInfo)
		{
			if (memberInfo is FieldInfo fieldInfo)
			{
				return
					(source, data) =>
					{
						BinaryPrimitives.WriteSingleLittleEndian(
							data,
							UncheckedNumberConverter.ToSingleDynamic(fieldInfo.GetValue(source)!));
						return 2;
					};
			}
			else if (memberInfo is PropertyInfo propertyInfo)
			{
				return
					(source, data) =>
					{
						BinaryPrimitives.WriteSingleLittleEndian(
							data,
							UncheckedNumberConverter.ToSingleDynamic(propertyInfo.GetValue(source)!));
						return 2;
					};
			}
			else
				throw CompilerException.TypeMismatch();
		}
	}

	class DoubleMarshal
	{
		public static Func<ReadOnlySpan<byte>, object, int> BuildOut(MemberInfo memberInfo)
		{
			var primitiveMarshaller = new DoublePrimitiveMarshaller();

			var assignment = BuildAssignment(memberInfo);

			return
				(data, target) =>
				{
					assignment(target, BinaryPrimitives.ReadDoubleLittleEndian(data));
					return 2;
				};
		}

		public static Func<object, Span<byte>, int> BuildIn(MemberInfo memberInfo)
		{
			if (memberInfo is FieldInfo fieldInfo)
			{
				return
					(source, data) =>
					{
						BinaryPrimitives.WriteDoubleLittleEndian(
							data,
							UncheckedNumberConverter.ToDoubleDynamic(fieldInfo.GetValue(source)!));
						return 2;
					};
			}
			else if (memberInfo is PropertyInfo propertyInfo)
			{
				return
					(source, data) =>
					{
						BinaryPrimitives.WriteDoubleLittleEndian(
							data,
							UncheckedNumberConverter.ToDoubleDynamic(propertyInfo.GetValue(source)!));
						return 2;
					};
			}
			else
				throw CompilerException.TypeMismatch();
		}
	}

	class CurrencyMarshal
	{
		public static Func<ReadOnlySpan<byte>, object, int> BuildOut(MemberInfo memberInfo)
		{
			var primitiveMarshaller = new CurrencyPrimitiveMarshaller();

			var assignment = BuildAssignment(memberInfo);

			return
				(data, target) =>
				{
					assignment(target, decimal.FromOACurrency(BinaryPrimitives.ReadInt64LittleEndian(data)));
					return 2;
				};
		}

		public static Func<object, Span<byte>, int> BuildIn(MemberInfo memberInfo)
		{
			if (memberInfo is FieldInfo fieldInfo)
			{
				return
					(source, data) =>
					{
						BinaryPrimitives.WriteInt64LittleEndian(
							data,
							decimal.ToOACurrency(
								UncheckedNumberConverter.ToDecimalDynamic(fieldInfo.GetValue(source)!)));
						return 2;
					};
			}
			else if (memberInfo is PropertyInfo propertyInfo)
			{
				return
					(source, data) =>
					{
						BinaryPrimitives.WriteInt64LittleEndian(
							data,
							decimal.ToOACurrency(
								UncheckedNumberConverter.ToDecimalDynamic(propertyInfo.GetValue(source)!)));
						return 2;
					};
			}
			else
				throw CompilerException.TypeMismatch();
		}
	}

	class StringMarshal
	{
		static CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

		public static Func<ReadOnlySpan<byte>, object, int> BuildOut(MemberInfo memberInfo, int fixedSize)
		{
			if (memberInfo is FieldInfo fieldInfo)
			{
				if (fieldInfo.FieldType == typeof(string))
				{
					return
						(data, target) =>
						{
							fieldInfo.SetValue(target, s_cp437.GetString(data.Slice(0, fixedSize)));
							return fixedSize;
						};
				}
				else if (fieldInfo.FieldType == typeof(char[]))
				{
					return
						(data, target) =>
						{
							var buffer = new char[fixedSize];
							s_cp437.GetChars(data.Slice(0, fixedSize), buffer.AsSpan());
							fieldInfo.SetValue(target, buffer);
							return fixedSize;
						};
				}
				else if (fieldInfo.FieldType == typeof(StringValue))
				{
					return
						(data, target) =>
						{
							var stringValue = fieldInfo.GetValue(target) as StringValue;

							if (stringValue == null)
							{
								stringValue = new StringValue();
								fieldInfo.SetValue(target, stringValue);
							}

							stringValue.Set(data.Slice(0, fixedSize));

							return fixedSize;
						};
				}
				else if (fieldInfo.FieldType == typeof(byte[]))
				{
					return
						(data, target) =>
						{
							fieldInfo.SetValue(target, data.Slice(0, fixedSize).ToArray());
							return fixedSize;
						};
				}
			}
			else if (memberInfo is PropertyInfo propertyInfo)
			{
				if (propertyInfo.PropertyType == typeof(string))
				{
					return
						(data, target) =>
						{
							propertyInfo.SetValue(target, s_cp437.GetString(data.Slice(0, fixedSize)));
							return fixedSize;
						};
				}
				else if (propertyInfo.PropertyType == typeof(char[]))
				{
					return
						(data, target) =>
						{
							var buffer = new char[fixedSize];
							s_cp437.GetChars(data.Slice(0, fixedSize), buffer.AsSpan());
							propertyInfo.SetValue(target, buffer);
							return fixedSize;
						};
				}
				else if (propertyInfo.PropertyType == typeof(StringValue))
				{
					return
						(data, target) =>
						{
							var stringValue = propertyInfo.GetValue(target) as StringValue;

							if (stringValue == null)
							{
								stringValue = new StringValue();
								propertyInfo.SetValue(target, stringValue);
							}

							stringValue.Set(data.Slice(0, fixedSize));

							return fixedSize;
						};
				}
				else if (propertyInfo.PropertyType == typeof(byte[]))
				{
					return
						(data, target) =>
						{
							propertyInfo.SetValue(target, data.Slice(0, fixedSize).ToArray());
							return fixedSize;
						};
				}
			}

			throw CompilerException.TypeMismatch();
		}

		public static Func<object, Span<byte>, int> BuildIn(MemberInfo memberInfo, int fixedSize)
		{
			if (memberInfo is FieldInfo fieldInfo)
			{
				if (fieldInfo.FieldType == typeof(string))
				{
					return
						(source, data) =>
						{
							string? str = (string?)fieldInfo.GetValue(source);

							if (str == null)
								data.Slice(0, fixedSize).Clear();
							else
							{
								var strSpan = str.AsSpan();

								if (strSpan.Length > fixedSize)
									strSpan = strSpan.Slice(0, fixedSize);

								int bytes = s_cp437.GetBytes(strSpan, data);

								data.Slice(bytes, fixedSize - bytes).Clear();
							}

							return fixedSize;
						};
				}
				else if (fieldInfo.FieldType == typeof(char[]))
				{
					return
						(source, data) =>
						{
							char[]? str = (char[]?)fieldInfo.GetValue(source);

							if (str == null)
								data.Slice(0, fixedSize).Clear();
							else
							{
								var strSpan = str.AsSpan();

								if (strSpan.Length > fixedSize)
									strSpan = strSpan.Slice(0, fixedSize);

								int bytes = s_cp437.GetBytes(strSpan, data);

								data.Slice(bytes, fixedSize - bytes).Clear();
							}

							return fixedSize;
						};
				}
				else if (fieldInfo.FieldType == typeof(StringValue))
				{
					return
						(source, data) =>
						{
							StringValue? str = (StringValue?)fieldInfo.GetValue(source);

							if (str == null)
								data.Slice(0, fixedSize).Clear();
							else
							{
								var strSpan = str.AsSpan();

								if (strSpan.Length > fixedSize)
									strSpan.Slice(0, fixedSize).CopyTo(data);
								else
								{
									strSpan.CopyTo(data);
									data.Slice(strSpan.Length, fixedSize - strSpan.Length).Clear();
								}
							}

							return fixedSize;
						};
				}
				else if (fieldInfo.FieldType == typeof(byte[]))
				{
					return
						(source, data) =>
						{
							byte[]? str = (byte[]?)fieldInfo.GetValue(source);

							if (str == null)
								data.Slice(0, fixedSize).Clear();
							else
							{
								var strSpan = str.AsSpan();

								if (strSpan.Length > fixedSize)
									strSpan.Slice(0, fixedSize).CopyTo(data);
								else
								{
									strSpan.CopyTo(data);
									data.Slice(strSpan.Length, fixedSize - strSpan.Length).Clear();
								}
							}

							return fixedSize;
						};
				}
			}
			else if (memberInfo is PropertyInfo propertyInfo)
			{
				if (propertyInfo.PropertyType == typeof(string))
				{
					return
						(source, data) =>
						{
							string? str = (string?)propertyInfo.GetValue(source);

							if (str == null)
								data.Slice(0, fixedSize).Clear();
							else
							{
								var strSpan = str.AsSpan();

								if (strSpan.Length > fixedSize)
									strSpan = strSpan.Slice(0, fixedSize);

								int bytes = s_cp437.GetBytes(strSpan, data);

								data.Slice(bytes, fixedSize - bytes).Clear();
							}

							return fixedSize;
						};
				}
				else if (propertyInfo.PropertyType == typeof(char[]))
				{
					return
						(source, data) =>
						{
							char[]? str = (char[]?)propertyInfo.GetValue(source);

							if (str == null)
								data.Slice(0, fixedSize).Clear();
							else
							{
								var strSpan = str.AsSpan();

								if (strSpan.Length > fixedSize)
									strSpan = strSpan.Slice(0, fixedSize);

								int bytes = s_cp437.GetBytes(strSpan, data);

								data.Slice(bytes, fixedSize - bytes).Clear();
							}

							return fixedSize;
						};
				}
				else if (propertyInfo.PropertyType == typeof(StringValue))
				{
					return
						(source, data) =>
						{
							StringValue? str = (StringValue?)propertyInfo.GetValue(source);

							if (str == null)
								data.Slice(0, fixedSize).Clear();
							else
							{
								var strSpan = str.AsSpan();

								if (strSpan.Length > fixedSize)
									strSpan.Slice(0, fixedSize).CopyTo(data);
								else
								{
									strSpan.CopyTo(data);
									data.Slice(strSpan.Length, fixedSize - strSpan.Length).Clear();
								}
							}

							return fixedSize;
						};
				}
				else if (propertyInfo.PropertyType == typeof(byte[]))
				{
					return
						(source, data) =>
						{
							byte[]? str = (byte[]?)propertyInfo.GetValue(source);

							if (str == null)
								data.Slice(0, fixedSize).Clear();
							else
							{
								var strSpan = str.AsSpan();

								if (strSpan.Length > fixedSize)
									strSpan.Slice(0, fixedSize).CopyTo(data);
								else
								{
									strSpan.CopyTo(data);
									data.Slice(strSpan.Length, fixedSize - strSpan.Length).Clear();
								}
							}

							return fixedSize;
						};
				}
			}

			throw CompilerException.TypeMismatch();
		}
	}

	class ArrayMarshal
	{
		static CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

		public static void BuildMarshallers(DataType arrayType, ArraySubscripts arraySubscripts, Type nativeType, out Func<ReadOnlySpan<byte>, object, int> marshalOut, out Func<object, Span<byte>, int> marshalIn)
		{
			int elementCount = arraySubscripts.ElementCount;

			if (arrayType.IsPrimitiveType)
			{
				switch (arrayType.PrimitiveType)
				{
					case PrimitiveDataType.Integer:
					{
						if (nativeType != typeof(short))
							throw CompilerException.TypeMismatch();

						marshalOut =
							(data, target) =>
							{
								var targetArray = (short[])target;

								var typedData = MemoryMarshal.Cast<byte, short>(data);

								typedData.Slice(0, elementCount).CopyTo(targetArray);

								return 2 * elementCount;
							};

						marshalIn =
							(source, data) =>
							{
								var sourceArray = ((short[])source).AsSpan();

								var typedData = MemoryMarshal.Cast<byte, short>(data);

								if (sourceArray.Length > elementCount)
									sourceArray = sourceArray.Slice(0, elementCount);

								sourceArray.CopyTo(typedData);
								typedData.Slice(sourceArray.Length, elementCount - sourceArray.Length).Clear();

								return 2 * elementCount;
							};

						break;
					}
					case PrimitiveDataType.Long:
					{
						if (nativeType != typeof(int))
							throw CompilerException.TypeMismatch();

						marshalOut =
							(data, target) =>
							{
								var targetArray = (int[])target;

								var typedData = MemoryMarshal.Cast<byte, int>(data);

								typedData.Slice(0, elementCount).CopyTo(targetArray);

								return 4 * elementCount;
							};

						marshalIn =
							(source, data) =>
							{
								var sourceArray = ((int[])source).AsSpan();

								var typedData = MemoryMarshal.Cast<byte, int>(data);

								if (sourceArray.Length > elementCount)
									sourceArray = sourceArray.Slice(0, elementCount);

								sourceArray.CopyTo(typedData);
								typedData.Slice(sourceArray.Length, elementCount - sourceArray.Length).Clear();

								return 4 * elementCount;
							};

						break;
					}
					case PrimitiveDataType.Single:
					{
						if (nativeType != typeof(float))
							throw CompilerException.TypeMismatch();

						marshalOut =
							(data, target) =>
							{
								var targetArray = (float[])target;

								var typedData = MemoryMarshal.Cast<byte, float>(data);

								typedData.Slice(0, elementCount).CopyTo(targetArray);

								return 4 * elementCount;
							};

						marshalIn =
							(source, data) =>
							{
								var sourceArray = ((float[])source).AsSpan();

								var typedData = MemoryMarshal.Cast<byte, float>(data);

								if (sourceArray.Length > elementCount)
									sourceArray = sourceArray.Slice(0, elementCount);

								sourceArray.CopyTo(typedData);
								typedData.Slice(sourceArray.Length, elementCount - sourceArray.Length).Clear();

								return 4 * elementCount;
							};

						break;
					}
					case PrimitiveDataType.Double:
					{
						if (nativeType != typeof(double))
							throw CompilerException.TypeMismatch();

						marshalOut =
							(data, target) =>
							{
								var targetArray = (double[])target;

								var typedData = MemoryMarshal.Cast<byte, double>(data);

								typedData.Slice(0, elementCount).CopyTo(targetArray);

								return 8 * elementCount;
							};

						marshalIn =
							(source, data) =>
							{
								var sourceArray = ((double[])source).AsSpan();

								var typedData = MemoryMarshal.Cast<byte, double>(data);

								if (sourceArray.Length > elementCount)
									sourceArray = sourceArray.Slice(0, elementCount);

								sourceArray.CopyTo(typedData);
								typedData.Slice(sourceArray.Length, elementCount - sourceArray.Length).Clear();

								return 8 * elementCount;
							};

						break;
					}
					case PrimitiveDataType.Currency:
					{
						if (nativeType != typeof(decimal))
							throw CompilerException.TypeMismatch();

						marshalOut =
							(data, target) =>
							{
								var targetArray = (decimal[])target;

								var typedData = MemoryMarshal.Cast<byte, long>(data);

								for (int i = 0; i < elementCount; i++)
									targetArray[i] = decimal.FromOACurrency(typedData[i]);

								return 8 * elementCount;
							};

						marshalIn =
							(source, data) =>
							{
								var sourceArray = ((decimal[])source).AsSpan();

								var typedData = MemoryMarshal.Cast<byte, long>(data);

								if (sourceArray.Length > elementCount)
									sourceArray = sourceArray.Slice(0, elementCount);

								for (int i = 0; i < sourceArray.Length; i++)
									typedData[i] = decimal.ToOACurrency(sourceArray[i]);
								typedData.Slice(sourceArray.Length, elementCount - sourceArray.Length).Clear();

								return 8 * elementCount;
							};

						break;
					}
					case PrimitiveDataType.String:
					{
						if (nativeType != typeof(string))
							throw CompilerException.TypeMismatch();

						int fixedSize = arrayType.ByteSize;

						marshalOut =
							(data, target) =>
							{
								var targetArray = (string[])target;

								for (int i = 0; i < elementCount; i++)
								{
									targetArray[i] = s_cp437.GetString(data.Slice(0, fixedSize));
									data = data.Slice(fixedSize);
								}

								return fixedSize * elementCount;
							};

						marshalIn =
							(source, data) =>
							{
								var sourceArray = ((string[])source).AsSpan();

								var typedData = MemoryMarshal.Cast<byte, long>(data);

								if (sourceArray.Length > elementCount)
									sourceArray = sourceArray.Slice(0, elementCount);

								for (int i = 0; i < sourceArray.Length; i++)
								{
									var sourceChars = sourceArray[i].AsSpan();

									if (sourceChars.Length > fixedSize)
										sourceChars = sourceChars.Slice(0, fixedSize);

									int numBytes = s_cp437.GetBytes(sourceChars, data);

									data.Slice(numBytes, fixedSize - numBytes).Clear();
								}

								typedData.Slice(sourceArray.Length * fixedSize, (elementCount - sourceArray.Length) * fixedSize).Clear();

								return fixedSize * elementCount;
							};

						break;
					}
				}
			}
			else if (arrayType.IsUserType)
			{
				var userTypeMapper = UserDataTypeMarshaller.Construct(arrayType.UserType, nativeType.GetElementType() ?? nativeType);

				int fixedSize = arrayType.UserType.CalculateByteSize();

				marshalOut =
					(data, target) =>
					{
						var targetArray = (System.Array)target;

						for (int i = 0; i < elementCount; i++)
						{
							object? to = null;

							userTypeMapper.Map(data, ref to);

							targetArray.SetValue(to, i);

							data = data.Slice(fixedSize);
						}

						return fixedSize * elementCount;
					};

				marshalIn =
					(source, data) =>
					{
						var sourceArray = (System.Array)source;

						int sourceElementCount = Math.Min(sourceArray.Length, elementCount);

						for (int i = 0; i < sourceElementCount; i++)
						{
							var from = sourceArray.GetValue(i);

							int numBytes =
								from is null
								? 0
								: userTypeMapper.Map(from, data);

							data.Slice(numBytes, fixedSize - numBytes).Clear();
						}

						data.Slice(sourceElementCount * fixedSize, (elementCount - sourceElementCount) * fixedSize).Clear();

						return fixedSize * elementCount;
					};
			}

			throw new Exception("Internal error: Can't figure out array type");
		}
	}
}

