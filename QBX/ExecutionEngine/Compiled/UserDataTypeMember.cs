namespace QBX.ExecutionEngine.Compiled;

public class UserDataTypeMember(string name, DataType type)
{
	public string Name => name;
	public DataType Type => type;
}
