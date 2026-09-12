using QBX.ExecutionEngine.Execution;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class LabelStatement(Identifier labelName, Token? labelToken, CodeModel.Statements.Statement source) : Executable(source)
{
	public Identifier LabelName = labelName;
	public Token? LabelToken = labelToken;

	public override bool CanBreak { get => false; set { } }
	public override bool IsLabel => true;

	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
	}
}
