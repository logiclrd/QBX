using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public class UserDataTypeField(string name, DataType type, ArraySubscripts? subscripts)
{
	public string Name => name;
	public DataType Type => type;
	public ArraySubscripts? ArraySubscripts => subscripts;
}
