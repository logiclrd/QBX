using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class FilePrintEmitter(ExecutionContext context, OpenFile openFile) : PrintEmitter
{
	public override int CursorX { get; set; }
	public override int Width => openFile.LineWidth;

	public override void Emit(ReadOnlySpan<byte> str)
	{
		if (openFile.IOMode == OpenFileIOMode.Random)
			openFile.WriteToFields(str);
		else
		{
			int cursorX = CursorX;

			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == '\r')
					cursorX = 0;
				else if ((str[i] != 0) && (str[i] != 10))
					cursorX++;
			}

			CursorX = cursorX;

			context.Machine.DOS.Write(
				openFile.FileHandle,
				str,
				out _);
		}
	}
}
