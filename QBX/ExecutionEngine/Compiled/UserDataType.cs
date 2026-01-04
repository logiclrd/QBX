namespace QBX.ExecutionEngine.Compiled;

public class UserDataType(string name)
{
	public string Name { get; } = name;
	public List<UserDataTypeMember> Members { get; } = new List<UserDataTypeMember>();

	public CodeModel.Statements.TypeStatement? Statement { get; }

	public UserDataType(CodeModel.Statements.TypeStatement typeStatement)
		: this(typeStatement.Name)
	{
		Statement = typeStatement;
	}
}
