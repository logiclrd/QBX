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

		var portValue = PortExpression.Evaluate(context, stackFrame);
		var dataValue = DataExpression.Evaluate(context, stackFrame);

		int port = portValue.CoerceToInt();
		int data = dataValue.CoerceToInt();

		context.Machine.OutPort(port, unchecked((byte)data));
	}
}
