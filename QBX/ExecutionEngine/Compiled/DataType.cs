using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public class DataType
{
	public readonly PrimitiveDataType PrimitiveType;
	public readonly UserDataType? UserType;
	public readonly bool IsArray;

	public bool IsPrimitiveType => (UserType == null);
	public bool IsUserType => (UserType != null);

	public bool IsInteger => PrimitiveType == PrimitiveDataType.Integer;
	public bool IsLong => PrimitiveType == PrimitiveDataType.Long;
	public bool IsSingle => PrimitiveType == PrimitiveDataType.Single;
	public bool IsDouble => PrimitiveType == PrimitiveDataType.Double;
	public bool IsCurrency => PrimitiveType == PrimitiveDataType.Currency;
	public bool IsString => PrimitiveType == PrimitiveDataType.String;

	private DataType(PrimitiveDataType primitiveType, bool isArray = false)
	{
		PrimitiveType = primitiveType;
		IsArray = isArray;
	}

	public DataType(UserDataType userType, bool isArray = false)
	{
		UserType = userType;
		IsArray = isArray;
	}

	public DataType MakeArrayType()
	{
		if (UserType != null)
			return new DataType(UserType, isArray: true);
		else
			return new DataType(PrimitiveType, isArray: true);
	}

	public static readonly DataType Integer = new DataType(PrimitiveDataType.Integer);
	public static readonly DataType Long = new DataType(PrimitiveDataType.Long);
	public static readonly DataType Single = new DataType(PrimitiveDataType.Single);
	public static readonly DataType Double = new DataType(PrimitiveDataType.Double);
	public static readonly DataType String = new DataType(PrimitiveDataType.String);
	public static readonly DataType Currency = new DataType(PrimitiveDataType.Currency);

	public static DataType FromCodeModelDataType(CodeModel.DataType dataType)
	{
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
}
