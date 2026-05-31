using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class CosFunction : ConstructibleOneArgumentMathFunction<SingleCosFunction, DoubleCosFunction>
{
}

public class SingleCosFunction : CosFunction
{
	public override DataType Type => DataType.Single;

	protected override Variable EvaluateImplementation(Evaluable argument, ExecutionContext context, StackFrame stackFrame)
	{
		var type = argument.Type;

		var argumentValue = argument.Evaluate(context, stackFrame);

		var angle = NumberConverter.ToSingle(argumentValue, Source?.Token);

		var result = MathF.Cos(angle);

		if (float.IsNaN(result))
			throw RuntimeException.IllegalFunctionCall(Source);

		return new SingleVariable(result);
	}
}

public class DoubleCosFunction : CosFunction
{
	public override DataType Type => DataType.Double;

	protected override Variable EvaluateImplementation(Evaluable argument, ExecutionContext context, StackFrame stackFrame)
	{
		var type = argument.Type;

		var argumentValue = argument.Evaluate(context, stackFrame);

		var angle = NumberConverter.ToDouble(argumentValue, Source?.Token);

		var result = Math.Cos(angle);

		if (double.IsNaN(result))
			throw RuntimeException.IllegalFunctionCall(Source);

		return new DoubleVariable(result);
	}
}
