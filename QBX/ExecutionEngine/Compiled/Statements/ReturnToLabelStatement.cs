using QBX.ExecutionEngine.Execution;
using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ReturnToLabelStatement(Identifier labelName, CodeModel.Statements.Statement source)
	: JumpStatement(labelName, source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		stackFrame.PopReturnPath(Source);

		base.Execute(context, stackFrame);
	}
}
