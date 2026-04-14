using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class GoToStatement(Identifier labelName, CodeModel.Statements.GoToStatement source)
	: JumpStatement(labelName, source)
{
}
