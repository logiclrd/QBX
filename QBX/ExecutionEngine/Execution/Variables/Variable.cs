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
			throw new NotImplementedException("TODO: return new UserDataTypeVariable(type.UserType);");
		else
		{
			switch (type.PrimitiveType)
			{
				default: throw new Exception("Internal error: Unrecognized data type in Variable.Construct " + type);
			}
		}
	}

	public static Variable ConstructArray(DataType type, ArraySubscripts subscripts)
	{
		throw new NotImplementedException("TODO");
	}

	public DataType DataType { get; }
	
	public Variable(DataType dataType)
	{
		DataType = dataType;
	}

	public abstract object GetData();
	public abstract void SetData(object value);
	public abstract int CoerceToInt();

	public abstract bool IsZero { get; }
	public abstract bool IsPositive { get; }
	public abstract bool IsNegative { get; }
}
