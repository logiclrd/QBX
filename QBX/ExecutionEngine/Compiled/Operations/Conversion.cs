using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Operations;

public class ConvertToInteger(IEvaluable value) : IEvaluable
{
	public IEvaluable Value => value;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Integer;

	public Variable Evaluate() => new IntegerVariable(NumberConverter.ToInteger(Value.Evaluate()));
}

public class ConvertToLong(IEvaluable value) : IEvaluable
{
	public IEvaluable Value => value;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Long;

	public Variable Evaluate() => new LongVariable(NumberConverter.ToLong(Value.Evaluate()));
}

public class ConvertToSingle(IEvaluable value) : IEvaluable
{
	public IEvaluable Value => value;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Single;

	public Variable Evaluate() => new SingleVariable(NumberConverter.ToSingle(Value.Evaluate()));
}

public class ConvertToDouble(IEvaluable value) : IEvaluable
{
	public IEvaluable Value => value;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Double;

	public Variable Evaluate() => new DoubleVariable(NumberConverter.ToDouble(Value.Evaluate()));
}

public class ConvertToCurrency(IEvaluable value) : IEvaluable
{
	public IEvaluable Value => value;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.Currency;

	public Variable Evaluate() => new CurrencyVariable(NumberConverter.ToCurrency(Value.Evaluate()));
}

public class ConvertToString(IEvaluable value) : IEvaluable
{
	public IEvaluable Value => value;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	public DataType Type => DataType.String;

	public Variable Evaluate() => new StringVariable(Value.Evaluate().ToString() ?? "");
}

