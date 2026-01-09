using System;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Operations;

public static class Division
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

		// Dividing two CURRENCY values produces a CURRENCY value.

		if (left.Type.IsCurrency && right.Type.IsCurrency)
			return new CurrencyDivision(left, right);

		// Otherwise, dividing always produces a floating-point result.
		// If the only types involved are INTEGER and/or SINGLE, then
		// the division is single-precision, otherwise it is
		// double-precision.

		if ((left.Type.IsInteger || left.Type.IsSingle)
		 && (right.Type.IsInteger || right.Type.IsSingle))
		{
			left = Conversion.Construct(left, PrimitiveDataType.Single);
			right = Conversion.Construct(right, PrimitiveDataType.Single);

			return new SingleDivision(left, right);
		}
		else
		{
			left = Conversion.Construct(left, PrimitiveDataType.Double);
			right = Conversion.Construct(right, PrimitiveDataType.Double);

			return new DoubleDivision(left, right);
		}
	}
}

public class IntegerDivision(IEvaluable left, IEvaluable right) : Expression
{
	public static IEvaluable Construct(IEvaluable left, IEvaluable right)
	{
		// The backslash integer division operator: If both the operands
		// are INTEGER, then the division is INTEGER, otherwise it is LONG.
		if (left.Type.IsInteger && right.Type.IsInteger)
			return new IntegerDivision(left, right);
		else
		{
			left = Conversion.Construct(left, PrimitiveDataType.Long);
			right = Conversion.Construct(right, PrimitiveDataType.Long);

			return new LongDivision(left, right);
		}
	}

	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (IntegerVariable)left.Evaluate(context, stackFrame);
		var rightValue = (IntegerVariable)right.Evaluate(context, stackFrame);

		if (rightValue.Value == 0)
			throw RuntimeException.DivisionByZero(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		int quotient = leftValue.Value / rightValue.Value;

		return new IntegerVariable(unchecked((short)quotient));
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (IntegerLiteralValue)left.EvaluateConstant();
		var rightValue = (IntegerLiteralValue)right.EvaluateConstant();

		if (rightValue.Value == 0)
			throw CompilerException.DivisionByZero(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		int quotient = leftValue.Value / rightValue.Value;

		return new IntegerLiteralValue(unchecked((short)quotient));
	}
}

public class LongDivision(IEvaluable left, IEvaluable right) : Expression
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public override DataType Type => DataType.Long;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (LongVariable)left.Evaluate(context, stackFrame);
		var rightValue = (LongVariable)right.Evaluate(context, stackFrame);

		if (rightValue.Value == 0)
			throw RuntimeException.DivisionByZero(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		try
		{
			return new LongVariable(leftValue.Value / rightValue.Value);
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
			return new LongLiteralValue(leftValue.Value / rightValue.Value);
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class SingleDivision(IEvaluable left, IEvaluable right) : Expression
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public override DataType Type => DataType.Single;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (SingleVariable)left.Evaluate(context, stackFrame);
		var rightValue = (SingleVariable)right.Evaluate(context, stackFrame);

		try
		{
			return new SingleVariable(leftValue.Value / rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (SingleLiteralValue)left.EvaluateConstant();
		var rightValue = (SingleLiteralValue)right.EvaluateConstant();

		try
		{
			return new SingleLiteralValue(leftValue.Value / rightValue.Value);
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class DoubleDivision(IEvaluable left, IEvaluable right) : Expression
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public override DataType Type => DataType.Double;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (DoubleVariable)left.Evaluate(context, stackFrame);
		var rightValue = (DoubleVariable)right.Evaluate(context, stackFrame);

		try
		{
			return new DoubleVariable(leftValue.Value / rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (DoubleLiteralValue)left.EvaluateConstant();
		var rightValue = (DoubleLiteralValue)right.EvaluateConstant();

		try
		{
			return new DoubleLiteralValue(leftValue.Value / rightValue.Value);
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class CurrencyDivision(IEvaluable left, IEvaluable right) : Expression
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public override DataType Type => DataType.Currency;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (CurrencyVariable)left.Evaluate(context, stackFrame);
		var rightValue = (CurrencyVariable)right.Evaluate(context, stackFrame);

		try
		{
			decimal value = leftValue.Value / rightValue.Value;

			if (!value.IsInCurrencyRange())
				throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);

			return new CurrencyVariable(value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (CurrencyLiteralValue)left.EvaluateConstant();
		var rightValue = (CurrencyLiteralValue)right.EvaluateConstant();

		try
		{
			decimal value = leftValue.Value / rightValue.Value;

			if (!value.IsInCurrencyRange())
				throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);

			return new CurrencyLiteralValue(value);
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}
