using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ResumeStatement(bool retryStatement, CodeModel.Statements.ResumeStatement source) : Executable(source)
{
	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		if (stackFrame.IsHandlingError)
			throw new Resume() { RetryStatement = retryStatement };
		else
			throw RuntimeException.ResumeWithoutError(Source);
	}
}
