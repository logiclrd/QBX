using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class TrimFunction : Function
{
	public Evaluable? StringExpression;

	protected override int MinArgumentCount => 1;
	protected override int MaxArgumentCount => 1;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsString)
			throw CompilerException.TypeMismatch(value.Source);

		StringExpression = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref StringExpression);
	}

	public override DataType Type => DataType.String;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (StringExpression == null)
			throw new Exception(GetType().Name + "Function with no StringExpression");

		var stringVariable = (StringVariable)StringExpression.Evaluate(context, stackFrame);

		var stringValue = stringVariable.Value;

		return new StringVariable(PerformTrim(stringValue));
	}

	protected static bool IsSpace(byte value) => (value == 32);

	protected abstract StringValue PerformTrim(StringValue stringValue);
}
