using System;
using System.Diagnostics.CodeAnalysis;

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

public class ConvertToInteger(IEvaluable value) : IEvaluable
{
	public IEvaluable Value => value;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate(ExecutionContext context) => new IntegerVariable(NumberConverter.ToInteger(Value.Evaluate(context)));
}

public class ConvertToLong(IEvaluable value) : IEvaluable
{
	public IEvaluable Value => value;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Long;

	public Variable Evaluate(ExecutionContext context) => new LongVariable(NumberConverter.ToLong(Value.Evaluate(context)));
}

public class ConvertToSingle(IEvaluable value) : IEvaluable
{
	public IEvaluable Value => value;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Single;

	public Variable Evaluate(ExecutionContext context) => new SingleVariable(NumberConverter.ToSingle(Value.Evaluate(context)));
}

public class ConvertToDouble(IEvaluable value) : IEvaluable
{
	public IEvaluable Value => value;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Double;

	public Variable Evaluate(ExecutionContext context) => new DoubleVariable(NumberConverter.ToDouble(Value.Evaluate(context)));
}

public class ConvertToCurrency(IEvaluable value) : IEvaluable
{
	public IEvaluable Value => value;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Currency;

	public Variable Evaluate(ExecutionContext context) => new CurrencyVariable(NumberConverter.ToCurrency(Value.Evaluate(context)));
}

public class ConvertToString(IEvaluable value) : IEvaluable
{
	public IEvaluable Value => value;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.String;

	public Variable Evaluate(ExecutionContext context) => new StringVariable(Value.Evaluate(context).ToString() ?? "");
}

