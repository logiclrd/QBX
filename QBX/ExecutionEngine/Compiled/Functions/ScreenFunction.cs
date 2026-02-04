using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;

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

		if (context.VisualLibrary is TextLibrary textLibrary)
		{
			// SCREEN is documented to return just the foreground attribute, but in practice
			// it just returns the whole attribute byte.
			if (colourFlag)
				return new IntegerVariable(textLibrary.GetAttribute(column - 1, line - 1));
			else
				return new IntegerVariable(textLibrary.GetCharacter(column - 1, line - 1));
		}
		else if (context.VisualLibrary is GraphicsLibrary graphicsLibrary)
		{
			if (colourFlag)
				return new IntegerVariable(0);

			int x = (column - 1) * 8;
			int y = (line - 1) * graphicsLibrary.CharacterScans;

			byte[] pixels = new byte[graphicsLibrary.CharacterScans];

			// TODO: Come up with a faster way to do this. Could use GetSprite.
			for (int yy = 0; yy < pixels.Length; yy++)
			{
				int pixelValue = 0;

				for (int xx = 0; xx < 8; xx++)
					pixelValue = (pixelValue << 1) | (graphicsLibrary.PixelGet(x + xx, y + yy) != 0 ? 1 : 0);

				pixels[yy] = unchecked((byte)pixelValue);
			}

			for (int i = 1; i < graphicsLibrary.Font.Length; i++)
				if (graphicsLibrary.Font[i].SequenceEqual(pixels))
					return new IntegerVariable((short)i);

			return new IntegerVariable(32);
		}
		else
			throw new Exception("Internal error");
	}
}
