using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class LabelStatement(string labelName, CodeModel.Statements.Statement source) : Executable(source)
{
	public string LabelName = labelName;

	public override bool CanBreak { get => false; set { } }
	public override bool IsLabel => true;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
	}
}
