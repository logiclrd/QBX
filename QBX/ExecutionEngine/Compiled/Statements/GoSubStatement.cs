using QBX.ExecutionEngine.Execution;
using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class GoSubStatement(Identifier labelName, CodeModel.Statements.GoSubStatement source)
	: JumpStatement(labelName, source)
{
	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		var returnPath = GetPathToStatement(offset: 1);

		stackFrame.PushReturnPath(returnPath);

		base.ExecuteImplementation(context, stackFrame);
	}
}
