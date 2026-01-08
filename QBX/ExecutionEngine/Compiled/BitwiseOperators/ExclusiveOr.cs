using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Compiled.Operations;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using System;

namespace QBX.ExecutionEngine.Compiled.BitwiseOperators;

public static class ExclusiveOr
{
	public static IEvaluable Construct(IEvaluable left, IEvaluable right)
	{
		if (left.Type.IsString)
			throw CompilerException.TypeMismatch(left.SourceExpression?.Token);
		if (right.Type.IsString)
			throw CompilerException.TypeMismatch(right.SourceExpression?.Token);

		if (left.Type.IsInteger && right.Type.IsInteger)
			return new IntegerExclusiveOr(left, right);
		else
		{
			left = Conversion.Construct(left, PrimitiveDataType.Long);
			right = Conversion.Construct(right, PrimitiveDataType.Long);

			return new LongExclusiveOr(left, right);
		}
	}
}

public class IntegerExclusiveOr(IEvaluable left, IEvaluable right) : IEvaluable
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

		int result = leftValue.Value ^ rightValue.Value;

		return new IntegerVariable(unchecked((short)result));
	}

	public LiteralValue EvaluateConstant()
	{
		var leftValue = (IntegerLiteralValue)left.EvaluateConstant();
		var rightValue = (IntegerLiteralValue)right.EvaluateConstant();

		int result = leftValue.Value ^ rightValue.Value;

		return new IntegerLiteralValue(unchecked((short)result));
	}
}

public class LongExclusiveOr(IEvaluable left, IEvaluable right) : IEvaluable
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

		return new LongVariable(leftValue.Value ^ rightValue.Value);
	}

	public LiteralValue EvaluateConstant()
	{
		var leftValue = (LongLiteralValue)left.EvaluateConstant();
		var rightValue = (LongLiteralValue)right.EvaluateConstant();

		return new LongLiteralValue(leftValue.Value ^ rightValue.Value);
	}
}
