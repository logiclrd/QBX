using QBX.ExecutionEngine.Execution;
using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ClsStatement(CodeModel.Statements.ClsStatement source) : Executable(source)
{
	public Evaluable? ArgumentExpression;

	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		int mode;
		bool redrawSoftKeyMacroLine = false;

		if (ArgumentExpression != null)
			mode = ArgumentExpression.EvaluateAndCoerceToInt(context, stackFrame);
		else
		{
			redrawSoftKeyMacroLine = true;

			if (context.VisualLibrary is TextLibrary)
				mode = 2;
			else
				mode = 1;
		}

		switch (mode)
		{
			case 0:
			{
				// In graphics modes, clear all pixels.
				//
				// In text modes, clear everything. (Documentation says except the last row,
				// but that doesn't seem to be correct empirically.)
				//
				// Return the cursor to the top-left of the character line window.

				if (context.VisualLibrary is GraphicsLibrary)
					context.VisualLibrary.Clear();
				else
				{
					int savedWindowStart = context.VisualLibrary.CharacterLineWindowStart;
					int savedWindowEnd = context.VisualLibrary.CharacterLineWindowEnd;

					context.VisualLibrary.UpdateCharacterLineWindow(
						0,
						context.VisualLibrary.CharacterHeight - 1);

					context.VisualLibrary.ClearCharacterLineWindow();

					context.VisualLibrary.UpdateCharacterLineWindow(
						savedWindowStart,
						savedWindowEnd);
				}

				context.VisualLibrary.MoveCursor(0, 0);

				break;
			}
			case 1:
			{
				// In graphics modes, clear all pixels.
				// In text modes, do nothing.
				if (context.VisualLibrary is GraphicsLibrary)
				{
					context.VisualLibrary.Clear();
					context.VisualLibrary.MoveCursor(0, 0);
				}

				break;
			}
			case 2:
			{
				// In both graphics and text modes, clear the character line window.
				context.VisualLibrary.ClearCharacterLineWindow();
				context.VisualLibrary.MoveCursor(0, 0);

				break;
			}

			default:
				throw RuntimeException.IllegalFunctionCall(Source);
		}

		if (redrawSoftKeyMacroLine)
			context.RuntimeState.RenderSoftKeyMacroLine(context.VisualLibrary);

		if (context.VisualLibrary is TextLibrary textLibrary)
		{
			if (context.RuntimeState.TextCursorVisible)
				textLibrary.ShowCursor();
			else
				textLibrary.HideCursor();
		}
	}
}
