using QBX.ExecutionEngine.Execution;
using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class RestartExecutionFromLineStatement(Identifier startingLineNumber, CodeModel.Statements.RunStatement source) : JumpStatement(startingLineNumber, source)
{
	public override bool TargetIsInMainModule => true;

	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		if (TargetPath == null)
			throw RuntimeException.LabelNotDefined(source);

		throw new ReplaceRunningProgram(TargetPath);
	}
}
