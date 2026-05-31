using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class LogFunction : ConstructibleOneArgumentMathFunction<SingleLogFunction, DoubleLogFunction>
{
}

public class SingleLogFunction : LogFunction
{
	public override DataType Type => DataType.Single;

	protected override Variable EvaluateImplementation(Evaluable argument, ExecutionContext context, StackFrame stackFrame)
	{
		var type = argument.Type;

		var argumentValue = argument.Evaluate(context, stackFrame);

		var value = NumberConverter.ToSingle(argumentValue, Source?.Token);

		if (NumberConverter.IsIndeterminate(value) || (value < 0))
			throw RuntimeException.IllegalFunctionCall(Source);

		var result = NumberConverter.TranslateNaN(MathF.Log(value));

		return new SingleVariable(result);
	}
}

public class DoubleLogFunction : LogFunction
{
	public override DataType Type => DataType.Double;

	protected override Variable EvaluateImplementation(Evaluable argument, ExecutionContext context, StackFrame stackFrame)
	{
		var type = argument.Type;

		var argumentValue = argument.Evaluate(context, stackFrame);

		var value = NumberConverter.ToDouble(argumentValue, Source?.Token);

		if (NumberConverter.IsIndeterminate(value) || (value < 0))
			throw RuntimeException.IllegalFunctionCall(Source);

		var result = NumberConverter.TranslateNaN(Math.Log(value));

		return new DoubleVariable(result);
	}
}
