using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ResumeLineStatement(string target, CodeModel.Statements.Statement source)
	: JumpStatement(target, source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (TargetPath == null)
			throw new Exception("Internal error: Executing an unresolved ResumeLineStatement");

		if (stackFrame.IsHandlingError)
			throw new Resume() { GoTo = new GoTo(TargetPath, stackFrame) };
		else
			throw RuntimeException.ResumeWithoutError(Source);
	}
}
