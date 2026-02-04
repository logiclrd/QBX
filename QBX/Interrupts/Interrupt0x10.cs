using QBX.Hardware;
using System;

namespace QBX.Interrupts;

// Based on Ralf Brown's Interrupt List: https://www.ctyme.com/intr/int-10.htm

public class Interrupt0x10(Machine machine) : InterruptHandler
{
	public enum Function : byte
	{
		SetVideoMode = 0x0000,
		SetTextModeCursorShape = 0x0001,
		SetCursorPosition = 0x0002,
		GetCursorPositionAndSize = 0x0003,
		ReadLightPenPosition = 0x0004,
		SelectActiveDisplayPage = 0x0005,
		ScrollUpWindow = 0x0006,
		ScrollDownWindow = 0x0007,
	}

	public override Registers Execute(Registers input)
	{
		byte ah = unchecked((byte)(input.AX >> 8));
		byte al = unchecked((byte)input.AX);

		var function = (Function)ah;

		var result = input;

		switch (function)
		{
			case Function.SetVideoMode:
			{
				if (machine.VideoFirmware.SetMode(al))
				{
					result.AX &= 0xFF00;

					if (al > 7)
						result.AX |= 0x20;
					else if (al == 6)
						result.AX |= 0x3F;
					else
						result.AX |= 0x30;
				}

				break;
			}
			case Function.SetTextModeCursorShape:
			{
				int cursorStart = result.CX >> 8; // CH
				int cursorEnd = result.CX & 0xFF; // CL

				bool cursorVisible = (cursorStart & 96) == 0;

				cursorStart &= 31;

				machine.VideoFirmware.SetCursorScans(cursorStart, cursorEnd, cursorVisible);

				break;
			}
			case Function.SetCursorPosition:
			{
				int pageNumber = result.BX >> 8; // BH;

				if (!machine.VideoFirmware.IsTextMode
				 && (pageNumber > 0))
					break;

				int row = result.DX >> 8; // DH
				int column = result.DX & 0xFF; // DL

				machine.VideoFirmware.MoveCursor(column, row, pageNumber);

				break;
			}
			case Function.GetCursorPositionAndSize:
			{
				int pageNumber = result.BX >> 8; // BH;

				if (!machine.VideoFirmware.IsTextMode
				 && (pageNumber > 0))
					break;

				(int startScan, int endScan) = machine.VideoFirmware.GetCursorScans();
				(int cursorX, int cursorY) = machine.VideoFirmware.GetCursorPosition();

				result.CX = unchecked((ushort)((startScan << 8) | endScan));
				result.DX = unchecked((ushort)((cursorY << 8) | cursorX));

				break;
			}
			case Function.ReadLightPenPosition:
			{
				result.AX = machine.MouseDriver.LightPenIsDown ? (ushort)0x100 : (ushort)0x000;

				var lightPenPosition = machine.MouseDriver.LightPenEndPosition;

				int lightPenCharacterX = lightPenPosition.X / 8;
				int lightPenCharacterY = lightPenPosition.Y / 8;

				result.BX = unchecked((ushort)lightPenPosition.X);

				if (machine.MouseDriver.PointerMaximumY < 256)
					result.CX = unchecked((ushort)(lightPenPosition.Y << 8)); // weirdos using only CH instead of only CL
				else
					result.CX = unchecked((ushort)lightPenPosition.Y);

				result.DX = unchecked((ushort)((lightPenCharacterY << 8) | lightPenCharacterX));

				break;
			}
			case Function.SelectActiveDisplayPage:
			{
				machine.VideoFirmware.VisualLibrary.SetActivePage(al);

				break;
			}
			case Function.ScrollUpWindow:
			case Function.ScrollDownWindow:
			{
				int numLines = al;

				if (function == Function.ScrollUpWindow)
					numLines = -numLines;

				if (numLines == 0)
					numLines = machine.VideoFirmware.VisualLibrary.CharacterHeight;

				int fillAttribute = input.BX >> 8;

				int x1 = input.CX & 0xFF;
				int y1 = input.CX >> 8;

				int x2 = input.DX & 0xFF;
				int y2 = input.DX >> 8;

				machine.VideoFirmware.VisualLibrary.ScrollTextWindow(x1, y1, x2, y2, numLines, fillAttribute);

				break;
			}
			// TODO: moar functions
		}

		throw new NotImplementedException();
	}
}
