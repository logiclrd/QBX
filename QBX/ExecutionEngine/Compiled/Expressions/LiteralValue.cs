using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Expressions;

public abstract class LiteralValue : IEvaluable
{
	public static IEvaluable ConstructFromCodeModel(CodeModel.Expressions.LiteralExpression literal)
	{
		if (literal.IsStringLiteral)
			return new StringLiteralValue(literal.StringLiteralValue);
		else if (literal.TryAsInteger(out var integerValue))
			return new IntegerLiteralValue(integerValue);
		else if (literal.TryAsLong(out var longValue))
			return new LongLiteralValue(longValue);
		else if (literal.TryAsSingle(out var singleValue))
			return new SingleLiteralValue(singleValue);
		else if (literal.TryAsDouble(out var doubleValue))
			return new DoubleLiteralValue(doubleValue);
		else if (literal.TryAsCurrency(out var currencyValue))
			return new CurrencyLiteralValue(currencyValue);
		else
			throw new Exception("Internal error: Couldn't figure out how to interpret a LiteralExpression");
	}

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public abstract DataType Type { get; }

	public abstract Variable Evaluate(ExecutionContext context);
	public LiteralValue EvaluateConstant() => this;
}

public abstract class LiteralValue<T>(T value) : LiteralValue
{
	public T Value = value;
}

public class IntegerLiteralValue(short value) : LiteralValue<short>(value)
{
	public override DataType Type => DataType.Integer;
	public override Variable Evaluate(ExecutionContext context) => new IntegerVariable(Value);
}

public class LongLiteralValue(int value) : LiteralValue<int>(value)
{
	public override DataType Type => DataType.Long;
	public override Variable Evaluate(ExecutionContext context) => new LongVariable(Value);
}

public class SingleLiteralValue(float value) : LiteralValue<float>(value)
{
	public override DataType Type => DataType.Single;
	public override Variable Evaluate(ExecutionContext context) => new SingleVariable(Value);
}

public class DoubleLiteralValue(double value) : LiteralValue<double>(value)
{
	public override DataType Type => DataType.Double;
	public override Variable Evaluate(ExecutionContext context) => new DoubleVariable(Value);
}

public class CurrencyLiteralValue(decimal value) : LiteralValue<decimal>(value)
{
	public override DataType Type => DataType.Currency;
	public override Variable Evaluate(ExecutionContext context) => new CurrencyVariable(Value);
}

public class StringLiteralValue(string value) : LiteralValue<string>(value)
{
	public override DataType Type => DataType.String;
	public override Variable Evaluate(ExecutionContext context) => new StringVariable(Value);
}
