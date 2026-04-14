using System;

using QBX.ExecutionEngine.Execution;
using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class OnErrorGoToLineStatement(Identifier target, bool local, CodeModel.Statements.OnErrorStatement source)
	: JumpStatement(target, source)
{
	public override bool TargetIsInMainModule => !local;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (TargetPath == null)
			throw new Exception("Internal error: Executing an unresolved OnErrorGoToLineStatement");

		if (!local)
			stackFrame.Module.SetErrorHandler(ErrorResponse.ExecuteHandler, TargetPath);
		else
			context.SetLocalErrorHandler(stackFrame, ErrorResponse.ExecuteHandler, TargetPath);
	}
}
