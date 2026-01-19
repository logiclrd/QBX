using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class ErrFunction : Function
{
	protected override int MinArgumentCount =>  0;
	protected override int MaxArgumentCount => 0;

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		return context.ErrVariable;
	}
}
