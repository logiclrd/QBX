using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class AscFunction : Function
{
	public Evaluable? Argument;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsString)
			throw CompilerException.TypeMismatch(value.SourceExpression?.Token);

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
			throw new Exception("AscFunction with no Argument");

		var argumentValue = ((StringVariable)Argument.Evaluate(context, stackFrame)).Value;

		if (argumentValue.Length == 0)
			throw RuntimeException.IllegalFunctionCall(SourceExpression?.Token);

		return new IntegerVariable(argumentValue[0]);
	}
}
