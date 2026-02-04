using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using QBX.Hardware;
using System.Reflection.PortableExecutable;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class LocateStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public Evaluable? RowExpression;
	public Evaluable? ColumnExpression;
	public Evaluable? CursorVisibilityExpression;
	public Evaluable? CursorStartExpression;
	public Evaluable? CursorEndExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if ((RowExpression != null) || (ColumnExpression != null))
		{
			int row, column;

			row = context.VisualLibrary.CursorY + 1;
			column = context.VisualLibrary.CursorX + 1;

			if (RowExpression != null)
				row = RowExpression.EvaluateAndCoerceToInt(context, stackFrame);

			if (ColumnExpression != null)
				column = ColumnExpression.EvaluateAndCoerceToInt(context, stackFrame);

			context.VisualLibrary.MoveCursor(column - 1, row - 1);
		}

		if (CursorVisibilityExpression != null)
		{
			int cursorVisibility = CursorVisibilityExpression.EvaluateAndCoerceToInt(context, stackFrame);

			var textLibrary = context.VisualLibrary as TextLibrary;

			switch (cursorVisibility)
			{
				case 1: textLibrary?.ShowCursor(); break;
				case 0: textLibrary?.HideCursor(); break;
				default: throw RuntimeException.IllegalFunctionCall(Source);
			}
		}

		if ((CursorStartExpression != null) || (CursorEndExpression != null))
		{
			var array = context.Machine.GraphicsArray;

			int cursorStart = array.CRTController.CursorScanStart;
			int cursorEnd = array.CRTController.CursorScanEnd;

			if (CursorStartExpression != null)
			{
				int cursorStartValue = CursorStartExpression.EvaluateAndCoerceToInt(context, stackFrame);

				if (cursorStart != (cursorStart & 31))
					throw RuntimeException.IllegalFunctionCall(Source);
			}

			if (CursorEndExpression != null)
			{
				int cursorEndValue = CursorEndExpression.EvaluateAndCoerceToInt(context, stackFrame);

				if (cursorEnd != (cursorEnd & 31))
					throw RuntimeException.IllegalFunctionCall(Source);
			}

			if (cursorStart > cursorEnd)
				(cursorStart, cursorEnd) = (cursorEnd, cursorStart);

			if (context.VisualLibrary is TextLibrary textLibrary)
				textLibrary.SetCursorScans(cursorStart, cursorEnd);
		}
	}
}
