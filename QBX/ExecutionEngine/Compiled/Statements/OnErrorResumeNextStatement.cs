using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class OnErrorResumeNextStatement(bool local, CodeModel.Statements.Statement source) : Executable(source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (!local)
			context.SetErrorHandler(ErrorResponse.SkipStatement);
		else
			context.SetLocalErrorHandler(stackFrame, ErrorResponse.SkipStatement);
	}
}
