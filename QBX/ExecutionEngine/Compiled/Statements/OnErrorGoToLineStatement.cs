using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class OnErrorGoToLineStatement(string target, bool local, CodeModel.Statements.Statement source)
	: JumpStatement(target, source)
{
	public override bool TargetIsInMainModule => !local;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (TargetPath == null)
			throw new Exception("Internal error: Executing an unresolved OnErrorGoToLineStatement");

		if (!local)
			context.SetErrorHandler(ErrorResponse.ExecuteHandler, TargetPath);
		else
			context.SetLocalErrorHandler(stackFrame, ErrorResponse.ExecuteHandler, TargetPath);
	}
}
