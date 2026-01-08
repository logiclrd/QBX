using System;
using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Operations;

public static class Not
{
	public static IEvaluable Construct(IEvaluable right)
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

public class IntegerNot(IEvaluable right) : IEvaluable
{
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate(ExecutionContext context)
	{
		var rightValue = (IntegerVariable)right.Evaluate(context);

		return new IntegerVariable(unchecked((short)~rightValue.Value));
	}

	public LiteralValue EvaluateConstant()
	{
		var rightValue = (IntegerLiteralValue)right.EvaluateConstant();

		return new IntegerLiteralValue(unchecked((short)~rightValue.Value));
	}
}

public class LongNot(IEvaluable right) : IEvaluable
{
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Long;

	public Variable Evaluate(ExecutionContext context)
	{
		var rightValue = (LongVariable)right.Evaluate(context);

		return new LongVariable(~rightValue.Value);
	}

	public LiteralValue EvaluateConstant()
	{
		var rightValue = (LongLiteralValue)right.EvaluateConstant();

		return new LongLiteralValue(~rightValue.Value);
	}
}
