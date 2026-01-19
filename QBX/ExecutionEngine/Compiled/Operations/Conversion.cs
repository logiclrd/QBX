using System;
using System.Diagnostics.CodeAnalysis;
using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.LexicalAnalysis;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Operations;

public abstract class Conversion(Evaluable value) : Evaluable
{
	[return: NotNullIfNotNull(nameof(expression))]
	public static Evaluable? Construct(Evaluable? expression, PrimitiveDataType targetType, Token? context = null)
	{
		if (expression == null)
			return null;

		if (!expression.Type.IsPrimitiveType)
			throw CompilerException.TypeMismatch(context);

		if (expression.Type.PrimitiveType == targetType)
			return expression;

		if (expression is LiteralValue)
		{
			if (!expression.Type.IsNumeric)
				throw CompilerException.TypeMismatch(expression.Source);

			var value = expression.EvaluateConstant();

			try
			{
				switch (targetType)
				{
					case PrimitiveDataType.Integer: return new IntegerLiteralValue(NumberConverter.ToInteger(value.GetData()));
					case PrimitiveDataType.Long: return new LongLiteralValue(NumberConverter.ToLong(value.GetData()));
					case PrimitiveDataType.Single: return new SingleLiteralValue(NumberConverter.ToSingle(value.GetData()));
					case PrimitiveDataType.Double: return new DoubleLiteralValue(NumberConverter.ToDouble(value.GetData()));
					case PrimitiveDataType.Currency: return new CurrencyLiteralValue(NumberConverter.ToCurrency(value.GetData()));

					default: throw new Exception("Internal error: Failed to match PrimitiveDataType");
				}
			}
			catch (RuntimeException)
			{
				throw CompilerException.IllegalNumber(expression.Source);
			}
		}

		Evaluable conversion;

		switch (targetType)
		{
			case PrimitiveDataType.Integer: conversion = new ConvertToInteger(expression); break;
			case PrimitiveDataType.Long: conversion = new ConvertToLong(expression); break;
			case PrimitiveDataType.Single: conversion = new ConvertToSingle(expression); break;
			case PrimitiveDataType.Double: conversion = new ConvertToDouble(expression); break;
			case PrimitiveDataType.Currency: conversion = new ConvertToCurrency(expression); break;
			case PrimitiveDataType.String: conversion = new ConvertToString(expression); break;

			default: throw new Exception("Internal error: Unrecognized PrimitiveDataType " + targetType);
		}

		if (conversion.IsConstant)
			return conversion.EvaluateConstant();
		else
			return conversion;
	}

	public Evaluable Value => value;

	public override bool IsConstant => value.IsConstant;

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref value);
	}
}

public class ConvertToInteger(Evaluable value) : Conversion(value)
{
	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new IntegerVariable(NumberConverter.ToInteger(Value.Evaluate(context, stackFrame)));
	public override LiteralValue EvaluateConstant() => new IntegerLiteralValue(NumberConverter.ToInteger(Value.EvaluateConstant()));
}

public class ConvertToLong(Evaluable value) : Conversion(value)
{
	public override DataType Type => DataType.Long;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new LongVariable(NumberConverter.ToLong(Value.Evaluate(context, stackFrame)));
	public override LiteralValue EvaluateConstant() => new LongLiteralValue(NumberConverter.ToLong(Value.EvaluateConstant()));
}

public class ConvertToSingle(Evaluable value) : Conversion(value)
{
	public override DataType Type => DataType.Single;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new SingleVariable(NumberConverter.ToSingle(Value.Evaluate(context, stackFrame)));
	public override LiteralValue EvaluateConstant() => new SingleLiteralValue(NumberConverter.ToSingle(Value.EvaluateConstant()));
}

public class ConvertToDouble(Evaluable value) : Conversion(value)
{
	public override DataType Type => DataType.Double;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new DoubleVariable(NumberConverter.ToDouble(Value.Evaluate(context, stackFrame)));
	public override LiteralValue EvaluateConstant() => new DoubleLiteralValue(NumberConverter.ToDouble(Value.EvaluateConstant()));
}

public class ConvertToCurrency(Evaluable value) : Conversion(value)
{
	public override DataType Type => DataType.Currency;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new CurrencyVariable(NumberConverter.ToCurrency(Value.Evaluate(context, stackFrame)));
	public override LiteralValue EvaluateConstant() => new CurrencyLiteralValue(NumberConverter.ToCurrency(Value.EvaluateConstant()));
}

public class ConvertToString(Evaluable value) : Conversion(value)
{
	public override DataType Type => DataType.String;

	StringValue ToString(Variable value)
	{
		switch (value)
		{
			case IntegerVariable integerValue: return NumberFormatter.Format(integerValue.Value).ToStringValue();
			case LongVariable longValue: return NumberFormatter.Format(longValue.Value, qualify: false).ToStringValue();
			case SingleVariable singleValue: return NumberFormatter.Format(singleValue.Value, qualify: false).ToStringValue();
			case DoubleVariable doubleValue: return NumberFormatter.Format(doubleValue.Value, qualify: false).ToStringValue();
			case CurrencyVariable currencyValue: return NumberFormatter.Format(currencyValue.Value, qualify: false).ToStringValue();
			case StringVariable stringValue: return stringValue.Value;

			default: throw new Exception("Internal error");
		}
	}

	StringValue ToString(object value, PrimitiveDataType type)
	{
		switch (type)
		{
			case PrimitiveDataType.Integer: return NumberFormatter.Format((short)value).ToStringValue();
			case PrimitiveDataType.Long: return NumberFormatter.Format((int)value, qualify: false).ToStringValue();
			case PrimitiveDataType.Single: return NumberFormatter.Format((float)value, qualify: false).ToStringValue();
			case PrimitiveDataType.Double: return NumberFormatter.Format((double)value, qualify: false).ToStringValue();
			case PrimitiveDataType.Currency: return NumberFormatter.Format((decimal)value, qualify: false).ToStringValue();
			case PrimitiveDataType.String: return ((StringValue?)value) ?? new StringValue();

			default: throw new Exception("Internal error");
		}
	}

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new StringVariable(ToString(Value.Evaluate(context, stackFrame)));
	public override LiteralValue EvaluateConstant() => new StringLiteralValue(ToString(Value.EvaluateConstant(), Value.Type.PrimitiveType));
}

