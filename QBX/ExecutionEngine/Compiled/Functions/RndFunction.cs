using QBX.ExecutionEngine.Compiled.Statements;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class RndFunction : Function
{
	public Evaluable? Argument;

	protected override int MinArgumentCount => 0;

	protected override void SetArgument(int index, Evaluable value)
	{
		Argument = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref Argument);
	}

	public override DataType Type => DataType.Single;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (Argument == null)
			RandomNumberGenerator.Advance();
		else
		{
			var argumentValue = Argument.Evaluate(context, stackFrame);

			if (argumentValue.IsNegative)
				RandomizeStatement.Reseed(argumentValue);

			if (!argumentValue.IsZero)
				RandomNumberGenerator.Advance();
		}

		return new SingleVariable(RandomNumberGenerator.CurrentValue);
	}
}
