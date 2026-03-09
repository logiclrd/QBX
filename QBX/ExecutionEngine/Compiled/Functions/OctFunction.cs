using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class OctFunction : Function
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
			throw new Exception("OctFunction with no Argument");

		var argumentValue = Argument.Evaluate(context, stackFrame);

		string formatted;

		if (argumentValue is IntegerVariable integerVariable)
			formatted = Convert.ToString(unchecked((ushort)integerVariable.Value), toBase: 8);
		else
			formatted = Convert.ToString(argumentValue.CoerceToInt(Argument), toBase: 8);

		var stringValue = new StringValue(formatted);

		return new StringVariable(stringValue);
	}
}
