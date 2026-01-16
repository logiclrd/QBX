using System;
using System.Reflection.Metadata;
using QBX.Firmware;

namespace QBX.ExecutionEngine.Execution;

public class RuntimeState
{
	public int SegmentBase;
	public bool EnablePaletteRemapping = true;
	public StringValue?[] SoftKeyMacros = new StringValue?[12];
	public bool DisplaySoftKeyMacroLine = false;

	public void RenderSoftKeyMacroLine(VisualLibrary visualLibrary)
	{
		if (DisplaySoftKeyMacroLine)
		{
			int savedWindowStart = visualLibrary.CharacterLineWindowStart;
			int savedWindowEnd = visualLibrary.CharacterLineWindowEnd;
			int savedCursorX = visualLibrary.CursorX;
			int savedCursorY = visualLibrary.CursorY;

			try
			{
				var textLibrary = visualLibrary as TextLibrary;

				visualLibrary.UpdateCharacterLineWindow(
					visualLibrary.CharacterHeight - 1,
					visualLibrary.CharacterHeight - 1);

				visualLibrary.ClearCharacterLineWindow();

				for (int x = 0, i = 0; x < visualLibrary.CharacterWidth; x += 8, i++)
				{
					int n = i + 1;

					textLibrary?.SetAttributes(7, 0);

					int showChars;

					if (n == 10)
					{
						visualLibrary.WriteText("10");
						showChars = 5;
					}
					else
					{
						showChars = 6;
						visualLibrary.WriteText((byte)('0' + n));
					}

					var macro = SoftKeyMacros[i];

					if (StringValue.IsNullOrEmpty(macro))
						visualLibrary.WriteText("        ".AsSpan().Slice(7 - showChars));
					else
					{
						var macroSpan = macro.AsSpan();

						if (macroSpan.Length > showChars)
							macroSpan = macroSpan.Slice(0, showChars);

						textLibrary?.SetAttributes(0, 7);

						for (int j = 0; j < macroSpan.Length; j++)
						{
							switch (macroSpan[j])
							{
								case 7: visualLibrary.WriteText(14); break;
								case 8: visualLibrary.WriteText(254); break;
								case 9: visualLibrary.WriteText(26); break;
								case 10: visualLibrary.WriteText(60); break;
								case 11: visualLibrary.WriteText(127); break;
								case 12: visualLibrary.WriteText(22); break;
								case 13: visualLibrary.WriteText(27); break;
								case 28: visualLibrary.WriteText(16); break;
								case 29: visualLibrary.WriteText(17); break;

								default: visualLibrary.WriteText(macroSpan[j]); break;
							}
						}

						int padRight = showChars - macroSpan.Length;

						visualLibrary.WriteText("       ".AsSpan().Slice(7 - padRight));

						textLibrary?.SetAttributes(7, 0);

						if (x + 8 < visualLibrary.CharacterWidth)
							visualLibrary.WriteText((byte)' ');
					}
				}

				if (savedWindowEnd + 1 >= visualLibrary.Height)
					savedWindowEnd--;
			}
			finally
			{
				visualLibrary.UpdateCharacterLineWindow(savedWindowStart, savedWindowEnd);
				visualLibrary.MoveCursor(savedCursorX, savedCursorY);
			}
		}
	}

	public RuntimeState()
	{
		SegmentBase = GetDataSegmentBase();
	}

	public int GetDataSegmentBase()
	{
		return 0x40000;
	}
}
