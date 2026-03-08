using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class StringFunction : Function
{
	public Evaluable? LengthExpression;
	public Evaluable? FillExpression;

	protected override int MinArgumentCount => 2;
	protected override int MaxArgumentCount => 2;

	protected override void SetArgument(int index, Evaluable value)
	{
		switch (index)
		{
			case 0:
			{
				if (!value.Type.IsNumeric)
					throw CompilerException.TypeMismatch(value.Source);

				LengthExpression = value;

				break;
			}
			case 1:
			{
				if (!value.Type.IsNumeric && !value.Type.IsString)
					throw CompilerException.TypeMismatch(value.Source);

				FillExpression = value;

				break;
			}
		}
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref LengthExpression);
		CollapseConstantExpression(ref FillExpression);
	}

	public override DataType Type => DataType.String;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (LengthExpression == null)
			throw new Exception("ChrFunction with no LengthExpression");
		if (FillExpression == null)
			throw new Exception("ChrFunction with no FillExpression");

		int length = LengthExpression.EvaluateAndCoerceToInt(context, stackFrame);

		byte fill;

		if (FillExpression.Type.IsNumeric)
		{
			var fillCharacter = checked((short)FillExpression.EvaluateAndCoerceToInt(context, stackFrame));

			fill = unchecked((byte)fillCharacter);
		}
		else
		{
			var fillPrototype = FillExpression.Evaluate(context, stackFrame);

			if (fillPrototype is not StringVariable fillString)
				throw RuntimeException.TypeMismatch(FillExpression.Source);
			if (fillString.Value.Length == 0)
				throw RuntimeException.IllegalFunctionCall(Source);

			fill = fillString.Value[0];
		}

		var stringValue = StringValue.CreateFixedLength(length);

		stringValue.AsSpan().Fill(fill);

		return new StringVariable(stringValue);
	}
}
