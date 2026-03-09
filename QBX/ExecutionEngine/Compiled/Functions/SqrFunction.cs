using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class SqrFunction : Function
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
			throw new Exception("SqrFunction with no Argument");

		var type = Argument.Type;

		var argumentValue = Argument.Evaluate(context, stackFrame);

		var value = NumberConverter.ToDouble(argumentValue, Source?.Token);

		var result = Math.Sqrt(value); // negative values become NaN

		return new DoubleVariable(result);
	}
}
