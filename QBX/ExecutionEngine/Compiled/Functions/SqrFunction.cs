using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class SqrFunction : ConstructibleOneArgumentMathFunction<SingleSqrFunction, DoubleSqrFunction>
{
}

public class SingleSqrFunction : SqrFunction
{
	public override DataType Type => DataType.Single;

	protected override Variable EvaluateImplementation(Evaluable argument, ExecutionContext context, StackFrame stackFrame)
	{
		var type = argument.Type;

		var argumentValue = argument.Evaluate(context, stackFrame);

		var value = NumberConverter.ToSingle(argumentValue, Source?.Token);

		float result;

		if (value < 0)
			result = NumberConverter.SingleIndeterminate;
		else
			result = NumberConverter.TranslateNaN(MathF.Sqrt(value)); // negative values become NaN

		return new SingleVariable(result);
	}
}

public class DoubleSqrFunction : SqrFunction
{
	public override DataType Type => DataType.Double;

	protected override Variable EvaluateImplementation(Evaluable argument, ExecutionContext context, StackFrame stackFrame)
	{
		var type = argument.Type;

		var argumentValue = argument.Evaluate(context, stackFrame);

		var value = NumberConverter.ToDouble(argumentValue, Source?.Token);

		double result;

		if (value < 0)
			result = NumberConverter.DoubleIndeterminate;
		else
			result = NumberConverter.TranslateNaN(Math.Sqrt(value)); // negative values become NaN

		return new DoubleVariable(result);
	}
}
