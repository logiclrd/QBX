using System;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class MidFunction : Function
{
	public Evaluable? StringExpression;
	public Evaluable? StartExpression;
	public Evaluable? LengthExpression;

	public bool IsAssignable { get; private set; }

	protected override int MinArgumentCount => 2;
	protected override int MaxArgumentCount => 3;

	protected override void SetArgument(int index, Evaluable value)
	{
		switch (index)
		{
			case 0:
				if (!value.Type.IsString)
					throw CompilerException.TypeMismatch(value.Source);

				IsAssignable =
					(value is IdentifierExpression) ||
					(value is FieldAccessExpression);

				StringExpression = value;
				break;
			case 1:
				StartExpression = value;
				break;
			case 2:
				LengthExpression = value;
				break;
		}
	}

	public override void CollapseConstantSubexpressions()
	{
		StringExpression?.CollapseConstantSubexpressions();
		CollapseConstantExpression(ref StartExpression);
		CollapseConstantExpression(ref LengthExpression);
	}

	public override DataType Type => DataType.String;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (StringExpression == null)
			throw new Exception("MidFunction with no StringExpression");
		if (StartExpression == null)
			throw new Exception("MidFunction with no StartExpression");

		var stringVariable = (StringVariable)StringExpression.Evaluate(context, stackFrame);

		var startValue = StartExpression.Evaluate(context, stackFrame);

		var lengthValue = LengthExpression?.Evaluate(context, stackFrame);

		int stringLength = stringVariable.ValueSpan.Length;

		int start = startValue.CoerceToInt(context: StartExpression) - 1;
		int length = lengthValue?.CoerceToInt(context: LengthExpression) ?? (stringLength - start);

		if ((start < 0) || (length < 0) || (start + length >= stringLength))
			throw RuntimeException.IllegalFunctionCall(Source);

		return new Substring(stringVariable, start, length);
	}
}
