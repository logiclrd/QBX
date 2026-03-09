using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class MkSMBFFunction : Function
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
			throw new Exception("MkSMBFFunction with no Argument");

		var argumentValue = Argument.Evaluate(context, stackFrame);

		var value = NumberConverter.ToSingle(argumentValue);

		byte[] bytes = MicrosoftBinaryFormat.GetBytes(value);

		return new StringVariable(new StringValue(bytes));
	}
}
