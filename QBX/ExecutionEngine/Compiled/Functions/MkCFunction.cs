using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;
using QBX.Utility;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class MkCFunction : Function
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

	public override DataType Type => DataType.String;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (Argument == null)
			throw new Exception("MkCFunction with no Argument");

		var argumentValue = Argument.Evaluate(context, stackFrame);

		var value = NumberConverter.ToCurrency(argumentValue);

		byte[] bytes = BitConverter.GetBytes(decimal.ToOACurrency(value));

		return new StringVariable(new StringValue(bytes));
	}
}
