using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class GoToStatement(Identifier labelName, CodeModel.Statements.Statement source)
	: JumpStatement(labelName, source)
{
}
