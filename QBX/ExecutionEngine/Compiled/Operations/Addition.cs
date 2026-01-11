using System;
using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Operations;

public static class Addition
{
	public static Evaluable Construct(Evaluable left, Evaluable right)
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

public class IntegerAddition(Evaluable left, Evaluable right) : BinaryExpression(left, right)
{
	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (IntegerVariable)left.Evaluate(context, stackFrame);
		var rightValue = (IntegerVariable)right.Evaluate(context, stackFrame);

		int sum = leftValue.Value + rightValue.Value;

		if ((sum < short.MinValue) || (sum > short.MaxValue))
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		return new IntegerVariable(unchecked((short)sum));
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (IntegerLiteralValue)left.EvaluateConstant();
		var rightValue = (IntegerLiteralValue)right.EvaluateConstant();

		int sum = leftValue.Value + rightValue.Value;

		if ((sum < short.MinValue) || (sum > short.MaxValue))
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		return new IntegerLiteralValue(unchecked((short)sum));
	}
}

public class LongAddition(Evaluable left, Evaluable right) : BinaryExpression(left, right)
{
	public override DataType Type => DataType.Long;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (LongVariable)left.Evaluate(context, stackFrame);
		var rightValue = (LongVariable)right.Evaluate(context, stackFrame);

		try
		{
			return new LongVariable(leftValue.Value + rightValue.Value);
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

		try
		{
			return new LongLiteralValue(leftValue.Value + rightValue.Value);
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class SingleAddition(Evaluable left, Evaluable right) : BinaryExpression(left, right)
{
	public override DataType Type => DataType.Single;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (SingleVariable)left.Evaluate(context, stackFrame);
		var rightValue = (SingleVariable)right.Evaluate(context, stackFrame);

		try
		{
			return new SingleVariable(leftValue.Value + rightValue.Value);
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
			return new SingleLiteralValue(leftValue.Value + rightValue.Value);
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class DoubleAddition(Evaluable left, Evaluable right) : BinaryExpression(left, right)
{
	public override DataType Type => DataType.Double;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (DoubleVariable)left.Evaluate(context, stackFrame);
		var rightValue = (DoubleVariable)right.Evaluate(context, stackFrame);

		try
		{
			return new DoubleVariable(leftValue.Value + rightValue.Value);
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
			return new DoubleLiteralValue(leftValue.Value + rightValue.Value);
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class CurrencyAddition(Evaluable left, Evaluable right) : BinaryExpression(left, right)
{
	public override DataType Type => DataType.Currency;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (CurrencyVariable)left.Evaluate(context, stackFrame);
		var rightValue = (CurrencyVariable)right.Evaluate(context, stackFrame);

		try
		{
			return new CurrencyVariable(leftValue.Value + rightValue.Value);
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
			return new CurrencyLiteralValue(leftValue.Value + rightValue.Value);
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}

public class StringAddition(Evaluable left, Evaluable right) : BinaryExpression(left, right)
{
	public override DataType Type => DataType.String;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (StringVariable)left.Evaluate(context, stackFrame);
		var rightValue = (StringVariable)right.Evaluate(context, stackFrame);

		try
		{
			return new StringVariable(leftValue.Value + rightValue.Value);
		}
		catch (OverflowException)
		{
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (StringLiteralValue)left.EvaluateConstant();
		var rightValue = (StringLiteralValue)right.EvaluateConstant();

		try
		{
			return new StringLiteralValue(leftValue.Value + rightValue.Value);
		}
		catch (OverflowException)
		{
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);
		}
	}
}
