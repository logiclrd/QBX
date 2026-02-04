using System;

using QBX.Hardware;
using QBX.Utility;

namespace QBX.Firmware;

public class TextLibrary : VisualLibrary
{
	public TextLibrary(Machine machine)
		: base(machine)
	{
		machine.MouseDriver.TextPointerAppearanceChanged +=
			() =>
			{
				if (machine.MouseDriver.TextPointerEnableSoftware)
				{
					_pointerCharacterMask = machine.MouseDriver.TextPointerCharacterMask;
					_pointerCharacterInvert = machine.MouseDriver.TextPointerCharacterInvert;
					_pointerAttributeMask = machine.MouseDriver.TextPointerAttributeMask;
					_pointerAttributeInvert = machine.MouseDriver.TextPointerAttributeInvert;

					DrawPointer();
				}
				else
					UndrawPointer();
			};
	}

	public int CursorAddress => CursorY * Width + CursorX;

	public bool MovePhysicalCursor = true;

	public byte Attributes = 7;
	public bool EnableWriteAttributes = true;

	IntegerRect _clipRect =
		new IntegerRect()
		{
			X1 = int.MinValue,
			Y1 = int.MinValue,
			X2 = int.MaxValue,
			Y2 = int.MaxValue,
		};

	public override void RefreshParameters()
	{
		if (Array.CRTController.CharacterHeight == 0)
			return;

		Width = Array.CRTController.Registers.EndHorizontalDisplay + 1;
		Height = Array.CRTController.NumScanLines / Array.CRTController.CharacterHeight;

		CharacterWidth = Width;
		CharacterHeight = Height;

		base.RefreshParameters();

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

	public void MoveCursorWithinClip(int x, int y)
	{
		if (x > _clipRect.X2)
			x = _clipRect.X2;
		if (x < _clipRect.X1)
			x = _clipRect.X1;

		if (y > _clipRect.Y2)
			y = _clipRect.Y2;
		if (y < _clipRect.Y1)
			y = _clipRect.Y1;

		MoveCursor(x, y);
	}

	protected override void MoveCursorHandlePhysicalCursor()
	{
		// Check if the mouse driver has hijacked the cursor.
		if ((Machine.MouseDriver.TextPointerEnableSoftware == false)
		 && Machine.MouseDriver.PointerVisible)
			return;

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

	class ClipScope(TextLibrary owner, IntegerRect previousRect) : IDisposable
	{
		bool _isDisposed;

		public void Dispose()
		{
			if (!_isDisposed)
			{
				owner.SetClipRect(previousRect);
				_isDisposed = true;
			}
		}
	}

	void SetClipRect(IntegerRect newClipRect)
	{
		_clipRect = newClipRect;
	}

	public IDisposable PushClipRect(int x1, int y1, int x2, int y2)
		=> PushClipRect(new IntegerRect(x1, y1, x2, y2));

	public IDisposable PushClipRect(IntegerRect newClipRect)
	{
		var previousRect = _clipRect;

		if (newClipRect.X1 < previousRect.X1)
			newClipRect.X1 = previousRect.X1;
		if (newClipRect.Y1 < previousRect.Y1)
			newClipRect.Y1 = previousRect.Y1;
		if (newClipRect.X2 > previousRect.X2)
			newClipRect.X2 = previousRect.X2;
		if (newClipRect.Y2 > previousRect.Y2)
			newClipRect.Y2 = previousRect.Y2;

		SetClipRect(newClipRect);

		return new ClipScope(this, previousRect);
	}

	protected override void ClearImplementation(int fromCharacterLine = 0, int toCharacterLine = -1)
	{
		int planeBytesUsed = Width * Height;

		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(StartAddress);

		if (fromCharacterLine < 0)
			fromCharacterLine = CharacterHeight - 1;
		if (fromCharacterLine < _clipRect.Y1)
			fromCharacterLine = _clipRect.Y1;

		if (toCharacterLine >= CharacterHeight)
			toCharacterLine = CharacterHeight - 1;
		if (toCharacterLine >= _clipRect.Y2)
			toCharacterLine = _clipRect.Y2;

		if (toCharacterLine < fromCharacterLine)
			return;

		int x1 = Math.Max(0, _clipRect.X1);
		int x2 = Math.Min(CharacterWidth - 1, _clipRect.X2);

		int width = x2 - x1 + 1;

		using (HidePointerForOperationIfPointerAware())
		{
			if (width == CharacterWidth)
			{
				int windowStart = fromCharacterLine * Width;
				int windowLength = (toCharacterLine - fromCharacterLine + 1) * Width;

				if (windowStart + windowLength > planeBytesUsed)
					windowLength = planeBytesUsed - windowStart;

				if (windowLength <= 0)
					return;

				var plane0 = vramSpan.Slice(0x00000 + windowStart, windowLength);
				var plane1 = vramSpan.Slice(0x10000 + windowStart, windowLength);

				plane0.Clear();
				plane1.Fill(Attributes);
			}
			else
			{
				int windowStart = fromCharacterLine * Width + x1;
				int windowLength = width;

				for (int y = _clipRect.Y1; y <= _clipRect.Y2; y++)
				{
					if (windowStart + windowLength > planeBytesUsed)
						windowLength = planeBytesUsed - windowStart;

					if (windowLength <= 0)
						return;

					var plane0 = vramSpan.Slice(0x00000 + windowStart, windowLength);
					var plane1 = vramSpan.Slice(0x10000 + windowStart, windowLength);

					plane0.Clear();
					plane1.Fill(Attributes);
				}
			}
		}
	}

	public void WriteAttributesAt(int x, int y, int charCount)
	{
		MoveCursor(x, y);

		if (_clipRect.Contains(x, y))
		{
			using (HidePointerForOperationIfPointerAware())
				WriteAttributes(charCount);
		}
	}

	static byte[] ControlCharacters = [7, 9, 10, 11, 12, 13, 28, 29, 30, 31];

	public override void WriteText(ReadOnlySpan<byte> buffer)
	{
		if (buffer.Length == 0)
			return;

		ResolvePassiveNewLine();

		int o = CursorAddress;

		int startAddress = StartAddress;
		int windowEndOffset = (CharacterLineWindowEnd + 1) * Width;

		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(startAddress);

		var plane0 = vramSpan.Slice(0x00000, windowEndOffset);
		var plane1 = vramSpan.Slice(0x10000, windowEndOffset);

		int scrollOffset = 0;
		bool clearLastLineOnScroll = true;

		if (CharacterLineWindowStart < _clipRect.Y1)
		{
			int difference = _clipRect.Y1 - CharacterLineWindowStart;

			scrollOffset += difference * Width;
		}

		if (CharacterLineWindowEnd > _clipRect.Y2)
		{
			int difference = CharacterLineWindowEnd - _clipRect.Y2;

			plane0 = plane0.Slice(0, plane0.Length - (difference - 1) * Width);
			plane1 = plane1.Slice(0, plane1.Length - (difference - 1) * Width);
			clearLastLineOnScroll = false;
		}

		int cursorX = CursorX;
		int cursorY = CursorY;

		var attributes = Attributes;

		using (HidePointerForOperationIfPointerAware())
		{
			void BeginNewLine()
			{
				cursorX = 0;

				if (cursorY + 1 <= CharacterLineWindowEnd)
					cursorY++;
				else
				{
					Span<byte> vramSpan = Array.VRAM;

					vramSpan = vramSpan.Slice(startAddress);

					var plane0 = vramSpan.Slice(0x00000, windowEndOffset);
					var plane1 = vramSpan.Slice(0x10000, windowEndOffset);

					ScrollText(
						plane0.Slice(scrollOffset, plane0.Length - scrollOffset),
						plane1.Slice(scrollOffset, plane1.Length - scrollOffset),
						clearLastLineOnScroll);

					o -= Width;
				}
			}

			while (!buffer.IsEmpty)
			{
				switch (buffer[0])
				{
					case 7: // BEL
						Machine.Speaker.ChangeSound(true, false, frequency: 1000, false, hold: TimeSpan.FromMilliseconds(200));
						Machine.Speaker.ChangeSound(false, false, frequency: 1000, false);

						buffer = buffer.Slice(1);
						continue;
					case 9: // TAB
						do
						{
							plane0[o] = 32;
							if (EnableWriteAttributes)
								plane1[o] = attributes;
							cursorX++;
						} while ((cursorX < CharacterWidth) && ((cursorX & 7) != 0));

						if (cursorX == CharacterWidth)
							BeginNewLine();

						buffer = buffer.Slice(1);
						continue;
					case 10: // LF
					case 13: // CR
						BeginNewLine();

						buffer = buffer.Slice(1);
						continue;
					case 11: // VT (Vertical Tab)
						cursorX = 0;
						cursorY = CharacterLineWindowStart;
						o = cursorY * CharacterWidth;

						buffer = buffer.Slice(1);
						continue;
					case 12: // FF (Form Food)
						Clear();
						cursorX = 0;
						cursorY = CharacterLineWindowStart;
						o = cursorY * CharacterWidth;

						buffer = buffer.Slice(1);
						continue;
					case 28: // cursor right
						if (cursorX + 1 < windowEndOffset)
						{
							cursorX++;
							o++;
						}

						buffer = buffer.Slice(1);
						continue;
					case 29: // cursor left
						if ((cursorX > 0) || (cursorY > CharacterLineWindowStart))
						{
							cursorX--;

							if (cursorX < 0)
							{
								cursorX += CharacterWidth;
								cursorY--;
							}

							o--;
						}

						buffer = buffer.Slice(1);
						continue;
					case 30: // up
						if (cursorY > CharacterLineWindowStart)
						{
							cursorY--;
							o -= CharacterWidth;
						}

						buffer = buffer.Slice(1);
						continue;
					case 31: // down
						if (cursorY < CharacterLineWindowEnd)
							cursorY++;
						else
							ScrollText();

						buffer = buffer.Slice(1);
						continue;
				}

				int remainingChars = Width - cursorX;

				int spanLength = Math.Min(buffer.Length, remainingChars);

				int controlCharacterOffset = buffer.Slice(0, spanLength).IndexOfAny(ControlCharacters);

				if (controlCharacterOffset >= 0)
					spanLength = controlCharacterOffset;

				for (int i = 0; i < spanLength; i++)
				{
					if (_clipRect.Contains(cursorX, cursorY))
					{
						plane0[o] = buffer[i];
						if (EnableWriteAttributes)
							plane1[o] = attributes;
					}

					o++;
					cursorX++;
				}

				buffer = buffer.Slice(spanLength);

				if (!buffer.IsEmpty)
					BeginNewLine();
			}

			if (cursorX < CharacterWidth)
				MoveCursor(cursorX, cursorY);
			else
				PassiveNewLine();
		}
	}

	public void WriteAttributes(int charCount)
	{
		int o = CursorAddress;

		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(StartAddress);

		var plane1 = vramSpan.Slice(0x10000, (CharacterLineWindowEnd + 1) * Width);

		int scrollOffset = 0;
		bool clearLastLineOnScroll = true;

		if (CharacterLineWindowStart < _clipRect.Y1)
		{
			int difference = _clipRect.Y1 - CharacterLineWindowStart;

			scrollOffset += difference * Width;
		}

		if (CharacterLineWindowEnd > _clipRect.Y2)
		{
			int difference = CharacterLineWindowEnd - _clipRect.Y2;

			plane1 = plane1.Slice(0, plane1.Length - (difference - 1) * Width);
			clearLastLineOnScroll = false;
		}

		int cursorX = CursorX;
		int cursorY = CursorY;

		var attributes = Attributes;

		using (HidePointerForOperationIfPointerAware())
		{
			while (charCount > 0)
			{
				int remainingChars = Width - cursorX;

				int spanLength = Math.Min(charCount, remainingChars);

				for (int i = 0; i < spanLength; i++)
				{
					if (_clipRect.Contains(cursorX, cursorY))
						plane1[o] = attributes;

					o++;
					cursorX++;
				}

				charCount -= spanLength;

				if (charCount > 0)
				{
					cursorX = 0;

					if (cursorY + 1 <= CharacterLineWindowEnd)
						cursorY++;
					else
					{
						ScrollText(Span<byte>.Empty, plane1.Slice(scrollOffset, plane1.Length - scrollOffset), clearLastLineOnScroll);
						o -= Width;
					}
				}
			}

			MoveCursor(cursorX, cursorY);
		}
	}

	public byte GetCharacter(int x, int y)
	{
		if ((x < 0) || (x >= CharacterWidth)
		 || (y < 0) || (y >= CharacterHeight))
			return 0;

		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(StartAddress);

		var plane0 = vramSpan.Slice(0, CharacterWidth * CharacterHeight);

		int offset = y * CharacterWidth + x;

		return plane0[offset];
	}

	public byte GetAttribute(int x, int y)
	{
		if ((x < 0) || (x >= CharacterWidth)
		 || (y < 0) || (y >= CharacterHeight))
			return 0;

		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(StartAddress);

		var plane1 = vramSpan.Slice(0x10000, (CharacterLineWindowEnd + 1) * Width);

		int offset = y * CharacterWidth + x;

		return plane1[offset];
	}

	public override void ScrollText()
	{
		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(StartAddress);

		var plane0 = vramSpan.Slice(0x00000, 0x10000);
		var plane1 = vramSpan.Slice(0x10000, 0x10000);

		int characterLineWindowLines = CharacterLineWindowEnd - CharacterLineWindowStart + 1;

		int windowOffset = CharacterLineWindowStart * Width;
		int windowLength = characterLineWindowLines * Width;

		bool clearLastLine = true;

		if (CharacterLineWindowStart < _clipRect.Y1)
		{
			int difference = _clipRect.Y1 - CharacterLineWindowStart;

			windowOffset += difference * Width;
			windowLength -= difference * Width;
		}

		if (CharacterLineWindowEnd > _clipRect.Y2)
		{
			int difference = _clipRect.Y2 - CharacterLineWindowEnd - 1;

			if (difference > 0)
			{
				windowLength -= difference * Width;
				clearLastLine = false;
			}
		}

		if (windowOffset + windowLength > plane0.Length)
			windowLength = plane0.Length - windowOffset;

		if (windowLength > 0)
		{
			plane0 = plane0.Slice(windowOffset, windowLength);
			plane1 = plane1.Slice(windowOffset, windowLength);

			ScrollText(plane0, plane1, clearLastLine);
		}
	}

	void ScrollText(Span<byte> plane0, Span<byte> plane1, bool clearLastLine)
	{
		using (HidePointerForOperationIfPointerAware())
		{
			int x1 = Math.Max(0, _clipRect.X1);
			int x2 = Math.Min(CharacterWidth - 1, _clipRect.X2);

			int width = x2 - x1 + 1;

			if (width == Width)
			{
				if (plane0.Length > Width)
				{
					plane0.Slice(Width).CopyTo(plane0);
					if (clearLastLine)
						plane0.Slice(plane0.Length - Width).Fill((byte)' ');
				}
				else if (clearLastLine)
					plane0.Fill((byte)' ');

				if (plane1.Length > Width)
				{
					plane1.Slice(Width).CopyTo(plane1);
					if (clearLastLine)
						plane1.Slice(plane1.Length - Width).Fill(Attributes);
				}
				else if (clearLastLine)
					plane1.Fill(Attributes);
			}
			else
			{
				while (plane0.Length > Width)
				{
					plane0.Slice(x1 + Width, width).CopyTo(plane0.Slice(x1));
					plane0 = plane0.Slice(Width);
				}

				if (clearLastLine)
					plane0.Slice(x1, width).Fill((byte)' ');

				while (plane1.Length > Width)
				{
					plane1.Slice(x1 + Width, width).CopyTo(plane1.Slice(x1));
					plane1 = plane1.Slice(Width);
				}

				if (clearLastLine)
					plane1.Slice(x1, width).Fill(Attributes);
			}
		}
	}

	byte _pointerCharacterMask;
	byte _pointerCharacterInvert;
	byte _pointerAttributeMask;
	byte _pointerAttributeInvert;

	byte _pointerSavedCharacter;
	byte _pointerSavedAttribute;
	int _pointerSavedOffset;

	protected override void DrawPointer()
	{
		if (Machine.MouseDriver.PointerVisible && Machine.MouseDriver.TextPointerEnableSoftware && !PointerIsDrawn)
		{
			int pointerCharacterX = Machine.MouseDriver.PointerX / 8;
			int pointerCharacterY = Machine.MouseDriver.PointerY / 8;

			int pointerOffset = pointerCharacterY * CharacterWidth + pointerCharacterX;

			Span<byte> vramSpan = Array.VRAM;

			vramSpan = vramSpan.Slice(StartAddress);

			var plane0 = vramSpan;
			var plane1 = vramSpan.Slice(0x10000);

			_pointerSavedCharacter = plane0[pointerOffset];
			_pointerSavedAttribute = plane1[pointerOffset];
			_pointerSavedOffset = pointerOffset;

			PointerRect.X1 = pointerCharacterX * Array.Sequencer.CharacterWidth;
			PointerRect.Y1 = pointerCharacterY * Array.CRTController.CharacterHeight;
			PointerRect.X2 = PointerRect.X1 + Array.Sequencer.CharacterWidth - 1;
			PointerRect.Y2 = PointerRect.Y1 + Array.CRTController.CharacterHeight - 1;

			plane0[pointerOffset] = unchecked((byte)(
				(plane0[pointerOffset] & _pointerCharacterMask) ^ _pointerCharacterInvert));
			plane1[pointerOffset] = unchecked((byte)(
				(plane1[pointerOffset] & _pointerAttributeMask) ^ _pointerAttributeInvert));

			PointerIsDrawn = true;
		}
	}

	protected override void UndrawPointer()
	{
		if (PointerIsDrawn)
		{
			Span<byte> vramSpan = Array.VRAM;

			var plane0 = vramSpan.Slice(StartAddress);
			var plane1 = vramSpan.Slice(StartAddress + 0x10000);

			plane0[_pointerSavedOffset] = _pointerSavedCharacter;
			plane1[_pointerSavedOffset] = _pointerSavedAttribute;
			PointerIsDrawn = false;
		}
	}
}
