using System;

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

			throw RuntimeException.TypeMismatch(blame);
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
			if (!left.Type.IsSingle)
				left = new ConvertToSingle(left);
			if (!right.Type.IsSingle)
				right = new ConvertToSingle(right);

			return new SingleDivision(left, right);
		}
		else
		{
			if (!left.Type.IsDouble)
				left = new ConvertToDouble(left);
			if (!right.Type.IsDouble)
				right = new ConvertToDouble(right);

			return new DoubleDivision(left, right);
		}
	}
}

public class IntegerDivision(IEvaluable left, IEvaluable right) : IEvaluable
{
	public static IEvaluable Construct(IEvaluable left, IEvaluable right)
	{
		// The backslash integer division operator: If both the operands
		// are INTEGER, then the division is INTEGER, otherwise it is LONG.
		if (left.Type.IsInteger && right.Type.IsInteger)
			return new IntegerDivision(left, right);
		else
		{
			if (!left.Type.IsLong)
				left = new ConvertToLong(left);
			if (!right.Type.IsLong)
				right = new ConvertToLong(right);

			return new LongDivision(left, right);
		}
	}

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
			throw RuntimeException.DivisionByZero(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		int quotient = leftValue.Value / rightValue.Value;

		return new IntegerVariable(unchecked((short)quotient));
	}
}

public class LongDivision(IEvaluable left, IEvaluable right) : IEvaluable
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
}

public class SingleDivision(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate(ExecutionContext context)
	{
		var leftValue = (SingleVariable)left.Evaluate(context);
		var rightValue = (SingleVariable)right.Evaluate(context);

		try
		{
			return new SingleVariable(leftValue.Value / rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class DoubleDivision(IEvaluable left, IEvaluable right) : IEvaluable
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
			return new DoubleVariable(leftValue.Value / rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class CurrencyDivision(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate(ExecutionContext context)
	{
		var leftValue = (CurrencyVariable)left.Evaluate(context);
		var rightValue = (CurrencyVariable)right.Evaluate(context);

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
}
