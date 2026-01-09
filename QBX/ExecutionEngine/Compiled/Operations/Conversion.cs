using System;
using System.Diagnostics.CodeAnalysis;
using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Operations;

public static class Conversion
{
	[return: NotNullIfNotNull(nameof(expression))]
	public static IEvaluable? Construct(IEvaluable? expression, PrimitiveDataType targetType)
	{
		if (expression == null)
			return null;

		if (expression.Type.IsPrimitiveType && (expression.Type.PrimitiveType == targetType))
			return expression;

		if (expression is LiteralValue)
		{
			if (expression.Type.IsString)
				throw CompilerException.TypeMismatch(expression.SourceExpression?.Token);

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
				throw CompilerException.IllegalNumber(expression.SourceExpression?.Token);
			}
		}

		if (expression.Type.IsPrimitiveType && (expression.Type.PrimitiveType == targetType))
			return expression;

		switch (targetType)
		{
			case PrimitiveDataType.Integer: return new ConvertToInteger(expression);
			case PrimitiveDataType.Long: return new ConvertToLong(expression);
			case PrimitiveDataType.Single: return new ConvertToSingle(expression);
			case PrimitiveDataType.Double: return new ConvertToDouble(expression);
			case PrimitiveDataType.Currency: return new ConvertToCurrency(expression);
			case PrimitiveDataType.String: return new ConvertToString(expression);

			default: throw new Exception("Internal error: Unrecognized PrimitiveDataType " + targetType);
		}
	}
}

public class ConvertToInteger(IEvaluable value) : Expression
{
	public IEvaluable Value => value;

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new IntegerVariable(NumberConverter.ToInteger(Value.Evaluate(context, stackFrame)));
	public override LiteralValue EvaluateConstant() => new IntegerLiteralValue(NumberConverter.ToInteger(Value.EvaluateConstant()));
}

public class ConvertToLong(IEvaluable value) : Expression
{
	public IEvaluable Value => value;

	public override DataType Type => DataType.Long;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new LongVariable(NumberConverter.ToLong(Value.Evaluate(context, stackFrame)));
	public override LiteralValue EvaluateConstant() => new LongLiteralValue(NumberConverter.ToLong(Value.EvaluateConstant()));
}

public class ConvertToSingle(IEvaluable value) : Expression
{
	public IEvaluable Value => value;

	public override DataType Type => DataType.Single;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new SingleVariable(NumberConverter.ToSingle(Value.Evaluate(context, stackFrame)));
	public override LiteralValue EvaluateConstant() => new SingleLiteralValue(NumberConverter.ToSingle(Value.EvaluateConstant()));
}

public class ConvertToDouble(IEvaluable value) : Expression
{
	public IEvaluable Value => value;

	public override DataType Type => DataType.Double;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new DoubleVariable(NumberConverter.ToDouble(Value.Evaluate(context, stackFrame)));
	public override LiteralValue EvaluateConstant() => new DoubleLiteralValue(NumberConverter.ToDouble(Value.EvaluateConstant()));
}

public class ConvertToCurrency(IEvaluable value) : Expression
{
	public IEvaluable Value => value;

	public override DataType Type => DataType.Currency;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new CurrencyVariable(NumberConverter.ToCurrency(Value.Evaluate(context, stackFrame)));
	public override LiteralValue EvaluateConstant() => new CurrencyLiteralValue(NumberConverter.ToCurrency(Value.EvaluateConstant()));
}

public class ConvertToString(IEvaluable value) : Expression
{
	public IEvaluable Value => value;

	public override DataType Type => DataType.String;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame) => new StringVariable(Value.Evaluate(context, stackFrame).ToString() ?? "");
	public override LiteralValue EvaluateConstant() => new StringLiteralValue(Value.EvaluateConstant().ToString() ?? "");
}

