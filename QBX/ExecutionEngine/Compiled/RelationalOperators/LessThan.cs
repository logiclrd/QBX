using System;
using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Compiled.Operations;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.RelationalOperators;

public static class LessThan
{
	public static IEvaluable Construct(IEvaluable left, IEvaluable right)
	{
		if (left.Type.IsString && right.Type.IsString)
			return new StringLessThan(left, right);

		if (left.Type.IsString || right.Type.IsString)
			throw CompilerException.TypeMismatch(right.SourceExpression?.Token);

		if (left.Type.IsDouble || right.Type.IsDouble)
		{
			left = Conversion.Construct(left, PrimitiveDataType.Double);
			right = Conversion.Construct(right, PrimitiveDataType.Double);

			return new DoubleLessThan(left, right);
		}

		if (left.Type.IsSingle || right.Type.IsSingle)
		{
			left = Conversion.Construct(left, PrimitiveDataType.Single);
			right = Conversion.Construct(right, PrimitiveDataType.Single);

			return new SingleLessThan(left, right);
		}

		if (left.Type.IsCurrency || right.Type.IsCurrency)
		{
			left = Conversion.Construct(left, PrimitiveDataType.Currency);
			right = Conversion.Construct(right, PrimitiveDataType.Currency);

			return new CurrencyLessThan(left, right);
		}

		if (left.Type.IsLong || right.Type.IsLong)
		{
			left = Conversion.Construct(left, PrimitiveDataType.Long);
			right = Conversion.Construct(right, PrimitiveDataType.Long);

			return new LongLessThan(left, right);
		}

		if (!left.Type.IsInteger || !right.Type.IsInteger)
			throw new Exception("Internal error: left and right expressions should both be of type Integer");

		return new IntegerLessThan(left, right);
	}
}

public class IntegerLessThan(IEvaluable left, IEvaluable right) : Expression
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (IntegerVariable)left.Evaluate(context, stackFrame);
		var rightValue = (IntegerVariable)right.Evaluate(context, stackFrame);

		bool result = leftValue.Value < rightValue.Value;

		return new IntegerVariable(result ? (short)-1 : (short)0);
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (IntegerLiteralValue)left.EvaluateConstant();
		var rightValue = (IntegerLiteralValue)right.EvaluateConstant();

		bool result = leftValue.Value < rightValue.Value;

		return new IntegerLiteralValue(result ? (short)-1 : (short)0);
	}
}

public class LongLessThan(IEvaluable left, IEvaluable right) : Expression
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (LongVariable)left.Evaluate(context, stackFrame);
		var rightValue = (LongVariable)right.Evaluate(context, stackFrame);

		bool result = leftValue.Value < rightValue.Value;

		return new IntegerVariable(result ? (short)-1 : (short)0);
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (LongLiteralValue)left.EvaluateConstant();
		var rightValue = (LongLiteralValue)right.EvaluateConstant();

		bool result = leftValue.Value < rightValue.Value;

		return new IntegerLiteralValue(result ? (short)-1 : (short)0);
	}
}

public class SingleLessThan(IEvaluable left, IEvaluable right) : Expression
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (SingleVariable)left.Evaluate(context, stackFrame);
		var rightValue = (SingleVariable)right.Evaluate(context, stackFrame);

		bool result = leftValue.Value < rightValue.Value;

		return new IntegerVariable(result ? (short)-1 : (short)0);
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (SingleLiteralValue)left.EvaluateConstant();
		var rightValue = (SingleLiteralValue)right.EvaluateConstant();

		bool result = leftValue.Value < rightValue.Value;

		return new IntegerLiteralValue(result ? (short)-1 : (short)0);
	}
}

public class DoubleLessThan(IEvaluable left, IEvaluable right) : Expression
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (DoubleVariable)left.Evaluate(context, stackFrame);
		var rightValue = (DoubleVariable)right.Evaluate(context, stackFrame);

		bool result = leftValue.Value < rightValue.Value;

		return new IntegerVariable(result ? (short)-1 : (short)0);
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (DoubleLiteralValue)left.EvaluateConstant();
		var rightValue = (DoubleLiteralValue)right.EvaluateConstant();

		bool result = leftValue.Value < rightValue.Value;

		return new IntegerLiteralValue(result ? (short)-1 : (short)0);
	}
}

public class CurrencyLessThan(IEvaluable left, IEvaluable right) : Expression
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (CurrencyVariable)left.Evaluate(context, stackFrame);
		var rightValue = (CurrencyVariable)right.Evaluate(context, stackFrame);

		bool result = leftValue.Value < rightValue.Value;

		return new IntegerVariable(result ? (short)-1 : (short)0);
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (CurrencyLiteralValue)left.EvaluateConstant();
		var rightValue = (CurrencyLiteralValue)right.EvaluateConstant();

		bool result = leftValue.Value < rightValue.Value;

		return new IntegerLiteralValue(result ? (short)-1 : (short)0);
	}
}

public class StringLessThan(IEvaluable left, IEvaluable right) : Expression
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (StringVariable)left.Evaluate(context, stackFrame);
		var rightValue = (StringVariable)right.Evaluate(context, stackFrame);

		bool result = leftValue.Value.CompareTo(rightValue.Value, StringComparison.Ordinal) < 0;

		return new IntegerVariable(result ? (short)-1 : (short)0);
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (StringLiteralValue)left.EvaluateConstant();
		var rightValue = (StringLiteralValue)right.EvaluateConstant();

		bool result = leftValue.Value.CompareTo(rightValue.Value, StringComparison.Ordinal) < 0;

		return new IntegerLiteralValue(result ? (short)-1 : (short)0);
	}
}

