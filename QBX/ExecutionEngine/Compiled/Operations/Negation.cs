using System;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Operations;

public static class Negation
{
	public static IEvaluable Construct(IEvaluable right)
	{
		if (right.Type.IsString)
			throw CompilerException.TypeMismatch(right.SourceExpression?.Token);

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

public class IntegerNegation(IEvaluable right) : IEvaluable
{
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate(ExecutionContext context)
	{
		var rightValue = (IntegerVariable)right.Evaluate(context);

		if (rightValue.Value == short.MinValue) // -MinValue is larger than MaxValue
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		return new IntegerVariable(unchecked((short)-rightValue.Value));
	}

	public LiteralValue EvaluateConstant()
	{
		var rightValue = (IntegerLiteralValue)right.EvaluateConstant();

		if (rightValue.Value == short.MinValue) // -MinValue is larger than MaxValue
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		return new IntegerLiteralValue(unchecked((short)-rightValue.Value));
	}
}

public class LongNegation(IEvaluable right) : IEvaluable
{
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Long;

	public Variable Evaluate(ExecutionContext context)
	{
		var rightValue = (LongVariable)right.Evaluate(context);

		if (rightValue.Value == int.MinValue) // -MinValue is larger than MaxValue
			throw RuntimeException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		return new LongVariable(-rightValue.Value);
	}

	public LiteralValue EvaluateConstant()
	{
		var rightValue = (LongLiteralValue)right.EvaluateConstant();

		if (rightValue.Value == int.MinValue) // -MinValue is larger than MaxValue
			throw CompilerException.Overflow(SourceExpression?.Token ?? SourceStatement?.FirstToken);

		return new LongLiteralValue(-rightValue.Value);
	}
}

public class SingleNegation(IEvaluable right) : IEvaluable
{
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Single;

	public Variable Evaluate(ExecutionContext context)
	{
		var rightValue = (SingleVariable)right.Evaluate(context);

		return new SingleVariable(-rightValue.Value);
	}

	public LiteralValue EvaluateConstant()
	{
		var rightValue = (SingleLiteralValue)right.EvaluateConstant();

		return new SingleLiteralValue(-rightValue.Value);
	}
}

public class DoubleNegation(IEvaluable right) : IEvaluable
{
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Double;

	public Variable Evaluate(ExecutionContext context)
	{
		var rightValue = (DoubleVariable)right.Evaluate(context);

		return new DoubleVariable(-rightValue.Value);
	}

	public LiteralValue EvaluateConstant()
	{
		var rightValue = (DoubleLiteralValue)right.EvaluateConstant();

		return new DoubleLiteralValue(-rightValue.Value);
	}
}

public class CurrencyNegation(IEvaluable right) : IEvaluable
{
	public IEvaluable Right => right;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Currency;

	public Variable Evaluate(ExecutionContext context)
	{
		var rightValue = (CurrencyVariable)right.Evaluate(context);

		return new CurrencyVariable(-rightValue.Value);
	}

	public LiteralValue EvaluateConstant()
	{
		var rightValue = (CurrencyLiteralValue)right.EvaluateConstant();

		return new CurrencyLiteralValue(-rightValue.Value);
	}
}
