using System;
using QBX.ExecutionEngine.Compiled.Expressions;
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

			throw CompilerException.TypeMismatch(blame);
		}

		// Exponentiating a CURRENCY with an INTEGER or a LONG
		// evaluates to a CURRENCY.

		if (left.Type.IsCurrency
		 && (right.Type.IsInteger || right.Type.IsLong))
		{
			right = Conversion.Construct(right, PrimitiveDataType.Long);

			return new CurrencyExponentiation(left, right);
		}

		// Otherwise, exponentiation always evaluates to DOUBLE.

		left = Conversion.Construct(left, PrimitiveDataType.Double);
		right = Conversion.Construct(right, PrimitiveDataType.Double);

		return new DoubleExponentiation(left, right);
	}
}

public class CurrencyExponentiation(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Currency;

	static decimal CalculateResult(decimal baseValue, int exponentValue)
	{
		decimal result = 1;

		int bits = exponentValue;

		while (bits != 0)
		{
			if ((bits & 1) != 0)
				result *= baseValue;

			bits >>= 1;
			baseValue *= baseValue;
		}

		return result;
	}

	public Variable Evaluate(ExecutionContext context)
	{
		var leftValue = (CurrencyVariable)left.Evaluate(context);
		var rightValue = (LongVariable)right.Evaluate(context);

		try
		{
			decimal result = CalculateResult(leftValue.Value, rightValue.Value);

			if (!result.IsInCurrencyRange())
				throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);

			return new CurrencyVariable(result);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}

	public LiteralValue EvaluateConstant()
	{
		var leftValue = (CurrencyLiteralValue)left.EvaluateConstant();
		var rightValue = (LongLiteralValue)right.EvaluateConstant();

		try
		{
			decimal result = CalculateResult(leftValue.Value, rightValue.Value);

			if (!result.IsInCurrencyRange())
				throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);

			return new CurrencyLiteralValue(result);
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class DoubleExponentiation(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Double;

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

	public LiteralValue EvaluateConstant()
	{
		var leftValue = (DoubleLiteralValue)left.EvaluateConstant();
		var rightValue = (DoubleLiteralValue)right.EvaluateConstant();

		try
		{
			return new DoubleLiteralValue(Math.Pow(leftValue.Value, rightValue.Value));
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}
