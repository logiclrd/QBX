using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class OutStatement(CodeModel.Statements.Statement source)
	: Executable(source)
{
	public Evaluable? PortExpression;
	public Evaluable? DataExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (PortExpression == null)
			throw new Exception("OutStatement with no PortExpression");
		if (DataExpression == null)
			throw new Exception("OutStatement with no Dataxpression");

		int port = PortExpression.EvaluateAndCoerceToInt(context, stackFrame);
		int data = DataExpression.EvaluateAndCoerceToInt(context, stackFrame);

		context.Machine.OutPort(port, unchecked((byte)data));
	}
}
