using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class CvLFunction : Function
{
	public Evaluable? Argument;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsString)
			throw CompilerException.TypeMismatch(value.Source);

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
			throw new Exception("CvLFunction with no Argument");

		var argumentValue = (StringVariable)Argument.Evaluate(context, stackFrame);

		var bytes = argumentValue.ValueSpan;

		if (bytes.Length < 4)
			throw RuntimeException.IllegalFunctionCall(Source);

		var value = BitConverter.ToInt32(bytes);

		return new LongVariable(value);
	}
}
