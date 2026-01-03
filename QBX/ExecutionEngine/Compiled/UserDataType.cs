namespace QBX.ExecutionEngine.Compiled;

public class UserDataType(string name)
{
	public string Name => name;
	public List<UserDataTypeMember> Members { get; } = new List<UserDataTypeMember>();
}
