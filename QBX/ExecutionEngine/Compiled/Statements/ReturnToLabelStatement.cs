using QBX.ExecutionEngine.Execution;
using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ReturnToLabelStatement(Identifier labelName, CodeModel.Statements.ReturnStatement source)
	: JumpStatement(labelName, source)
{
	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		stackFrame.PopReturnPath(Source);

		base.ExecuteImplementation(context, stackFrame);
	}
}
