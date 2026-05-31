using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class ExpFunction : ConstructibleOneArgumentMathFunction<SingleExpFunction, DoubleExpFunction>
{
}

public class SingleExpFunction : ExpFunction
{
	public override DataType Type => DataType.Single;

	protected override Variable EvaluateImplementation(Evaluable argument, ExecutionContext context, StackFrame stackFrame)
	{
		var type = argument.Type;

		var argumentValue = argument.Evaluate(context, stackFrame);

		var exponent = NumberConverter.ToSingle(argumentValue, Source?.Token);

		float result;

		if (NumberConverter.IsIndeterminate(exponent))
			result = 0;
		else
		{
			if (float.IsPositiveInfinity(exponent) || float.IsNaN(exponent))
				throw RuntimeException.Overflow(Source);

			result = NumberConverter.TranslateNaN(MathF.Exp(exponent));
		}

		return new SingleVariable(result);
	}
}

public class DoubleExpFunction : ExpFunction
{
	public override DataType Type => DataType.Double;

	protected override Variable EvaluateImplementation(Evaluable argument, ExecutionContext context, StackFrame stackFrame)
	{
		var type = argument.Type;

		var argumentValue = argument.Evaluate(context, stackFrame);

		var exponent = NumberConverter.ToDouble(argumentValue, Source?.Token);

		double result;

		if (NumberConverter.IsIndeterminate(exponent))
			result = 0;
		else
		{
			if (double.IsPositiveInfinity(exponent) || double.IsNaN(exponent))
				throw RuntimeException.Overflow(Source);

			result = NumberConverter.TranslateNaN(Math.Exp(exponent));
		}

		return new DoubleVariable(result);
	}
}
