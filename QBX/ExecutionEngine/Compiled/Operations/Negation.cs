using System;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Operations;

public static class Negation
{
	public static Evaluable Construct(Evaluable right)
	{
		if (!right.Type.IsNumeric)
			throw CompilerException.TypeMismatch(right.Source);

		if (right.Type.IsInteger)
			return new IntegerNegation(right);
		if (right.Type.IsLong)
			return new LongNegation(right);
		if (right.Type.IsSingle)
			return new SingleNegation(right);
		if (right.Type.IsDouble)
			return new DoubleNegation(right);
		if (right.Type.IsCurrency)
			return new CurrencyNegation(right);

		throw new Exception("Internal error: didn't match primitive data type");
	}
}

public class IntegerNegation(Evaluable right) : UnaryExpression(right)
{
	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var rightValue = (IntegerVariable)right.Evaluate(context, stackFrame);

		if (rightValue.Value == short.MinValue) // -MinValue is larger than MaxValue
			throw RuntimeException.Overflow(Source?.Token);

		return new IntegerVariable(unchecked((short)-rightValue.Value));
	}

	public override LiteralValue EvaluateConstant()
	{
		var rightValue = (IntegerLiteralValue)right.EvaluateConstant();

		if (rightValue.Value == short.MinValue) // -MinValue is larger than MaxValue
			throw CompilerException.Overflow(Source?.Token);

		return new IntegerLiteralValue(unchecked((short)-rightValue.Value));
	}
}

public class LongNegation(Evaluable right) : UnaryExpression(right)
{
	public override DataType Type => DataType.Long;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var rightValue = (LongVariable)right.Evaluate(context, stackFrame);

		if (rightValue.Value == int.MinValue) // -MinValue is larger than MaxValue
			throw RuntimeException.Overflow(Source?.Token);

		return new LongVariable(-rightValue.Value);
	}

	public override LiteralValue EvaluateConstant()
	{
		var rightValue = (LongLiteralValue)right.EvaluateConstant();

		if (rightValue.Value == int.MinValue) // -MinValue is larger than MaxValue
			throw CompilerException.Overflow(Source?.Token);

		return new LongLiteralValue(-rightValue.Value);
	}
}

public class SingleNegation(Evaluable right) : UnaryExpression(right)
{
	public override DataType Type => DataType.Single;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var rightValue = (SingleVariable)right.Evaluate(context, stackFrame);

		return new SingleVariable(-rightValue.Value);
	}

	public override LiteralValue EvaluateConstant()
	{
		var rightValue = (SingleLiteralValue)right.EvaluateConstant();

		return new SingleLiteralValue(-rightValue.Value);
	}
}

public class DoubleNegation(Evaluable right) : UnaryExpression(right)
{
	public override DataType Type => DataType.Double;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var rightValue = (DoubleVariable)right.Evaluate(context, stackFrame);

		return new DoubleVariable(-rightValue.Value);
	}

	public override LiteralValue EvaluateConstant()
	{
		var rightValue = (DoubleLiteralValue)right.EvaluateConstant();

		return new DoubleLiteralValue(-rightValue.Value);
	}
}

public class CurrencyNegation(Evaluable right) : UnaryExpression(right)
{
	public override DataType Type => DataType.Currency;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var rightValue = (CurrencyVariable)right.Evaluate(context, stackFrame);

		return new CurrencyVariable(-rightValue.Value);
	}

	public override LiteralValue EvaluateConstant()
	{
		var rightValue = (CurrencyLiteralValue)right.EvaluateConstant();

		return new CurrencyLiteralValue(-rightValue.Value);
	}
}
