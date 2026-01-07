using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Operations;

public static class Addition
{
	public static IEvaluable Construct(IEvaluable left, IEvaluable right)
	{
		// Remaining possibilities: String, Currency, Double, Single, Long, Integer

		if (left.Type.IsString || right.Type.IsString)
		{
			left = Conversion.Construct(left, PrimitiveDataType.String);
			right = Conversion.Construct(right, PrimitiveDataType.String);

			return new StringAddition(left, right);
		}

		// Remaining possibilities: Currency, Double, Single, Long, Integer

		if (left.Type.IsDouble || right.Type.IsDouble)
		{
			left = Conversion.Construct(left, PrimitiveDataType.Double);
			right = Conversion.Construct(right, PrimitiveDataType.Double);

			return new DoubleAddition(left, right);
		}

		// Remaining possibilities: Currency, Single, Long, Integer

		if ((left.Type.IsCurrency && right.Type.IsSingle)
		 || (right.Type.IsCurrency && left.Type.IsSingle))
		{
			left = new ConvertToDouble(left);
			right = new ConvertToDouble(right);

			return new DoubleAddition(left, right);
		}

		if (left.Type.IsCurrency || right.Type.IsCurrency)
		{
			left = Conversion.Construct(left, PrimitiveDataType.Currency);
			right = Conversion.Construct(right, PrimitiveDataType.Currency);

			return new CurrencyAddition(left, right);
		}

		// Remaining possibilities: Single, Long, Integer

		if (left.Type.IsSingle || right.Type.IsSingle)
		{
			left = Conversion.Construct(left, PrimitiveDataType.Single);
			right = Conversion.Construct(right, PrimitiveDataType.Single);

			return new SingleAddition(left, right);
		}

		// Remaining possibilities: Long, Integer

		if (left.Type.IsLong || right.Type.IsLong)
		{
			left = Conversion.Construct(left, PrimitiveDataType.Long);
			right = Conversion.Construct(right, PrimitiveDataType.Long);

			return new LongAddition(left, right);
		}

		// Remaining possibilities: Integer

		if (!left.Type.IsInteger || !right.Type.IsInteger)
			throw new Exception("Internal error: Expression should be of Integer type");

		return new IntegerAddition(left, right);
	}
}

public class IntegerAddition(IEvaluable left, IEvaluable right) : IEvaluable
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

		int sum = leftValue.Value + rightValue.Value;

		if ((sum < short.MinValue) || (sum > short.MaxValue))
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		return new IntegerVariable(unchecked((short)sum));
	}
}

public class LongAddition(IEvaluable left, IEvaluable right) : IEvaluable
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
			return new LongVariable(leftValue.Value + rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class SingleAddition(IEvaluable left, IEvaluable right) : IEvaluable
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
			return new SingleVariable(leftValue.Value + rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class DoubleAddition(IEvaluable left, IEvaluable right) : IEvaluable
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
			return new DoubleVariable(leftValue.Value + rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class CurrencyAddition(IEvaluable left, IEvaluable right) : IEvaluable
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
			return new CurrencyVariable(leftValue.Value + rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class StringAddition(IEvaluable left, IEvaluable right) : IEvaluable
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.String;

	public Variable Evaluate(ExecutionContext context)
	{
		var leftValue = (StringVariable)left.Evaluate(context);
		var rightValue = (StringVariable)right.Evaluate(context);

		try
		{
			return new StringVariable(leftValue.Value + rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}
