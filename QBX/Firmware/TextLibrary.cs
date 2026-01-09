using System;

using QBX.Hardware;

namespace QBX.Firmware;

public class TextLibrary : VisualLibrary
{
	public TextLibrary(Machine machine)
		: base(machine)
	{
	}

	public int CursorAddress => CursorY * Width + CursorX;

	public bool MovePhysicalCursor = true;

	public byte Attributes = 7;
	public bool EnableWriteAttributes = true;

	public override void RefreshParameters()
	{
		if (Array.CRTController.CharacterHeight == 0)
			return;

		Width = Array.CRTController.Registers.EndHorizontalDisplay + 1;
		Height = Array.CRTController.NumScanLines / Array.CRTController.CharacterHeight;

		CharacterWidth = Width;
		CharacterHeight = Height;

		ReloadCursorAddress();
	}

	public void ReloadCursorAddress()
	{
		int cursorAddress = Array.CRTController.CursorAddress;

		CursorY = cursorAddress / Width;
		CursorX = cursorAddress % Width;
	}

	public void SetAttributes(int foreground, int background)
	{
		Attributes = unchecked((byte)((foreground & 15) | ((background & 15) << 4)));
	}

	public void SetForegroundAttribute(int foreground)
	{
		Attributes = unchecked((byte)((foreground & 15) | (Attributes & 0xF0)));
	}

	public void SetBackgroundAttribute(int background)
	{
		Attributes = unchecked((byte)((Attributes & 0x0F) | ((background & 15) << 4)));
	}

	public void ShowCursor()
	{
		Array.CRTController.Registers[GraphicsArray.CRTControllerRegisters.CursorStart]
			&= unchecked((byte)~GraphicsArray.CRTControllerRegisters.CursorStart_Disable);
	}

	public void HideCursor()
	{
		Array.CRTController.Registers[GraphicsArray.CRTControllerRegisters.CursorStart]
			|= GraphicsArray.CRTControllerRegisters.CursorStart_Disable;
	}

	public void SetCursorScans(int start, int end)
	{
		Array.CRTController.Registers[GraphicsArray.CRTControllerRegisters.CursorStart] = unchecked((byte)(
			(Array.CRTController.Registers[GraphicsArray.CRTControllerRegisters.CursorStart]
				& ~GraphicsArray.CRTControllerRegisters.CursorStart_Mask) |
			start));

		Array.CRTController.Registers[GraphicsArray.CRTControllerRegisters.CursorEnd] = unchecked((byte)(
			(Array.CRTController.Registers[GraphicsArray.CRTControllerRegisters.CursorEnd]
				& ~GraphicsArray.CRTControllerRegisters.CursorEnd_Mask) |
			end));
	}

	protected override void MoveCursorHandlePhysicalCursor()
	{
		if (MovePhysicalCursor)
			UpdatePhysicalCursor();
	}

	public void UpdatePhysicalCursor()
	{
		int cursorAddress = CursorY * Width + CursorX;

		Array.OutPort2(
			GraphicsArray.CRTControllerRegisters.IndexPort,
			GraphicsArray.CRTControllerRegisters.CursorLocationLow,
			unchecked((byte)(cursorAddress & 0xFF)));
		Array.OutPort2(
			GraphicsArray.CRTControllerRegisters.IndexPort,
			GraphicsArray.CRTControllerRegisters.CursorLocationHigh,
			unchecked((byte)((cursorAddress >> 8) & 0xFF)));
	}

	protected override void ClearImplementation()
	{
		int planeBytesUsed = Width * Height;

		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(StartAddress);

		var plane0 = vramSpan.Slice(0x00000, planeBytesUsed);
		var plane1 = vramSpan.Slice(0x10000, planeBytesUsed);

		plane0.Clear();
		plane1.Fill(Attributes);
	}

	public void WriteAttributesAt(int x, int y, int charCount)
	{
		MoveCursor(x, y);
		WriteAttributes(charCount);
	}

	public override void WriteText(ReadOnlySpan<byte> buffer)
	{
		int o = CursorAddress;

		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(StartAddress);

		var plane0 = vramSpan.Slice(0x00000, 0x10000);
		var plane1 = vramSpan.Slice(0x10000, 0x10000);

		int cursorX = CursorX;
		int cursorY = CursorY;

		var attributes = Attributes;

		while (!buffer.IsEmpty)
		{
			int remainingChars = Width - cursorX;

			int spanLength = Math.Min(buffer.Length, remainingChars);

			for (int i = 0; i < spanLength; i++)
			{
				plane0[o] = buffer[i];
				if (EnableWriteAttributes)
					plane1[o] = attributes;

				o++;
				cursorX++;
			}

			buffer = buffer.Slice(spanLength);

			if (!buffer.IsEmpty)
			{
				cursorX = 0;

				if (cursorY + 1 < Height)
					cursorY++;
				else
				{
					ScrollText(plane0, plane1);
					o -= Width;
				}
			}
		}

		MoveCursor(cursorX, cursorY);
	}

	public override void ScrollText()
	{
		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(StartAddress);

		var plane0 = vramSpan.Slice(0x00000, 0x10000);
		var plane1 = vramSpan.Slice(0x10000, 0x10000);

		ScrollText(plane0, plane1);
	}

	void ScrollText(Span<byte> plane0, Span<byte> plane1)
	{
		plane0.Slice(Width).CopyTo(plane0);
		plane1.Slice(Width).CopyTo(plane1);
	}

	public void WriteAttributes(int charCount)
	{
		int o = CursorAddress;

		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(StartAddress);

		var plane1 = vramSpan.Slice(0x10000, 0x10000);

		int cursorX = CursorX;
		int cursorY = CursorY;

		var attributes = Attributes;

		while (charCount > 0)
		{
			int remainingChars = Width - cursorX;

			int spanLength = Math.Min(charCount, remainingChars);

			for (int i = 0; i < spanLength; i++)
			{
				plane1[o] = attributes;

				o++;
				cursorX++;
			}

			charCount -= spanLength;

			if (charCount > 0)
			{
				if (cursorY + 1 < Height)
					cursorY++;
				else
					break;

				cursorX = 0;
			}
		}

		MoveCursor(cursorX, cursorY);
	}
}
