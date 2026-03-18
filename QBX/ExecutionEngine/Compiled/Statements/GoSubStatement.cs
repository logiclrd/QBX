using QBX.ExecutionEngine.Execution;
using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class GoSubStatement(Identifier labelName, CodeModel.Statements.Statement source)
	: JumpStatement(labelName, source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		var returnPath = GetPathToStatement(offset: 1);

		stackFrame.PushReturnPath(returnPath);

		base.Execute(context, stackFrame);
	}
}
