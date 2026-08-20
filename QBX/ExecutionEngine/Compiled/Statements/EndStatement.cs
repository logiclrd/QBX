using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class EndStatement(CodeModel.Statements.EndStatement source) : Executable(source)
{
	public Evaluable? ExitCodeExpression;

	public bool ExitAutoRunToSystem;

	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		int exitCode = ExitCodeExpression?.EvaluateAndCoerceToInt(context, stackFrame) ?? 0;

		context.SetExitCode(exitCode, ExitAutoRunToSystem);

		throw new EndProgram();
	}
}
