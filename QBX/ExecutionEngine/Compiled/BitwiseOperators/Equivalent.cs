using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Compiled.Operations;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.BitwiseOperators;

public static class Equivalent
{
	public static Evaluable Construct(Evaluable left, Evaluable right)
	{
		if (!left.Type.IsNumeric)
			throw CompilerException.TypeMismatch(left.Source);
		if (!right.Type.IsNumeric)
			throw CompilerException.TypeMismatch(right.Source);

		if (left.Type.IsInteger && right.Type.IsInteger)
			return new IntegerEquivalent(left, right);
		else
		{
			left = Conversion.Construct(left, PrimitiveDataType.Long);
			right = Conversion.Construct(right, PrimitiveDataType.Long);

			return new LongEquivalent(left, right);
		}
	}
}

public class IntegerEquivalent(Evaluable left, Evaluable right) : BinaryExpression(left, right)
{
	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (IntegerVariable)left.Evaluate(context, stackFrame);
		var rightValue = (IntegerVariable)right.Evaluate(context, stackFrame);

		int result = ~(leftValue.Value ^ rightValue.Value);

		return new IntegerVariable(unchecked((short)result));
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (IntegerLiteralValue)left.EvaluateConstant();
		var rightValue = (IntegerLiteralValue)right.EvaluateConstant();

		int result = ~(leftValue.Value ^ rightValue.Value);

		return new IntegerLiteralValue(unchecked((short)result));
	}
}

public class LongEquivalent(Evaluable left, Evaluable right) : BinaryExpression(left, right)
{
	public override DataType Type => DataType.Long;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var leftValue = (LongVariable)left.Evaluate(context, stackFrame);
		var rightValue = (LongVariable)right.Evaluate(context, stackFrame);

		return new LongVariable(~(leftValue.Value ^ rightValue.Value));
	}

	public override LiteralValue EvaluateConstant()
	{
		var leftValue = (LongLiteralValue)left.EvaluateConstant();
		var rightValue = (LongLiteralValue)right.EvaluateConstant();

		return new LongLiteralValue(~(leftValue.Value ^ rightValue.Value));
	}
}
