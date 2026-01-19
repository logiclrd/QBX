using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class TanFunction : Function
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

	public override DataType Type => DataType.Double;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (Argument == null)
			throw new Exception("ValFunction with no Argument");

		var type = Argument.Type;

		var argumentValue = Argument.Evaluate(context, stackFrame);

		var angle = NumberConverter.ToDouble(argumentValue, Source?.Token);

		var result = Math.Tan(angle);

		return new DoubleVariable(result);
	}
}
