using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class OnErrorGoTo0Statement(bool local, CodeModel.Statements.Statement source) : Executable(source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (!local)
			stackFrame.Module.ClearErrorHandler(source);
		else
			context.ClearLocalErrorHandler(stackFrame, source);
	}
}
