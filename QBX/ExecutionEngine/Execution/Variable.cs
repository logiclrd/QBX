using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution;

public class Variable
{
	public object? Data;
	public DataType DataType { get; }

	public Variable(DataType dataType)
	{
		DataType = dataType;
	}

	internal int CoerceToInt() => Convert.ToInt32(Data);
}
