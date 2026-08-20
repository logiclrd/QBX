using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class StopStatement(CodeModel.Statements.StopStatement source) : Executable(source)
{
	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		// "In the QBX environment, STOP always returns an error level of 0 even
		// if you specify a different error level."
		context.SetExitCode(0);

		throw new BreakExecution() { IsExplicitBreak = true };
	}
}
