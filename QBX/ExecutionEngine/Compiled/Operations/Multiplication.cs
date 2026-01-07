using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Operations;

public static class Multiplication
{
	public static IEvaluable Construct(IEvaluable left, IEvaluable right)
	{
		// Remaining possibilities: String, Currency, Double, Single, Long, Integer

		if (left.Type.IsString || right.Type.IsString)
		{
			var blame = left.Type.IsString
				? right.SourceExpression?.Token
				: left.SourceExpression?.Token;

			throw CompilerException.TypeMismatch(blame);
		}

		// Remaining possibilities: Currency, Double, Single, Long, Integer

		if (left.Type.IsDouble || right.Type.IsDouble)
		{
			left = Conversion.Construct(left, PrimitiveDataType.Double);
			right = Conversion.Construct(right, PrimitiveDataType.Double);

			return new DoubleMultiplication(left, right);
		}

		// Remaining possibilities: Currency, Single, Long, Integer

		if ((left.Type.IsCurrency && right.Type.IsSingle)
		 || (right.Type.IsCurrency && left.Type.IsSingle))
		{
			left = Conversion.Construct(left, PrimitiveDataType.Double);
			right = Conversion.Construct(right, PrimitiveDataType.Double);

			return new DoubleMultiplication(left, right);
		}

		if (left.Type.IsCurrency || right.Type.IsCurrency)
		{
			left = Conversion.Construct(left, PrimitiveDataType.Currency);
			right = Conversion.Construct(right, PrimitiveDataType.Currency);

			return new CurrencyMultiplication(left, right);
		}

		// Remaining possibilities: Single, Long, Integer

		if (left.Type.IsSingle || right.Type.IsSingle)
		{
			left = Conversion.Construct(left, PrimitiveDataType.Single);
			right = Conversion.Construct(right, PrimitiveDataType.Single);

			return new SingleMultiplication(left, right);
		}

		// Remaining possibilities: Long, Integer

		if (left.Type.IsLong || right.Type.IsLong)
		{
			left = Conversion.Construct(left, PrimitiveDataType.Long);
			right = Conversion.Construct(right, PrimitiveDataType.Long);

			return new LongMultiplication(left, right);
		}

		// Remaining possibilities: Integer

		if (!left.Type.IsInteger || !right.Type.IsInteger)
			throw new Exception("Internal error: Expression should be of Integer type");

		return new IntegerMultiplication(left, right);
	}
}

public class IntegerMultiplication(IEvaluable left, IEvaluable right) : IEvaluable
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

		int product = leftValue.Value * rightValue.Value;

		if ((product < short.MinValue) || (product > short.MaxValue))
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		return new IntegerVariable(unchecked((short)product));
	}
}

public class LongMultiplication(IEvaluable left, IEvaluable right) : IEvaluable
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

		try
		{
			return new LongVariable(leftValue.Value * rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class SingleMultiplication(IEvaluable left, IEvaluable right) : IEvaluable
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
			return new SingleVariable(leftValue.Value * rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class DoubleMultiplication(IEvaluable left, IEvaluable right) : IEvaluable
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
			return new DoubleVariable(leftValue.Value * rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class CurrencyMultiplication(IEvaluable left, IEvaluable right) : IEvaluable
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
			return new CurrencyVariable(leftValue.Value * rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}
