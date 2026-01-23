using System;
using System.Diagnostics.CodeAnalysis;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public class DataType
{
	public readonly PrimitiveDataType PrimitiveType;
	public readonly UserDataType? UserType;
	public readonly bool IsArray;
	public readonly int ByteSize;

	public bool IsPrimitiveType => (UserType == null);
	[MemberNotNullWhen(true, nameof(UserType))]
	public bool IsUserType => (UserType != null);

	public bool IsInteger => PrimitiveType == PrimitiveDataType.Integer;
	public bool IsLong => PrimitiveType == PrimitiveDataType.Long;
	public bool IsSingle => PrimitiveType == PrimitiveDataType.Single;
	public bool IsDouble => PrimitiveType == PrimitiveDataType.Double;
	public bool IsCurrency => PrimitiveType == PrimitiveDataType.Currency;
	public bool IsString => PrimitiveType == PrimitiveDataType.String;

	public bool IsNumeric
	{
		get
		{
			switch (PrimitiveType)
			{
				case PrimitiveDataType.Integer:
				case PrimitiveDataType.Long:
				case PrimitiveDataType.Single:
				case PrimitiveDataType.Double:
				case PrimitiveDataType.Currency:
					return true;
			}

			return false;
		}
	}

	private DataType(PrimitiveDataType primitiveType, bool isArray = false)
	{
		PrimitiveType = primitiveType;
		IsArray = isArray;

		switch (primitiveType)
		{
			case PrimitiveDataType.Integer: ByteSize = 2; break;
			case PrimitiveDataType.Long: ByteSize = 4; break;
			case PrimitiveDataType.Single: ByteSize = 4; break;
			case PrimitiveDataType.Double: ByteSize = 8; break;
			case PrimitiveDataType.Currency: ByteSize = 8; break;
		}
	}

	public DataType(UserDataType userType, bool isArray = false)
	{
		UserType = userType;
		IsArray = isArray;
		ByteSize = userType.CalculateByteSize();
	}

	private DataType(int fixedLength)
	{
		PrimitiveType = PrimitiveDataType.String;
		ByteSize = fixedLength;
	}

	public static DataType MakeFixedStringType(int fixedLength)
	{
		return new DataType(fixedLength);
	}

	public DataType MakeArrayType()
	{
		if (UserType != null)
			return new DataType(UserType, isArray: true);
		else
			return new DataType(PrimitiveType, isArray: true);
	}

	internal DataType MakeElementType()
	{
		if (UserType != null)
			return new DataType(UserType, isArray: false);
		else
			return new DataType(PrimitiveType, isArray: false);
	}

	public static readonly DataType Integer = new DataType(PrimitiveDataType.Integer);
	public static readonly DataType Long = new DataType(PrimitiveDataType.Long);
	public static readonly DataType Single = new DataType(PrimitiveDataType.Single);
	public static readonly DataType Double = new DataType(PrimitiveDataType.Double);
	public static readonly DataType String = new DataType(PrimitiveDataType.String);
	public static readonly DataType Currency = new DataType(PrimitiveDataType.Currency);

	public static DataType FromCodeModelDataType(CodeModel.DataType dataType, int fixedStringLength = 0)
	{
		if ((dataType == CodeModel.DataType.STRING)
		 && (fixedStringLength > 0))
			return MakeFixedStringType(fixedStringLength);

		switch (dataType)
		{
			case CodeModel.DataType.INTEGER: return DataType.Integer;
			case CodeModel.DataType.LONG: return DataType.Long;
			case CodeModel.DataType.SINGLE: return DataType.Single;
			case CodeModel.DataType.DOUBLE: return DataType.Double;
			case CodeModel.DataType.STRING: return DataType.String;
			case CodeModel.DataType.CURRENCY: return DataType.Currency;

			default: throw new Exception("Cannot construct ExecutionEngine.Compiled.DataType from unknown CodeModel.DataType");
		}
	}

	public static DataType ForPrimitiveDataType(PrimitiveDataType variableType)
	{
		switch (variableType)
		{
			case PrimitiveDataType.Integer: return DataType.Integer;
			case PrimitiveDataType.Long: return DataType.Long;
			case PrimitiveDataType.Single: return DataType.Single;
			case PrimitiveDataType.Double: return DataType.Double;
			case PrimitiveDataType.String: return DataType.String;
			case PrimitiveDataType.Currency: return DataType.Currency;

			default: throw new Exception("Cannot construct ExecutionEngine.Compiled.DataType from unknown PrimitiveDataType");
		}
	}

	public bool Equals(DataType? otherType)
	{
		if (otherType == null)
			return false;

		if (IsArray != otherType.IsArray)
			return false;

		if ((UserType == null) && (otherType.UserType == null))
			return PrimitiveType == otherType.PrimitiveType;
		if ((UserType != null) && (otherType.UserType != null))
			return UserType == otherType.UserType;

		return false;
	}

	public override string ToString()
	{
		string? array = IsArray ? "()" : null;

		if (IsPrimitiveType)
			return PrimitiveType.ToString().ToUpperInvariant() + array;
		else if (IsUserType)
			return "TYPE " + UserType + array;
		else
			return "Data Type";
	}
}
