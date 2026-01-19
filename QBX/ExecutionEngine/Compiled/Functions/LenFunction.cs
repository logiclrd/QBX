using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class LenFunction : Function
{
	public Evaluable? Argument;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsString && !value.Type.IsUserType)
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
			throw new Exception("LenFunction with no Argument");

		var type = Argument.Type;

		int lengthValue;

		if (type.IsString)
		{
			var argumentValue = ((StringVariable)Argument.Evaluate(context, stackFrame)).ValueSpan;

			lengthValue = argumentValue.Length;
		}
		else if (type.IsUserType)
			lengthValue = type.ByteSize;
		else
			throw new Exception("Internal error");

		if (lengthValue > short.MaxValue)
			throw RuntimeException.Overflow(Argument.Source);

		return new IntegerVariable((short)lengthValue);
	}
}
