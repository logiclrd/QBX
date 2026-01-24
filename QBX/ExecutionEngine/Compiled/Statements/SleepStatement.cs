using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using System;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class SleepStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? SecondsExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		int seconds = 0;

		if (SecondsExpression != null)
			seconds = SecondsExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if (seconds > 0)
			context.Machine.Keyboard.WaitForNewInput(timeout: TimeSpan.FromSeconds(seconds));
		else
			context.Machine.Keyboard.WaitForNewInput();
	}
}
