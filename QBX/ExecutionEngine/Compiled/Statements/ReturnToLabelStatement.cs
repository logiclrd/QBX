using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ReturnToLabelStatement(string labelName, CodeModel.Statements.Statement source)
	: JumpStatement(labelName, source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		stackFrame.PopReturnPath(Source);

		base.Execute(context, stackFrame);
	}
}
