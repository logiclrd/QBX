using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ComputedBranchTarget(Identifier labelName, Token token)
{
	public Identifier LabelName = labelName;
	public Token Token = token;

	public StatementPath? TargetPath;
}
