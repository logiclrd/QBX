using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Hardware;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class FreeFileFunction : Function
{
	public override DataType Type => DataType.Integer;

	protected override int MinArgumentCount => 0;
	protected override int MaxArgumentCount => 0;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		for (int i = 1; i < short.MaxValue; i++)
			if (!context.Files.ContainsKey(i))
				return new IntegerVariable((short)i);

		throw RuntimeException.TooManyFiles(Source);
	}
}
