using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Operations;

public static class Modulo
{
	public static IEvaluable Construct(IEvaluable left, IEvaluable right)
	{
		// The MOD integer division remainder operator: If both the operands
		// are INTEGER, then the division is INTEGER, otherwise it is LONG.
		if (left.Type.IsInteger && right.Type.IsInteger)
			return new IntegerModulo(left, right);
		else
		{
			if (!left.Type.IsLong)
				left = new ConvertToLong(left);
			if (!right.Type.IsLong)
				right = new ConvertToLong(right);

			return new LongModulo(left, right);
		}
	}
}

public class IntegerModulo(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate(ExecutionContext context)
	{
		var leftValue = (IntegerVariable)left.Evaluate(context);
		var rightValue = (IntegerVariable)right.Evaluate(context);

		if (rightValue.Value == 0)
			throw new RuntimeException(SourceExpression?.Token ?? SourceStatement?.FirstToken, "Division by zero");

		int remainder = leftValue.Value % rightValue.Value;

		return new IntegerVariable(unchecked((short)remainder));
	}
}

public class LongModulo(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate(ExecutionContext context)
	{
		var leftValue = (LongVariable)left.Evaluate(context);
		var rightValue = (LongVariable)right.Evaluate(context);

		if (rightValue.Value == 0)
			throw new RuntimeException(SourceExpression?.Token ?? SourceStatement?.FirstToken, "Division by zero");

		try
		{
			return new LongVariable(leftValue.Value % rightValue.Value);
		}
		catch (OverflowException)
		{
			throw new RuntimeException(SourceExpression?.Token ?? SourceStatement?.FirstToken, "Overflow");
		}
	}
}
