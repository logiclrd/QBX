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
			{
				var rowValue = RowExpression.Evaluate(context, stackFrame);

				row = rowValue.CoerceToInt();
			}

			if (ColumnExpression != null)
			{
				var columnValue = ColumnExpression.Evaluate(context, stackFrame);

				column = columnValue.CoerceToInt();
			}

			context.VisualLibrary.MoveCursor(column - 1, row - 1);
		}

		if (CursorVisibilityExpression != null)
		{
			var cursorVisibilityValue = CursorVisibilityExpression.Evaluate(context, stackFrame);

			int cursorVisibility = cursorVisibilityValue.CoerceToInt();

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
				var cursorStartValue = CursorStartExpression.Evaluate(context, stackFrame);

				cursorStart = cursorStartValue.CoerceToInt();

				if (cursorStart != (cursorStart & 31))
					throw RuntimeException.IllegalFunctionCall(Source);
			}

			if (CursorEndExpression != null)
			{
				var cursorEndValue = CursorEndExpression.Evaluate(context, stackFrame);

				cursorEnd = cursorEndValue.CoerceToInt();

				if (cursorEnd != (cursorEnd & 31))
					throw RuntimeException.IllegalFunctionCall(Source);
			}

			if (cursorStart > cursorEnd)
				(cursorStart, cursorEnd) = (cursorEnd, cursorStart);

			byte cursorStartRegisterValue = unchecked((byte)(
				cursorStart |
				(array.CRTController.CursorVisible
				? 0
				: GraphicsArray.CRTControllerRegisters.CursorStart_Disable)));

			byte cursorEndRegisterValue = unchecked((byte)cursorEnd);

			array.OutPort2(
				GraphicsArray.CRTControllerRegisters.IndexPort,
				GraphicsArray.CRTControllerRegisters.CursorStart,
				cursorStartRegisterValue);

			array.OutPort2(
				GraphicsArray.CRTControllerRegisters.IndexPort,
				GraphicsArray.CRTControllerRegisters.CursorEnd,
				cursorEndRegisterValue);
		}
	}
}
