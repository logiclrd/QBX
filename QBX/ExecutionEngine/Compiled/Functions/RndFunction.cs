using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class RndFunction : IEvaluable
{
	public IEvaluable? Argument;

	static RndFunction s_noParameter = new RndFunction();

	public static RndFunction NoParameterInstance => s_noParameter;

	public CodeModel.Statements.Statement? SourceStatement { get; set; }
	public CodeModel.Expressions.Expression? SourceExpression { get; set; }

	void Reseed(Variable seedValue)
	{
		int intPart = 0;
		double fracPart = 0;

		if ((seedValue is IntegerVariable) || (seedValue is LongVariable))
			intPart = seedValue.CoerceToInt();
		else if (seedValue is SingleVariable floatValue)
		{
			var noFracPart = float.Truncate(floatValue.Value);
			fracPart = floatValue.Value - noFracPart;

			while ((noFracPart < int.MinValue) || (noFracPart > int.MaxValue))
				noFracPart *= 0.5f;

			intPart = (int)noFracPart;
		}
		else if (seedValue is DoubleVariable doubleValue)
		{
			var noFracPart = double.Truncate(doubleValue.Value);
			fracPart = doubleValue.Value - noFracPart;

			while ((noFracPart < int.MinValue) || (noFracPart > int.MaxValue))
				noFracPart *= 0.5f;

			intPart = (int)noFracPart;
		}
		else if (seedValue is CurrencyVariable decimalValue)
		{
			var noFracPart = decimal.Truncate(decimalValue.Value);
			fracPart = decimal.ToDouble(decimalValue.Value - decimal.Truncate(decimalValue.Value));

			intPart = decimal.GetBits(noFracPart)[0];
		}

		RandomNumberGenerator.Reseed(intPart, fracPart);
	}

	public DataType Type => DataType.Single;

	public Variable Evaluate(ExecutionContext context)
	{
		if (Argument == null)
			RandomNumberGenerator.Advance();
		else
		{
			var argumentValue = Argument.Evaluate(context);

			if (argumentValue.IsNegative)
				Reseed(argumentValue);

			if (!argumentValue.IsZero)
				RandomNumberGenerator.Advance();
		}

		return new SingleVariable(RandomNumberGenerator.CurrentValue);
	}
}
