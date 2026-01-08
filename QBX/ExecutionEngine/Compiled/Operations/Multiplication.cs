using System;

using QBX.ExecutionEngine.Compiled.Expressions;
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

	public LiteralValue EvaluateConstant()
	{
		var leftValue = (IntegerLiteralValue)left.EvaluateConstant();
		var rightValue = (IntegerLiteralValue)right.EvaluateConstant();

		int product = leftValue.Value * rightValue.Value;

		if ((product < short.MinValue) || (product > short.MaxValue))
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		return new IntegerLiteralValue(unchecked((short)product));
	}
}

public class LongMultiplication(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Long;

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

	public LiteralValue EvaluateConstant()
	{
		var leftValue = (LongLiteralValue)left.EvaluateConstant();
		var rightValue = (LongLiteralValue)right.EvaluateConstant();

		try
		{
			return new LongLiteralValue(leftValue.Value * rightValue.Value);
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class SingleMultiplication(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Single;

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

	public LiteralValue EvaluateConstant()
	{
		var leftValue = (SingleLiteralValue)left.EvaluateConstant();
		var rightValue = (SingleLiteralValue)right.EvaluateConstant();

		try
		{
			return new SingleLiteralValue(leftValue.Value * rightValue.Value);
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class DoubleMultiplication(IEvaluable left, IEvaluable right) : IEvaluable
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
			return new DoubleVariable(leftValue.Value * rightValue.Value);
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
			return new DoubleLiteralValue(leftValue.Value * rightValue.Value);
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class CurrencyMultiplication(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Currency;

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

	public LiteralValue EvaluateConstant()
	{
		var leftValue = (CurrencyLiteralValue)left.EvaluateConstant();
		var rightValue = (CurrencyLiteralValue)right.EvaluateConstant();

		try
		{
			return new CurrencyLiteralValue(leftValue.Value * rightValue.Value);
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}
