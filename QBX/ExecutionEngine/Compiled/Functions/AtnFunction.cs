using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class AtnFunction : ConstructibleOneArgumentMathFunction<SingleAtnFunction, DoubleAtnFunction>
{
}

public class SingleAtnFunction : AtnFunction
{
	public override DataType Type => DataType.Single;

	protected override Variable EvaluateImplementation(Evaluable argument, ExecutionContext context, StackFrame stackFrame)
	{
		var type = argument.Type;

		var argumentValue = argument.Evaluate(context, stackFrame);

		var angle = NumberConverter.ToSingle(argumentValue, Source?.Token);

		// Math.Atan passes through Indeterminate values, but QuickBASIC ATN
		// turns them into QNaN values.
		float result;

		if (NumberConverter.IsIndeterminate(angle))
			result = NumberConverter.SingleQuietNaN;
		else
			result = NumberConverter.TranslateNaN(MathF.Atan(angle));

		return new SingleVariable(result);
	}
}

public class DoubleAtnFunction : AtnFunction
{
	public override DataType Type => DataType.Double;

	protected override Variable EvaluateImplementation(Evaluable argument, ExecutionContext context, StackFrame stackFrame)
	{
		var type = argument.Type;

		var argumentValue = argument.Evaluate(context, stackFrame);

		var angle = NumberConverter.ToDouble(argumentValue, Source?.Token);

		// Math.Atan passes through Indeterminate values, but QuickBASIC ATN
		// turns them into QNaN values.
		double result;

		if (NumberConverter.IsIndeterminate(angle))
			result = NumberConverter.DoubleQuietNaN;
		else
			result = NumberConverter.TranslateNaN(Math.Atan(angle));

		return new DoubleVariable(result);
	}
}
