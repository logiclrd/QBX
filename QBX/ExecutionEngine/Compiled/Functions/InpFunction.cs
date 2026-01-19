using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class InpFunction : Function
{
	public Evaluable? PortExpression;

	protected override void SetArgument(int index, Evaluable value)
	{
		PortExpression = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref PortExpression);
	}

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (PortExpression == null)
			throw new Exception("InpFunction with no PortExpression");

		var portValue = PortExpression.Evaluate(context, stackFrame);

		return new IntegerVariable(context.Machine.InPort(
			portValue.CoerceToInt(context: PortExpression)));
	}
}
