using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class CommandFunction : Function
{
	public override DataType Type => DataType.String;

	protected override int MinArgumentCount => 0;
	protected override int MaxArgumentCount => 0;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		return new StringVariable(context.CommandLine);
	}
}
