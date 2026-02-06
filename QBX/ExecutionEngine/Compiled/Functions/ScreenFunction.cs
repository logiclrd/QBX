using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class ScreenFunction : Function
{
	public Evaluable? LineExpression;
	public Evaluable? ColumnExpression;
	public Evaluable? ColourFlagExpression;

	protected override int MinArgumentCount => 2;
	protected override int MaxArgumentCount => 3;

	protected override void SetArgument(int index, Evaluable value)
	{
		switch (index)
		{
			case 0:
				if (!value.Type.IsNumeric)
					throw CompilerException.TypeMismatch(value.Source);

				LineExpression = value;
				break;
			case 1:
				if (!value.Type.IsNumeric)
					throw CompilerException.TypeMismatch(value.Source);

				ColumnExpression = value;
				break;
			case 2:
				if (!value.Type.IsNumeric)
					throw CompilerException.TypeMismatch(value.Source);

				ColourFlagExpression = value;
				break;
		}
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref LineExpression);
		CollapseConstantExpression(ref ColumnExpression);
		CollapseConstantExpression(ref ColourFlagExpression);
	}

	public override DataType Type => DataType.Integer;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (LineExpression == null)
			throw new Exception("ScreenFunction with no LineExpression");
		if (ColumnExpression == null)
			throw new Exception("ScreenFunction with no ColumnExpression");

		var line = LineExpression.EvaluateAndCoerceToInt(context, stackFrame);
		var column = ColumnExpression.EvaluateAndCoerceToInt(context, stackFrame);

		if ((line < short.MinValue) || (line > short.MaxValue))
			throw RuntimeException.Overflow(LineExpression.Source);
		if ((column < short.MinValue) || (column > short.MaxValue))
			throw RuntimeException.Overflow(ColumnExpression.Source);

		if ((line < 1) || (line > context.VisualLibrary.CharacterWidth))
			throw RuntimeException.IllegalFunctionCall(LineExpression.Source);
		if ((column < 1) || (column > context.VisualLibrary.CharacterHeight))
			throw RuntimeException.IllegalFunctionCall(ColumnExpression.Source);

		bool colourFlag = false;

		if (ColourFlagExpression != null)
		{
			var colourFlagValue = ColourFlagExpression.Evaluate(context, stackFrame);

			if ((colourFlagValue is LongVariable longValue)
			 && ((longValue.Value < short.MinValue) || (longValue.Value > short.MaxValue)))
				throw RuntimeException.Overflow(ColourFlagExpression.Source);

			colourFlag = !colourFlagValue.IsZero;
		}

		if (colourFlag)
		{
			// SCREEN is documented to return just the foreground attribute, but in practice
			// it just returns the whole attribute byte.
			return new IntegerVariable(context.VisualLibrary.GetAttribute(column - 1, line - 1));
		}
		else
		{
			// Undocumented: SCREEN will not return NUL characters. A character value of 0
			// gets returned as 32 (space).
			//
			// Documented: In graphics mode, an unrecognized character will be returned as
			// 32 (space). This happens automatically because the underlying graphics
			// library returns 0, but then the previous undocumented function converts the
			// 0 to a 32.
			short ch = context.VisualLibrary.GetCharacter(column - 1, line - 1);

			if (ch == 0)
				ch = 32;

			return new IntegerVariable(ch);
		}
	}
}
