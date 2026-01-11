using System;
using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Operations;

public static class Modulo
{
	public static Evaluable Construct(Evaluable left, Evaluable right)
	{
		// The MOD integer division remainder operator: If both the operands
		// are INTEGER, then the division is INTEGER, otherwise it is LONG.
		if (left.Type.IsInteger && right.Type.IsInteger)
			return new IntegerModulo(left, right);
		else
		{
			left = Conversion.Construct(left, PrimitiveDataType.Long);
			right = Conversion.Construct(right, PrimitiveDataType.Long);

			return new LongModulo(left, right);
		}
	}
}

public class IntegerModulo(Evaluable left, Evaluable right) : BinaryExpression(left, right)
{
	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (IntegerVariable)left.Evaluate(context, stackFrame);
		var rightValue = (IntegerVariable)right.Evaluate(context, stackFrame);

		if (rightValue.Value == 0)
			throw RuntimeException.DivisionByZero(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		int remainder = leftValue.Value % rightValue.Value;

		return new IntegerVariable(unchecked((short)remainder));
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (IntegerLiteralValue)left.EvaluateConstant();
		var rightValue = (IntegerLiteralValue)right.EvaluateConstant();

		if (rightValue.Value == 0)
			throw CompilerException.DivisionByZero(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		int remainder = leftValue.Value % rightValue.Value;

		return new IntegerLiteralValue(unchecked((short)remainder));
	}
}

public class LongModulo(Evaluable left, Evaluable right) : BinaryExpression(left, right)
{
	public override DataType Type => DataType.Long;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (LongVariable)left.Evaluate(context, stackFrame);
		var rightValue = (LongVariable)right.Evaluate(context, stackFrame);

		if (rightValue.Value == 0)
			throw RuntimeException.DivisionByZero(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		try
		{
			return new LongVariable(leftValue.Value % rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (LongLiteralValue)left.EvaluateConstant();
		var rightValue = (LongLiteralValue)right.EvaluateConstant();

		if (rightValue.Value == 0)
			throw CompilerException.DivisionByZero(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		try
		{
			return new LongLiteralValue(leftValue.Value % rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}
