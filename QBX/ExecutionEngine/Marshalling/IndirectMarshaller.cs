using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Marshalling;

public class IndirectMarshaller : Marshaller
{
	private IndirectMarshaller(int structureSize, Func<BinaryReader, object> outThunk, Action<object, BinaryWriter> inThunk)
	{
		_structureSize = structureSize;
		_outThunk = outThunk;
		_inThunk = inThunk;
	}

	int _structureSize;
	Func<BinaryReader, object> _outThunk;
	Action<object, BinaryWriter> _inThunk;

	[ThreadStatic]
	static byte[]? s_buffer;

	public override void Map(object from, ref object? to)
	{
		if (from is Variable fromVariable)
		{
			if ((s_buffer == null) || (s_buffer.Length < _structureSize))
				s_buffer = new byte[_structureSize * 2];

			fromVariable.Serialize(s_buffer);

			to = _outThunk(new BinaryReader(new MemoryStream(s_buffer, 0, _structureSize)));
		}
		else if (to is Variable toVariable)
		{
			if ((s_buffer == null) || (s_buffer.Length < _structureSize))
				s_buffer = new byte[_structureSize * 2];

			_inThunk(from, new BinaryWriter(new MemoryStream(s_buffer, 0, _structureSize)));

			toVariable.Deserialize(s_buffer);
		}
	}

	public static IndirectMarshaller Construct(Type nativeType, FixedLengthAttribute? fixedLengthAttribute)
	{
		return new IndirectMarshaller(
			CalculateStructureSize(nativeType, fixedLengthAttribute),
			BuildOut(nativeType, fixedLengthAttribute),
			BuildIn(nativeType, fixedLengthAttribute));
	}

	[ThreadStatic]
	static Dictionary<Type, int>? s_structureSizeCache;

	static int CalculateStructureSize(Type type, FixedLengthAttribute? fixedLengthAttribute)
	{
		s_structureSizeCache ??= new Dictionary<Type, int>();

		if (s_structureSizeCache.TryGetValue(type, out var cachedSize))
			return cachedSize;

		s_structureSizeCache[type] = 0; // Prevent infinite recursion on cycles

		int structureSize;

		if ((type == typeof(byte)) || (type == typeof(sbyte)))
			structureSize = 1;
		else if ((type == typeof(short)) || (type == typeof(ushort)))
			structureSize = 2;
		else if ((type == typeof(int)) || (type == typeof(uint)))
			structureSize = 4;
		else if ((type == typeof(long)) || (type == typeof(ulong)))
			structureSize = 8;
		else if (type == typeof(float))
			structureSize = 4;
		else if (type == typeof(double))
			structureSize = 8;
		else if (type == typeof(decimal))
			structureSize = 8;
		else if (type == typeof(string))
			structureSize = fixedLengthAttribute?.Length ?? throw new Exception("String must have [FixedLength(..)] attribute");
		else if (type.IsArray)
		{
			if (fixedLengthAttribute == null)
				throw new Exception("Array must have [FixedLength(..)] attribute");

			var elementType = type.GetElementType() ?? throw new Exception("Internal error");

			int elementSize = CalculateStructureSize(elementType, fixedLengthAttribute: null);

			structureSize = fixedLengthAttribute.Length * elementSize;
		}
		else
		{
			structureSize = 0;

			foreach (var member in EnumerateDataMembers(type))
			{
				var memberFixedLength = member.GetCustomAttribute<FixedLengthAttribute>();

				switch (member)
				{
					case FieldInfo fieldInfo: structureSize += CalculateStructureSize(fieldInfo.FieldType, memberFixedLength); break;
					case PropertyInfo propertyInfo: structureSize += CalculateStructureSize(propertyInfo.PropertyType, memberFixedLength); break;
				}
			}
		}

		s_structureSizeCache[type] = structureSize;

		return structureSize;
	}

	static Func<BinaryReader, object> BuildOut(Type type, FixedLengthAttribute? fixedLengthAttribute)
	{
		var reader = Expression.Parameter(typeof(BinaryReader));

		var body = new List<Expression>();

		var variables = new List<ParameterExpression>();

		var @this = Expression.Variable(type);

		variables.Add(@this);

		BuildOut(type, fixedLengthAttribute, reader, @this, body, variables, typesVisited: new HashSet<Type>());

		body.Add(Expression.Convert(@this, typeof(object)));

		var bodyBlock = Expression.Block(typeof(object), variables, body);

		return Expression.Lambda<Func<BinaryReader, object>>(bodyBlock, reader).Compile();
	}

	static MethodInfo BinaryReader_ReadByte = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadByte), BindingFlags.Public | BindingFlags.Instance)!;
	static MethodInfo BinaryReader_ReadSByte = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadSByte), BindingFlags.Public | BindingFlags.Instance)!;
	static MethodInfo BinaryReader_ReadInt16 = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadInt16), BindingFlags.Public | BindingFlags.Instance)!;
	static MethodInfo BinaryReader_ReadUInt16 = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadUInt16), BindingFlags.Public | BindingFlags.Instance)!;
	static MethodInfo BinaryReader_ReadInt32 = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadInt32), BindingFlags.Public | BindingFlags.Instance)!;
	static MethodInfo BinaryReader_ReadUInt32 = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadUInt32), BindingFlags.Public | BindingFlags.Instance)!;
	static MethodInfo BinaryReader_ReadInt64 = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadInt64), BindingFlags.Public | BindingFlags.Instance)!;
	static MethodInfo BinaryReader_ReadUInt64 = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadUInt64), BindingFlags.Public | BindingFlags.Instance)!;
	static MethodInfo BinaryReader_ReadSingle = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadSingle), BindingFlags.Public | BindingFlags.Instance)!;
	static MethodInfo BinaryReader_ReadDouble = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadDouble), BindingFlags.Public | BindingFlags.Instance)!;
	static MethodInfo BinaryReader_ReadBytes = typeof(BinaryReader).GetMethod(nameof(BinaryReader.ReadBytes), BindingFlags.Public | BindingFlags.Instance)!;

	static MethodInfo Decimal_FromOACurrency = typeof(decimal).GetMethod(nameof(decimal.FromOACurrency), BindingFlags.Public | BindingFlags.Static)!;

	static ConstructorInfo StringValue_ctor = typeof(StringValue).GetConstructor([typeof(byte[])])!;

	public static void BuildOut(Type type, FixedLengthAttribute? fixedLengthAttribute, Expression reader, Expression @this, List<Expression> body, List<ParameterExpression> variables, HashSet<Type> typesVisited)
	{
		if (!typesVisited.Add(type))
			throw new Exception("Type contains a reference to itself: " + type);

		try
		{
			Expression? valueExpression = null;

			if (type == typeof(byte))
				valueExpression = Expression.Call(reader, BinaryReader_ReadByte);
			else if (type == typeof(sbyte))
				valueExpression = Expression.Call(reader, BinaryReader_ReadSByte);
			else if (type == typeof(short))
				valueExpression = Expression.Call(reader, BinaryReader_ReadInt16);
			else if (type == typeof(ushort))
				valueExpression = Expression.Call(reader, BinaryReader_ReadUInt16);
			else if (type == typeof(int))
				valueExpression = Expression.Call(reader, BinaryReader_ReadInt32);
			else if (type == typeof(uint))
				valueExpression = Expression.Call(reader, BinaryReader_ReadUInt32);
			else if (type == typeof(long))
				valueExpression = Expression.Call(reader, BinaryReader_ReadInt64);
			else if (type == typeof(ulong))
				valueExpression = Expression.Call(reader, BinaryReader_ReadUInt64);
			else if (type == typeof(float))
				valueExpression = Expression.Call(reader, BinaryReader_ReadSingle);
			else if (type == typeof(double))
				valueExpression = Expression.Call(reader, BinaryReader_ReadDouble);
			else if (type == typeof(decimal))
			{
				valueExpression = Expression.Call(
					null,
					Decimal_FromOACurrency,
					Expression.Call(reader, BinaryReader_ReadInt64));
			}
			else if (type == typeof(StringValue))
			{
				if (fixedLengthAttribute == null)
					throw new Exception("String must have [FixedLength(..)] attribute");

				valueExpression = Expression.New(
					StringValue_ctor,
					Expression.Call(reader, BinaryReader_ReadBytes, Expression.Constant(fixedLengthAttribute.Length)));
			}

			if (valueExpression != null)
			{
				body.Add(
					Expression.Assign(
						@this,
						valueExpression));
			}
			else if (type.IsArray)
			{
				if (fixedLengthAttribute == null)
					throw new Exception("Array must have [FixedLength(..)] attribute");

				var elementType = type.GetElementType()!;

				var arrayVariable = Expression.Variable(type);

				var loopIterator = Expression.Variable(typeof(int));

				variables.Add(arrayVariable);
				variables.Add(loopIterator);

				body.Add(
					Expression.Assign(
						arrayVariable,
						Expression.NewArrayBounds(
							elementType,
							Expression.Constant(fixedLengthAttribute.Length))));

				body.Add(
					Expression.Assign(
						loopIterator,
						Expression.Constant(0)));

				var item = Expression.ArrayIndex(arrayVariable, loopIterator);

				var itemBody = new List<Expression>();
				var itemVariables = new List<ParameterExpression>();

				BuildOut(elementType, fixedLengthAttribute: null, reader, item, itemBody, itemVariables, typesVisited);

				var itemBlock =
					((itemBody.Count == 1) && (itemVariables.Count == 0))
					? itemBody[0]
					: Expression.Block(itemVariables, itemBody);

				var loopExit = Expression.Label();

				var loopBody =
					Expression.IfThenElse(
						Expression.LessThan(loopIterator, Expression.Constant(fixedLengthAttribute.Length)),
						ifTrue: itemBlock,
						ifFalse: Expression.Break(loopExit));

				body.Add(
					Expression.Loop(
						loopBody,
						loopExit));

				body.Add(Expression.Assign(
					@this,
					arrayVariable));
			}
			else
			{
				var thisVariable = Expression.Variable(type);

				variables.Add(thisVariable);

				body.Add(
					Expression.Assign(
						thisVariable,
						Expression.New(type)));

				foreach (var member in EnumerateDataMembers(type))
				{
					switch (member)
					{
						case FieldInfo:
						case PropertyInfo:
						{
							Type memberType;
							Expression memberExpression;

							switch (member)
							{
								case FieldInfo fieldInfo:
								{
									memberType = fieldInfo.FieldType;
									memberExpression = Expression.Field(thisVariable, fieldInfo);
									break;
								}
								case PropertyInfo propertyInfo:
								{
									memberType = propertyInfo.PropertyType;
									memberExpression = Expression.Property(thisVariable, propertyInfo);
									break;
								}

								default: throw new Exception("Sanity failure");
							}

							BuildOut(
								memberType,
								member.GetCustomAttribute<FixedLengthAttribute>(),
								reader,
								memberExpression,
								body,
								variables,
								typesVisited);

							break;
						}
					}
				}

				body.Add(
					Expression.Assign(
						@this,
						thisVariable));
			}
		}
		finally
		{
			typesVisited.Remove(type);
		}
	}

	public static Action<object, BinaryWriter> BuildIn(Type type, FixedLengthAttribute? fixedLengthAttribute)
	{
		var writer = Expression.Parameter(typeof(BinaryWriter));

		var body = new List<Expression>();

		var variables = new List<ParameterExpression>();

		var thisParameter = Expression.Parameter(typeof(object));

		var @this = Expression.Variable(type);

		variables.Add(@this);

		body.Add(
			Expression.Assign(
				@this,
				Expression.Convert(thisParameter, type)));

		BuildIn(type, fixedLengthAttribute, writer, @this, body, variables, typesVisited: new HashSet<Type>());

		var bodyBlock = Expression.Block(variables, body);

		return Expression.Lambda<Action<object, BinaryWriter>>(bodyBlock, thisParameter, writer).Compile();
	}

	static MethodInfo BinaryWriter_WriteByte = typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), BindingFlags.Public | BindingFlags.Instance, [typeof(byte)])!;
	static MethodInfo BinaryWriter_WriteSByte = typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), BindingFlags.Public | BindingFlags.Instance, [typeof(sbyte)])!;
	static MethodInfo BinaryWriter_WriteInt16 = typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), BindingFlags.Public | BindingFlags.Instance, [typeof(short)])!;
	static MethodInfo BinaryWriter_WriteUInt16 = typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), BindingFlags.Public | BindingFlags.Instance, [typeof(ushort)])!;
	static MethodInfo BinaryWriter_WriteInt32 = typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), BindingFlags.Public | BindingFlags.Instance, [typeof(int)])!;
	static MethodInfo BinaryWriter_WriteUInt32 = typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), BindingFlags.Public | BindingFlags.Instance, [typeof(uint)])!;
	static MethodInfo BinaryWriter_WriteInt64 = typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), BindingFlags.Public | BindingFlags.Instance, [typeof(long)])!;
	static MethodInfo BinaryWriter_WriteUInt64 = typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), BindingFlags.Public | BindingFlags.Instance, [typeof(ulong)])!;
	static MethodInfo BinaryWriter_WriteSingle = typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), BindingFlags.Public | BindingFlags.Instance, [typeof(float)])!;
	static MethodInfo BinaryWriter_WriteDouble = typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), BindingFlags.Public | BindingFlags.Instance, [typeof(double)])!;
	static MethodInfo BinaryWriter_WriteReadOnlySpanOfBytes = typeof(BinaryWriter).GetMethod(nameof(BinaryWriter.Write), BindingFlags.Public | BindingFlags.Instance, [typeof(ReadOnlySpan<byte>)])!;

	static MethodInfo Decimal_ToOACurrency = typeof(decimal).GetMethod(nameof(decimal.ToOACurrency), BindingFlags.Public | BindingFlags.Static)!;

	static MethodInfo StringValue_AsSpan = typeof(StringValue).GetMethod(nameof(StringValue.AsSpan), BindingFlags.Public | BindingFlags.Instance)!;

	public static void BuildIn(Type type, FixedLengthAttribute? fixedLengthAttribute, Expression writer, Expression @this, List<Expression> body, List<ParameterExpression> variables, HashSet<Type> typesVisited)
	{
		if (!typesVisited.Add(type))
			throw new Exception("Type contains a reference to itself: " + type);

		try
		{
			if (type == typeof(byte))
				body.Add(Expression.Call(writer, BinaryWriter_WriteByte, @this));
			else if (type == typeof(sbyte))
				body.Add(Expression.Call(writer, BinaryWriter_WriteSByte, @this));
			else if (type == typeof(short))
				body.Add(Expression.Call(writer, BinaryWriter_WriteInt16, @this));
			else if (type == typeof(ushort))
				body.Add(Expression.Call(writer, BinaryWriter_WriteUInt16, @this));
			else if (type == typeof(int))
				body.Add(Expression.Call(writer, BinaryWriter_WriteInt32, @this));
			else if (type == typeof(uint))
				body.Add(Expression.Call(writer, BinaryWriter_WriteUInt32, @this));
			else if (type == typeof(long))
				body.Add(Expression.Call(writer, BinaryWriter_WriteInt64, @this));
			else if (type == typeof(ulong))
				body.Add(Expression.Call(writer, BinaryWriter_WriteUInt64, @this));
			else if (type == typeof(float))
				body.Add(Expression.Call(writer, BinaryWriter_WriteSingle, @this));
			else if (type == typeof(double))
				body.Add(Expression.Call(writer, BinaryWriter_WriteDouble, @this));
			else if (type == typeof(decimal))
			{
				body.Add(Expression.Call(
					writer,
					BinaryWriter_WriteInt64,
					Expression.Call(null, Decimal_ToOACurrency, @this)));
			}
			else if (type == typeof(StringValue))
			{
				if (fixedLengthAttribute == null)
					throw new Exception("String member must have [FixedLength(..)] attribute");

				body.Add(Expression.Call(
					writer,
					BinaryWriter_WriteReadOnlySpanOfBytes,
					Expression.Convert(
						Expression.Call(@this, StringValue_AsSpan),
						typeof(ReadOnlySpan<byte>))));
			}
			else if (type.IsArray)
			{
				if (fixedLengthAttribute == null)
					throw new Exception("Array member must have [FixedLength(..)] attribute");

				var elementType = type.GetElementType()!;

				var arrayVariable = Expression.Variable(type);

				var loopIterator = Expression.Variable(typeof(int));

				variables.Add(arrayVariable);
				variables.Add(loopIterator);

				body.Add(
					Expression.Assign(
						arrayVariable,
						@this));

				body.Add(
					Expression.Assign(
						loopIterator,
						Expression.Constant(0)));

				var item = Expression.ArrayIndex(arrayVariable, loopIterator);

				var itemBody = new List<Expression>();
				var itemVariables = new List<ParameterExpression>();

				BuildIn(elementType, fixedLengthAttribute: null, writer, item, itemBody, itemVariables, typesVisited);

				var itemBlock =
					((itemBody.Count == 1) && (itemVariables.Count == 0))
					? itemBody[0]
					: Expression.Block(itemVariables, itemBody);

				var loopExit = Expression.Label();

				var loopBody =
					Expression.IfThenElse(
						Expression.LessThan(loopIterator, Expression.Constant(fixedLengthAttribute.Length)),
						ifTrue: itemBlock,
						ifFalse: Expression.Break(loopExit));

				body.Add(
					Expression.Loop(
						loopBody,
						loopExit));

				body.Add(Expression.Assign(
					@this,
					arrayVariable));
			}
			else
			{
				var thisVariable = Expression.Variable(type);

				variables.Add(thisVariable);

				body.Add(
					Expression.Assign(
						thisVariable,
						@this));

				foreach (var member in EnumerateDataMembers(type))
				{
					switch (member)
					{
						case FieldInfo:
						case PropertyInfo:
						{
							Type memberType;
							Expression memberExpression;

							switch (member)
							{
								case FieldInfo fieldInfo:
								{
									memberType = fieldInfo.FieldType;
									memberExpression = Expression.Field(thisVariable, fieldInfo);
									break;
								}
								case PropertyInfo propertyInfo:
								{
									memberType = propertyInfo.PropertyType;
									memberExpression = Expression.Property(thisVariable, propertyInfo);
									break;
								}

								default: throw new Exception("Sanity failure");
							}

							BuildIn(
								memberType,
								member.GetCustomAttribute<FixedLengthAttribute>(),
								writer,
								memberExpression,
								body,
								variables,
								typesVisited);

							break;
						}
					}
				}
			}
		}
		finally
		{
			typesVisited.Remove(type);
		}
	}
}
