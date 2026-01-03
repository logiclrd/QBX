
namespace QBX.ExecutionEngine;

[Serializable]
public class RuntimeException : Exception
{
	CodeModel.Statements.Statement Statement;

	public RuntimeException(CodeModel.Statements.Statement statement, string message)
		: base(message)
	{
		Statement = statement;
	}
}
