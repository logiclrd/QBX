using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Compiled.Statements;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class RndFunction : Function
{
	public Evaluable? Argument;

	protected override void SetArgument(int index, Evaluable value)
	{
		Argument = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref Argument);
	}

	static RndFunction s_noParameter = new RndFunction();

	public static RndFunction NoParameterInstance => s_noParameter;

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
