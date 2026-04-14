using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class WaitStatement(CodeModel.Statements.WaitStatement source) : Executable(source)
{
	public Evaluable? PortExpression;
	public Evaluable? AndExpression;
	public Evaluable? XOrExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (PortExpression == null)
			throw new Exception("OutStatement with no PortExpression");
		if (AndExpression == null)
			throw new Exception("OutStatement with no Dataxpression");

		int port = PortExpression.EvaluateAndCoerceToInt(context, stackFrame);
		byte mask = unchecked((byte)AndExpression.EvaluateAndCoerceToInt(context, stackFrame));
		byte flip = unchecked((byte)(XOrExpression?.EvaluateAndCoerceToInt(context, stackFrame) ?? 0));

		while (true)
		{
			int value = context.Machine.InPort(port);

			value ^= flip;
			value &= mask;

			if (value != 0)
				break;
		}
	}
}
