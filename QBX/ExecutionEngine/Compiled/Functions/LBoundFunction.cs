using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class LBoundFunction : Function
{
	public Evaluable? ArrayExpression;
	public Evaluable? DimensionExpression;

	protected override int MinArgumentCount => 1;
	protected override int MaxArgumentCount => 2;

	protected override void SetArgument(int index, Evaluable value)
	{
		switch (index)
		{
			case 0:
				if (!value.Type.IsArray)
					throw CompilerException.TypeMismatch(value.Source);

				ArrayExpression = value;
				break;
			case 1:
				if (!value.Type.IsNumeric)
					throw CompilerException.TypeMismatch(value.Source);

				DimensionExpression = value;
				break;
		}
	}

	public override void CollapseConstantSubexpressions()
	{
		ArrayExpression?.CollapseConstantSubexpressions();
		CollapseConstantExpression(ref DimensionExpression);
	}

	public override DataType Type => DataType.String;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (ArrayExpression == null)
			throw new Exception("LBoundFunction with no ArrayExpression");

		var arrayVariable = (ArrayVariable)ArrayExpression.Evaluate(context, stackFrame);

		var array = arrayVariable.Array;

		if (array.IsUninitialized)
			throw new Exception("Internal error: array is uninitialized");

		int dimension = 1;

		if (DimensionExpression != null)
			dimension = DimensionExpression.EvaluateAndCoerceToInt(context, stackFrame) - 1;

		if ((dimension < 1) || (dimension > array.Subscripts.Dimensions))
			throw RuntimeException.SubscriptOutOfRange(Source);

		int bound = array.Subscripts[dimension - 1].LowerBound;

		try
		{
			return new IntegerVariable((short)bound);
		}
		catch (OverflowException)
		{
			// Should never happen.
			throw RuntimeException.Overflow(Source);
		}
	}
}
