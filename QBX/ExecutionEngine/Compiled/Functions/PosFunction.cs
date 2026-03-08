using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class PosFunction : Function
{
	public Evaluable? DummyExpression;

	protected override void SetArgument(int index, Evaluable value)
	{
		DummyExpression = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref DummyExpression);
	}

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (DummyExpression == null)
			throw new Exception("PosFunction with no DummyExpression");

		// Per documentation, this expression isn't actually used for anything,
		// but it is required and it is evaluated.
		DummyExpression.Evaluate(context, stackFrame);

		return new IntegerVariable((short)(context.VisualLibrary.CursorX + 1));
	}
}
