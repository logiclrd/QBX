using System;
using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Operations;

public static class Not
{
	public static Evaluable Construct(Evaluable right)
	{
		if (right.Type.IsString)
			throw CompilerException.TypeMismatch(right.SourceExpression?.Token);

		if (right.Type.IsInteger)
			return new IntegerNot(right);
		else
		{
			right = Conversion.Construct(right, PrimitiveDataType.Long);

			return new LongNot(right);
		}
	}
}

public class IntegerNot(Evaluable right) : Evaluable
{
	public Evaluable Right => right;

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var rightValue = (IntegerVariable)right.Evaluate(context, stackFrame);

		return new IntegerVariable(unchecked((short)~rightValue.Value));
	}

	public override LiteralValue EvaluateConstant()
	{
		var rightValue = (IntegerLiteralValue)right.EvaluateConstant();

		return new IntegerLiteralValue(unchecked((short)~rightValue.Value));
	}
}

public class LongNot(Evaluable right) : Evaluable
{
	public Evaluable Right => right;

	public override DataType Type => DataType.Long;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var rightValue = (LongVariable)right.Evaluate(context, stackFrame);

		return new LongVariable(~rightValue.Value);
	}

	public override LiteralValue EvaluateConstant()
	{
		var rightValue = (LongLiteralValue)right.EvaluateConstant();

		return new LongLiteralValue(~rightValue.Value);
	}
}
