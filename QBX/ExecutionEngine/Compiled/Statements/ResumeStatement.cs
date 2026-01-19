using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ResumeStatement(bool retryStatement, CodeModel.Statements.Statement source) : Executable(source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (stackFrame.IsHandlingError)
			throw new Resume() { RetryStatement = retryStatement };
		else
			throw RuntimeException.ResumeWithoutError(Source);
	}
}
