using System;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution.Variables;

public abstract class Variable
{
	public static Variable Construct(DataType type)
	{
		if (type.IsArray)
			throw new Exception("Internal error: Construct called instead of ConstructArray for array type");
		else if (type.IsUserType)
			return new UserDataTypeVariable(type.UserType);
		else
		{
			switch (type.PrimitiveType)
			{
				case PrimitiveDataType.Integer: return new IntegerVariable();
				case PrimitiveDataType.Long: return new LongVariable();
				case PrimitiveDataType.Single: return new SingleVariable();
				case PrimitiveDataType.Double: return new DoubleVariable();
				case PrimitiveDataType.Currency: return new CurrencyVariable();
				case PrimitiveDataType.String: return new StringVariable(type.ByteSize);

				default: throw new Exception("Internal error: Unrecognized data type in Variable.Construct " + type);
			}
		}
	}

	public static Variable ConstructArray(DataType type)
		=> new ArrayVariable(type);

	public DataType DataType { get; }
	
	public Variable(DataType dataType)
	{
		DataType = dataType;
	}

	public abstract object GetData();
	public abstract void SetData(object value);
	public abstract int CoerceToInt(Evaluable? context);

	public abstract void Serialize(Span<byte> buffer);
	public abstract void Deserialize(ReadOnlySpan<byte> buffer);

	public abstract bool IsZero { get; }
	public abstract bool IsPositive { get; }
	public abstract bool IsNegative { get; }
}
