using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class CvDMBFFunction : Function
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

	public override DataType Type => DataType.Double;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (Argument == null)
			throw new Exception("CvDMBFFunction with no Argument");

		var argumentValue = (StringVariable)Argument.Evaluate(context, stackFrame);

		var bytes = argumentValue.ValueSpan;

		if (bytes.Length < 8)
			throw RuntimeException.IllegalFunctionCall(Source);

		var value = MicrosoftBinaryFormat.ToDouble(bytes);

		return new DoubleVariable(value);
	}
}
