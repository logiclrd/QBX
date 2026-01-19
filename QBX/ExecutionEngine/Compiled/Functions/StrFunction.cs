using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class StrFunction : Function
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
			throw new Exception("ChrFunction with no Argument");

		var argumentValue = Argument.Evaluate(context, stackFrame);

		var formatted = NumberFormatter.Format(argumentValue, qualify: false, Source);

		var stringValue = new StringValue(formatted);

		return new StringVariable(stringValue);
	}
}
