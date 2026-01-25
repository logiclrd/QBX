using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.LexicalAnalysis;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Expressions;

public abstract class LiteralValue : Evaluable
{
	public static Evaluable Construct(object value, DataType type, Token? context)
	{
		if (!type.IsPrimitiveType)
			throw new Exception("Internal error: Attempt to construct a LiteralValue with a non-primitive type.");

		return Construct(value, type.PrimitiveType, context);
	}

	public static Evaluable Construct(object value, PrimitiveDataType type, Token? context)
	{
		switch (type)
		{
			case PrimitiveDataType.Integer: return new IntegerLiteralValue(Convert.ToInt16(value));
			case PrimitiveDataType.Long: return new LongLiteralValue(Convert.ToInt32(value));
			case PrimitiveDataType.Single: return new SingleLiteralValue(Convert.ToSingle(value));
			case PrimitiveDataType.Double: return new DoubleLiteralValue(Convert.ToDouble(value));

			case PrimitiveDataType.Currency:
				decimal decimalValue = Convert.ToDecimal(value);

				if (!decimalValue.IsInCurrencyRange())
					throw CompilerException.Overflow(context);

				return new CurrencyLiteralValue(decimalValue);

			case PrimitiveDataType.String:
				return new StringLiteralValue(new StringValue((string)value));

			default: throw new Exception("Unrecognized primitive type " + type);
		}
	}

	public static Evaluable ConstructFromCodeModel(CodeModel.Expressions.LiteralExpression literal)
	{
		if (literal.IsStringLiteral)
			return new StringLiteralValue(new StringValue(literal.StringLiteralValue));
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

	public abstract object GetData();

	public override bool IsConstant => true;

	public override LiteralValue EvaluateConstant() => this;
}

public abstract class LiteralValue<T>(T value) : LiteralValue
	where T : notnull
{
	public T Value = value;

	public override object GetData() => Value;
}

public class IntegerLiteralValue(short value) : LiteralValue<short>(value)
{
	public override DataType Type => DataType.Integer;
	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new IntegerVariable(Value);
}

public class LongLiteralValue(int value) : LiteralValue<int>(value)
{
	public override DataType Type => DataType.Long;
	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new LongVariable(Value);
}

public class SingleLiteralValue(float value) : LiteralValue<float>(value)
{
	public override DataType Type => DataType.Single;
	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new SingleVariable(Value);
}

public class DoubleLiteralValue(double value) : LiteralValue<double>(value)
{
	public override DataType Type => DataType.Double;
	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new DoubleVariable(Value);
}

public class CurrencyLiteralValue(decimal value) : LiteralValue<decimal>(value)
{
	public override DataType Type => DataType.Currency;
	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new CurrencyVariable(Value);
}

public class StringLiteralValue(StringValue value) : LiteralValue<StringValue>(value)
{
	public override DataType Type => DataType.String;
	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new StringVariable(Value);
}
