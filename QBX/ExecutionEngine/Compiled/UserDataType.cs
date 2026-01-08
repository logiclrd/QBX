using System.Collections.Generic;

namespace QBX.ExecutionEngine.Compiled;

public class UserDataType(string name)
{
	public string Name { get; } = name;
	public List<UserDataTypeField> Fields { get; } = new List<UserDataTypeField>();

	public CodeModel.Statements.TypeStatement? Statement { get; }

	public UserDataType(CodeModel.Statements.TypeStatement typeStatement)
		: this(typeStatement.Name)
	{
		Statement = typeStatement;
	}

	public override string ToString() => "TYPE " + Name;
}
