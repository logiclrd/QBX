using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Operations;

public static class Exponentiation
{
	public static IEvaluable Construct(IEvaluable left, IEvaluable right)
	{
		if (left.Type.IsString || right.Type.IsString)
		{
			var blame = left.Type.IsString
				? right.SourceExpression?.Token
				: left.SourceExpression?.Token;

			throw RuntimeException.TypeMismatch(blame);
		}

		// Exponentiating a CURRENCY with an INTEGER or a LONG
		// evaluates to a CURRENCY.

		if (left.Type.IsCurrency
		 && (right.Type.IsInteger || right.Type.IsLong))
		{
			if (!right.Type.IsLong)
				right = new ConvertToLong(right);

			return new CurrencyExponentiation(left, right);
		}

		// Otherwise, exponentiation always evaluates to DOUBLE.

		if (!left.Type.IsDouble)
			left = new ConvertToDouble(left);
		if (!right.Type.IsDouble)
			right = new ConvertToDouble(right);

		return new DoubleExponentiation(left, right);
	}
}

public class CurrencyExponentiation(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate(ExecutionContext context)
	{
		var leftValue = (CurrencyVariable)left.Evaluate(context);
		var rightValue = (LongVariable)right.Evaluate(context);

		try
		{
			decimal result = 1;

			decimal baseValue = leftValue.Value;
			int bits = rightValue.Value;

			while (bits != 0)
			{
				if ((bits & 1) != 0)
					result *= baseValue;

				bits >>= 1;
				baseValue *= baseValue;
			}

			if (!result.IsInCurrencyRange())
				throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);

			return new CurrencyVariable(result);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class DoubleExponentiation(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate(ExecutionContext context)
	{
		var leftValue = (DoubleVariable)left.Evaluate(context);
		var rightValue = (DoubleVariable)right.Evaluate(context);

		try
		{
			return new DoubleVariable(Math.Pow(leftValue.Value, rightValue.Value));
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}
