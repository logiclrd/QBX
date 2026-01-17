using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Hardware;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class TimerFunction : Function
{
	public override DataType Type => DataType.Single;

	protected override int MinArgumentCount => 0;
	protected override int MaxArgumentCount => 0;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		return new SingleVariable((float)(context.Machine.Timer.TickCount / TimerChip.TicksPerSecond));
	}
}
