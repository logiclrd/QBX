using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Compiled.Operations;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using System;

namespace QBX.ExecutionEngine.Compiled.BitwiseOperators;

public static class Or
{
	public static IEvaluable Construct(IEvaluable left, IEvaluable right)
	{
		if (left.Type.IsString)
			throw CompilerException.TypeMismatch(left.SourceExpression?.Token);
		if (right.Type.IsString)
			throw CompilerException.TypeMismatch(right.SourceExpression?.Token);

		if (left.Type.IsInteger && right.Type.IsInteger)
			return new IntegerOr(left, right);
		else
		{
			left = Conversion.Construct(left, PrimitiveDataType.Long);
			right = Conversion.Construct(right, PrimitiveDataType.Long);

			return new LongOr(left, right);
		}
	}
}

public class IntegerOr(IEvaluable left, IEvaluable right) : Expression
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (IntegerVariable)left.Evaluate(context, stackFrame);
		var rightValue = (IntegerVariable)right.Evaluate(context, stackFrame);

		int result = leftValue.Value | rightValue.Value;

		return new IntegerVariable(unchecked((short)result));
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (IntegerLiteralValue)left.EvaluateConstant();
		var rightValue = (IntegerLiteralValue)right.EvaluateConstant();

		int result = leftValue.Value | rightValue.Value;

		return new IntegerLiteralValue(unchecked((short)result));
	}
}

public class LongOr(IEvaluable left, IEvaluable right) : Expression
{
	public IEvaluable Left => left;
	public IEvaluable Right => right;

	public override DataType Type => DataType.Long;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (LongVariable)left.Evaluate(context, stackFrame);
		var rightValue = (LongVariable)right.Evaluate(context, stackFrame);

		return new LongVariable(leftValue.Value | rightValue.Value);
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (LongLiteralValue)left.EvaluateConstant();
		var rightValue = (LongLiteralValue)right.EvaluateConstant();

		return new LongLiteralValue(leftValue.Value | rightValue.Value);
	}
}
