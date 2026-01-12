using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Hardware;
using QBX.Numbers;
using System;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class TimerFunction : Function
{
	public override DataType Type => DataType.Single;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		return new SingleVariable((float)(context.Machine.Timer.TickCount / TimerChip.TicksPerSecond));
	}
}
