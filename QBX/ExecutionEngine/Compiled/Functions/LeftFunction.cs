using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class LeftFunction : Function
{
	public Evaluable? StringExpression;
	public Evaluable? LengthExpression;

	protected override int MinArgumentCount => 2;
	protected override int MaxArgumentCount => 2;

	protected override void SetArgument(int index, Evaluable value)
	{
		switch (index)
		{
			case 0:
				if (!value.Type.IsString)
					throw CompilerException.TypeMismatch(value.Source);

				StringExpression = value;
				break;
			case 1:
				LengthExpression = value;
				break;
		}
	}

	public override void CollapseConstantSubexpressions()
	{
		StringExpression?.CollapseConstantSubexpressions();
		CollapseConstantExpression(ref LengthExpression);
	}

	public override DataType Type => DataType.String;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (StringExpression == null)
			throw new Exception("LeftFunction with no StringExpression");
		if (LengthExpression == null)
			throw new Exception("LeftFunction with no LengthExpression");

		var stringVariable = (StringVariable)StringExpression.Evaluate(context, stackFrame);

		var lengthValue = LengthExpression.Evaluate(context, stackFrame);

		int stringLength = stringVariable.ValueSpan.Length;

		int length = lengthValue.CoerceToInt(context: LengthExpression);

		if (length > stringLength)
			length = stringLength;
		if (length < 0)
			throw RuntimeException.IllegalFunctionCall(Source);

		return new Substring(stringVariable, 0, length);
	}
}
