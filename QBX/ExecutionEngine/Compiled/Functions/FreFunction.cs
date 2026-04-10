using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class FreFunction : Function
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

	public override DataType Type => DataType.Long;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (Argument == null)
			throw new Exception("CLngFunction with no Argument");

		var type = Argument.Type;

		var argumentValue = Argument.Evaluate(context, stackFrame);

		int returnValue;

		if (argumentValue is StringVariable)
			returnValue = 65520;
		else
		{
			switch (argumentValue.CoerceToInt(Argument))
			{
				case 0: returnValue = 65520; break;
				case -1: returnValue = int.MaxValue; break;
				case -2: returnValue = 900 * 1024; break;
				case -3: returnValue = 32768; break;

				default: throw RuntimeException.IllegalFunctionCall(Source);
			}
		}

		return new LongVariable(returnValue);
	}
}
