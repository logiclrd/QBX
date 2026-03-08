using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.OperatingSystem;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class CurDirFunction : Function
{
	protected override int MinArgumentCount => 0;
	protected override int MaxArgumentCount => 0;

	public override DataType Type => DataType.String;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (!ShortFileNames.TryMap(Environment.CurrentDirectory, out var shortPath))
			throw RuntimeException.PathNotFound(Source);

		return new StringVariable(new StringValue(shortPath));
	}
}
