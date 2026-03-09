using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class SgnFunction : Function
{
	public Evaluable? Argument;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsNumeric)
			throw CompilerException.TypeMismatch(value.Source);

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
			throw new Exception("SgnFunction with no Argument");

		var type = Argument.Type;

		var argumentValue = Argument.Evaluate(context, stackFrame);

		short result;

		if (argumentValue.IsZero)
			result = 0;
		else if (argumentValue.IsNegative)
			result = -1;
		else
			result = +1;

		return new IntegerVariable(result);
	}
}
