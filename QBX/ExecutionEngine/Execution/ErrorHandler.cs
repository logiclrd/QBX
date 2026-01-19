using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution;

public class ErrorHandler
{
	public StackFrame? StackFrame;
	public ErrorResponse Response;
	public StatementPath? HandlerPath;
}
