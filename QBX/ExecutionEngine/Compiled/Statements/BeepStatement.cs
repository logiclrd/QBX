using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class BeepStatement(CodeModel.Statements.BeepStatement source) : Executable(source)
{
	public Evaluable? ExitCodeExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		context.Machine.DOS.Beep();
	}
}
