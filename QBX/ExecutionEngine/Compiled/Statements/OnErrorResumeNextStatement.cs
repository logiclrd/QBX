using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class OnErrorResumeNextStatement(bool local, CodeModel.Statements.OnErrorStatement source) : Executable(source)
{
	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		if (!local)
			stackFrame.Module.SetErrorHandler(ErrorResponse.SkipStatement);
		else
			context.SetLocalErrorHandler(stackFrame, ErrorResponse.SkipStatement);
	}
}
