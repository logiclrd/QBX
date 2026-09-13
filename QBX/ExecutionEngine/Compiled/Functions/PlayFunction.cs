using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class PlayFunction : Function
{
	public Evaluable? Argument;

	public override bool CanEvaluateDirectWithoutEmbedding => Argument?.CanEvaluateDirectWithoutEmbedding ?? true;

	protected override void SetArgument(int index, Evaluable value)
	{
		Argument = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref Argument);
	}

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (Argument == null)
			throw new Exception("Internal error: PlayFunction with no argument");

		// Argument isn't used for anything, but it must still be evaluated as it can
		// have side-effects (e.g. by calling a FUNCTION). Its value is received as
		// an INTEGER.
		NumberConverter.ToInteger(Argument.Evaluate(context, stackFrame));

		return new IntegerVariable(NumberConverter.ToInteger(context.PersistentRuntimeState.PlayProcessor.QueueLength, Source?.Token));
	}
}
