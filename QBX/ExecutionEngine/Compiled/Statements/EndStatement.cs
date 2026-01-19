using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class EndStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? ExitCodeExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		int exitCode = ExitCodeExpression?.EvaluateAndCoerceToInt(context, stackFrame) ?? 0;

		context.SetExitCode(exitCode);

		throw new EndProgram();
	}
}
