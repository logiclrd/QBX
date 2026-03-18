using QBX.ExecutionEngine.Execution;
using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class LabelStatement(Identifier labelName, CodeModel.Statements.Statement source) : Executable(source)
{
	public Identifier LabelName = labelName;

	public override bool CanBreak { get => false; set { } }
	public override bool IsLabel => true;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
	}
}
