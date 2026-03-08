using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class BeepStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? ExitCodeExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		context.Machine.Speaker.ChangeSound(true, false, frequency: 1000, false, hold: TimeSpan.FromMilliseconds(200));
		context.Machine.Speaker.ChangeSound(false, false, frequency: 1000, false);
	}
}
