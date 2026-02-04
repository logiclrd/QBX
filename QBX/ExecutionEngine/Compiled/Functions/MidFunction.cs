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
				if (!value.Type.IsNumeric)
					throw CompilerException.TypeMismatch(value.Source);

				StartExpression = value;
				break;
			case 2:
				if (!value.Type.IsNumeric)
					throw CompilerException.TypeMismatch(value.Source);

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

		int stringLength = stringVariable.ValueSpan.Length;

		int start = StartExpression.EvaluateAndCoerceToInt(context, stackFrame) - 1;
		int length = LengthExpression?.EvaluateAndCoerceToInt(context, stackFrame) ?? (stringLength - start);

		if ((start < 0) || (length < 0) || (start + length > stringLength))
			throw RuntimeException.IllegalFunctionCall(Source);

		return new Substring(stringVariable, start, length);
	}
}
