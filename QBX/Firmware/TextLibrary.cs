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

	int _stride;

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

		_stride = Width * 2;

		CharacterWidth = Width;
		CharacterHeight = Height;

		base.RefreshParameters();

		ReloadCursorAddress();
	}

	public override byte CurrentAttributeByte
	{
		get => Attributes;
		set => Attributes = value;
	}

	public void ReloadCursorAddress()
	{
		(CursorX, CursorY) = Machine.VideoFirmware.GetCursorPosition(ActivePageNumber);
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

	class CursorVisibleScope(TextLibrary owner) : IDisposable
	{
		public void Dispose() => owner.HideCursor();
	}

	public IDisposable? ShowCursorForScope()
	{
		if (IsCursorVisible)
			return null;

		ShowCursor();

		return new CursorVisibleScope(this);
	}

	class CursorHiddenScope(TextLibrary owner) : IDisposable
	{
		public void Dispose() => owner.ShowCursor();
	}

	public IDisposable? HideCursorForScope()
	{
		if (!IsCursorVisible)
			return null;

		HideCursor();

		return new CursorHiddenScope(this);
	}

	public bool IsCursorVisible
	{
		get
		{
			return Array.CRTController.CursorVisible;
		}
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

	public void SetCursorScans(int newStart, int newEnd)
		=> Machine.VideoFirmware.SetCursorScans(newStart, newEnd);

	public void SetCursorScans(int newStart, int newEnd, bool newVisible)
		=> Machine.VideoFirmware.SetCursorScans(newStart, newEnd, newVisible);

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
		Machine.VideoFirmware.MoveCursor(CursorX, CursorY, ActivePageNumber);
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
				int windowStart = fromCharacterLine * _stride;
				int windowLength = (toCharacterLine - fromCharacterLine + 1) * _stride;

				if (windowStart + windowLength > planeBytesUsed)
					windowLength = planeBytesUsed - windowStart;

				if (windowLength <= 0)
					return;

				var plane0 = vramSpan.Slice(0x00000 + windowStart, windowLength);
				var plane1 = vramSpan.Slice(0x10000 + windowStart, windowLength);

				plane0.FillEven((byte)' ');
				plane1.FillEven(Attributes);
			}
			else
			{
				int windowStart = fromCharacterLine * _stride + 2 * x1;
				int windowLength = 2 * width;

				for (int y = _clipRect.Y1; y <= _clipRect.Y2; y++)
				{
					if (windowStart + windowLength > planeBytesUsed)
						windowLength = planeBytesUsed - windowStart;

					if (windowLength <= 0)
						return;

					var plane0 = vramSpan.Slice(0x00000 + windowStart, windowLength);
					var plane1 = vramSpan.Slice(0x10000 + windowStart, windowLength);

					plane0.FillEven((byte)' ');
					plane1.FillEven(Attributes);
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

		int o = 2 * CursorAddress;

		int startAddress = StartAddress;
		int windowEndOffset = (CharacterLineWindowEnd + 1) * _stride;

		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(startAddress);

		var plane0 = vramSpan.Slice(0x00000, windowEndOffset);
		var plane1 = vramSpan.Slice(0x10000, windowEndOffset);

		int scrollOffset = 0;
		bool clearLastLineOnScroll = true;

		if (CharacterLineWindowStart < _clipRect.Y1)
		{
			int difference = _clipRect.Y1 - CharacterLineWindowStart;

			scrollOffset += difference * _stride;
		}

		if (CharacterLineWindowEnd > _clipRect.Y2)
		{
			int difference = CharacterLineWindowEnd - _clipRect.Y2;

			plane0 = plane0.Slice(0, plane0.Length - (difference - 1) * _stride);
			plane1 = plane1.Slice(0, plane1.Length - (difference - 1) * _stride);
			clearLastLineOnScroll = false;
		}

		var attributes = Attributes;

		using (HidePointerForOperationIfPointerAware())
		{
			void BeginNewLine()
			{
				CursorX = 0;

				if (CursorY + 1 <= CharacterLineWindowEnd)
					CursorY++;
				else
				{
					Span<byte> vramSpan = Array.VRAM;

					vramSpan = vramSpan.Slice(startAddress);

					var plane0 = vramSpan.Slice(0x00000, windowEndOffset);
					var plane1 = vramSpan.Slice(0x10000, windowEndOffset);

					ScrollTextUp(
						plane0.Slice(scrollOffset, plane0.Length - scrollOffset),
						plane1.Slice(scrollOffset, plane1.Length - scrollOffset),
						clearLastLineOnScroll);

					o -= _stride;
				}
			}

			Action updateOffset =
				() =>
				{
					o = CursorY * _stride + 2 * CursorX;
				};

			Action<Span<byte>, Span<byte>> writeSpace =
				(plane0, plane1) =>
				{
					plane0[o] = 32;
					if (EnableWriteAttributes)
						plane1[o] = attributes;
				};

			while (!buffer.IsEmpty)
			{
				if (ProcessControlCharacters)
				{
					if (ProcessControlCharacter(ref buffer, ref CursorX, ref CursorY, updateOffset, writeSpace, BeginNewLine, plane0, plane1))
						continue;
				}

				if (ResolvePassiveNewLine())
					updateOffset();

				int remainingChars = Width - CursorX;

				int spanLength = Math.Min(buffer.Length, remainingChars);

				int controlCharacterOffset = -1;

				if (ProcessControlCharacters)
				{
					controlCharacterOffset = buffer.Slice(0, spanLength).IndexOfAny(ControlCharacters);

					if (controlCharacterOffset >= 0)
						spanLength = controlCharacterOffset;
				}

				for (int i = 0; i < spanLength; i++)
				{
					if (_clipRect.Contains(CursorX, CursorY))
					{
						plane0[o] = buffer[i];
						if (EnableWriteAttributes)
							plane1[o] = attributes;
					}

					o += 2;
					CursorX++;
				}

				buffer = buffer.Slice(spanLength);

				if (CursorX == Width)
					PassiveNewLine();
			}

			if (CursorX < CharacterWidth)
				SetCursorPosition(CursorX, CursorY);
			else
				PassiveNewLine();
		}
	}

	public void WriteAttributes(int charCount)
	{
		int o = 2 * CursorAddress;

		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(StartAddress);

		var plane1 = vramSpan.Slice(0x10000, (CharacterLineWindowEnd + 1) * _stride);

		int scrollOffset = 0;
		bool clearLastLineOnScroll = true;

		if (CharacterLineWindowStart < _clipRect.Y1)
		{
			int difference = _clipRect.Y1 - CharacterLineWindowStart;

			scrollOffset += difference * _stride;
		}

		if (CharacterLineWindowEnd > _clipRect.Y2)
		{
			int difference = CharacterLineWindowEnd - _clipRect.Y2;

			plane1 = plane1.Slice(0, plane1.Length - (difference - 1) * _stride);
			clearLastLineOnScroll = false;
		}

		var attributes = Attributes;

		using (HidePointerForOperationIfPointerAware())
		{
			while (charCount > 0)
			{
				int remainingChars = Width - CursorX;

				int spanLength = Math.Min(charCount, remainingChars);

				for (int i = 0; i < spanLength; i++)
				{
					if (_clipRect.Contains(CursorX, CursorY))
						plane1[o] = attributes;

					o += 2;
					CursorX++;
				}

				charCount -= spanLength;

				if (charCount > 0)
				{
					CursorX = 0;

					if (CursorY + 1 <= CharacterLineWindowEnd)
						CursorY++;
					else
					{
						ScrollTextUp(Span<byte>.Empty, plane1.Slice(scrollOffset, plane1.Length - scrollOffset), clearLastLineOnScroll);
						o -= _stride;
					}
				}
			}

			MoveCursor(CursorX, CursorY);
		}
	}

	public override byte GetCharacter(int x, int y)
	{
		if ((x < 0) || (x >= CharacterWidth)
		 || (y < 0) || (y >= CharacterHeight))
			return 0;

		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(StartAddress);

		var plane0 = vramSpan.Slice(0, _stride * CharacterHeight);

		int offset = y * _stride + 2 * x;

		return plane0[offset];
	}

	public override byte GetAttribute(int x, int y)
	{
		if ((x < 0) || (x >= CharacterWidth)
		 || (y < 0) || (y >= CharacterHeight))
			return 0;

		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(StartAddress);

		var plane1 = vramSpan.Slice(0x10000, (CharacterLineWindowEnd + 1) * _stride);

		int offset = y * _stride + 2 * x;

		return plane1[offset];
	}

	public override void ScrollTextUp()
	{
		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(StartAddress);

		var plane0 = vramSpan.Slice(0x00000, 0x10000);
		var plane1 = vramSpan.Slice(0x10000, 0x10000);

		int characterLineWindowLines = CharacterLineWindowEnd - CharacterLineWindowStart + 1;

		int windowOffset = CharacterLineWindowStart * _stride;
		int windowLength = characterLineWindowLines * _stride;

		bool clearLastLine = true;

		if (CharacterLineWindowStart < _clipRect.Y1)
		{
			int difference = _clipRect.Y1 - CharacterLineWindowStart;

			windowOffset += difference * _stride;
			windowLength -= difference * _stride;
		}

		if (CharacterLineWindowEnd > _clipRect.Y2)
		{
			int difference = _clipRect.Y2 - CharacterLineWindowEnd - 1;

			if (difference > 0)
			{
				windowLength -= difference * _stride;
				clearLastLine = false;
			}
		}

		if (windowOffset + windowLength > plane0.Length)
			windowLength = plane0.Length - windowOffset;

		if (windowLength > 0)
		{
			plane0 = plane0.Slice(windowOffset, windowLength);
			plane1 = plane1.Slice(windowOffset, windowLength);

			ScrollTextUp(plane0, plane1, clearLastLine);
		}
	}

	void ScrollTextUp(Span<byte> plane0, Span<byte> plane1, bool clearLastLine)
	{
		using (HidePointerForOperationIfPointerAware())
		{
			int x1 = Math.Max(0, _clipRect.X1);
			int x2 = Math.Min(CharacterWidth - 1, _clipRect.X2);

			int width = x2 - x1 + 1;

			int x1_2 = 2 * x1;
			int width_2 = 2 * width;

			if (width == Width)
			{
				if (plane0.Length > _stride)
				{
					plane0.Slice(_stride).CopyEvenTo(plane0);
					if (clearLastLine)
						plane0.Slice(plane0.Length - _stride).FillEven((byte)' ');
				}
				else if (clearLastLine)
					plane0.FillEven((byte)' ');

				if (plane1.Length > _stride)
				{
					plane1.Slice(_stride).CopyEvenTo(plane1);
					if (clearLastLine)
						plane1.Slice(plane1.Length - _stride).FillEven(Attributes);
				}
				else if (clearLastLine)
					plane1.FillEven(Attributes);
			}
			else
			{
				while (plane0.Length > _stride)
				{
					plane0.Slice(x1 + _stride, width_2).CopyEvenTo(plane0.Slice(x1_2));
					plane0 = plane0.Slice(_stride);
				}

				if (clearLastLine)
					plane0.Slice(x1_2, width_2).FillEven((byte)' ');

				while (plane1.Length > _stride)
				{
					plane1.Slice(x1_2 + _stride, width_2).CopyEvenTo(plane1.Slice(x1_2));
					plane1 = plane1.Slice(_stride);
				}

				if (clearLastLine)
					plane1.Slice(x1_2, width_2).FillEven(Attributes);
			}
		}
	}

	public override void ScrollTextDown()
	{
		Span<byte> vramSpan = Array.VRAM;

		vramSpan = vramSpan.Slice(StartAddress);

		var plane0 = vramSpan.Slice(0x00000, 0x10000);
		var plane1 = vramSpan.Slice(0x10000, 0x10000);

		int characterLineWindowLines = CharacterLineWindowEnd - CharacterLineWindowStart + 1;

		int windowOffset = CharacterLineWindowStart * _stride;
		int windowLength = characterLineWindowLines * _stride;

		bool clearLastLine = true;

		if (CharacterLineWindowStart < _clipRect.Y1)
		{
			int difference = _clipRect.Y1 - CharacterLineWindowStart;

			windowOffset += difference * _stride;
			windowLength -= difference * _stride;
		}

		if (CharacterLineWindowEnd > _clipRect.Y2)
		{
			int difference = _clipRect.Y2 - CharacterLineWindowEnd - 1;

			if (difference > 0)
			{
				windowLength -= difference * _stride;
				clearLastLine = false;
			}
		}

		if (windowOffset + windowLength > plane0.Length)
			windowLength = plane0.Length - windowOffset;

		if (windowLength > 0)
		{
			plane0 = plane0.Slice(windowOffset, windowLength);
			plane1 = plane1.Slice(windowOffset, windowLength);

			ScrollTextDown(plane0, plane1, clearLastLine);
		}
	}

	void ScrollTextDown(Span<byte> plane0, Span<byte> plane1, bool clearFirstLine)
	{
		using (HidePointerForOperationIfPointerAware())
		{
			int x1 = Math.Max(0, _clipRect.X1);
			int x2 = Math.Min(CharacterWidth - 1, _clipRect.X2);

			int width = x2 - x1 + 1;

			int x1_2 = 2 * x1;
			int width_2 = 2 * width;

			if (width == Width)
			{
				if (plane0.Length > _stride)
				{
					plane0.Slice(0, plane0.Length - _stride).CopyEvenTo(plane0.Slice(_stride));
					if (clearFirstLine)
						plane0.Slice(0, _stride).FillEven((byte)' ');
				}
				else if (clearFirstLine)
					plane0.FillEven((byte)' ');

				if (plane1.Length > _stride)
				{
					plane1.Slice(0, plane1.Length - _stride).CopyEvenTo(plane1.Slice(_stride));
					if (clearFirstLine)
						plane1.Slice(0, _stride).FillEven((byte)' ');
				}
				else if (clearFirstLine)
					plane1.FillEven((byte)' ');
			}
			else
			{
				int lineOffset = plane0.Length - (plane0.Length % _stride);

				while (plane0.Length > _stride)
				{
					lineOffset -= _stride;

					plane0.Slice(x1_2 + lineOffset, width_2).CopyEvenTo(plane0.Slice(x1_2 + lineOffset + _stride));
				}

				if (clearFirstLine)
					plane0.FillEven((byte)' ');

				lineOffset = plane1.Length - (plane1.Length % _stride);

				while (plane1.Length > _stride)
				{
					lineOffset -= _stride;

					plane1.Slice(x1_2 + lineOffset, width_2).CopyEvenTo(plane1.Slice(x1_2 + lineOffset + _stride));
				}

				if (clearFirstLine)
					plane1.Slice(x1_2, width_2).FillEven(Attributes);
			}
		}
	}

	public override void ScrollTextWindow(int x1, int y1, int x2, int y2, int numLines, byte fillAttribute)
	{
		// numLines < 0 => scroll up
		// numLines > 0 => scroll down

		if (numLines == 0)
			return;

		if (x1 > x2)
			(x1, x2) = (x2, x1);
		if (y1 > y2)
			(y1, y2) = (y2, y1);

		var scrollRect = new IntegerRect(x1, y1, x2, y2);

		if (!scrollRect.Intersects(_clipRect))
			return;

		(scrollRect.X1, scrollRect.Y1) = _clipRect.Constrain(x1, y1);
		(scrollRect.X2, scrollRect.Y2) = _clipRect.Constrain(x2, y2);

		if ((scrollRect.Y2 < CharacterLineWindowStart) || (scrollRect.Y1 > CharacterLineWindowEnd))
			return;

		if (scrollRect.Y1 < CharacterLineWindowStart)
			scrollRect.Y1 = CharacterLineWindowStart;
		if (scrollRect.Y2 > CharacterLineWindowEnd)
			scrollRect.Y2 = CharacterLineWindowEnd;

		using (HidePointerForOperationIfPointerAware())
		{
			int constrainedWidth = scrollRect.X2 - scrollRect.X1 + 1;
			int constrainedHeight = scrollRect.Y2 - scrollRect.Y1 + 1;

			int totalLines = y2 - y1 + 1;

			int fillLines = Math.Min(totalLines, Math.Abs(numLines));
			int keepLines = totalLines - fillLines;

			int firstBlitLineY = y1 + Math.Max(numLines, 0);
			int lastBlitLineY = firstBlitLineY + keepLines - 1;

			Span<byte> vramSpan = Array.VRAM;

			vramSpan = vramSpan.Slice(StartAddress);

			var plane0 = vramSpan.Slice(0x00000, 0x10000);
			var plane1 = vramSpan.Slice(0x10000, 0x10000);

			plane0 = plane0.Slice(scrollRect.Y1 * _stride);
			plane1 = plane1.Slice(scrollRect.Y1 * _stride);

			if (constrainedWidth == Width)
			{
				int actualFirstBlitLineY = Math.Max(scrollRect.Y1, firstBlitLineY);
				int actualLastBlitLineY = Math.Min(scrollRect.Y2, lastBlitLineY);

				int actualBlitLines = actualLastBlitLineY - actualFirstBlitLineY + 1;

				int offset = fillLines * _stride;
				int length = actualBlitLines * _stride;

				int fromOffset = (numLines < 0) ? offset : 0;
				int toOffset = (numLines > 0) ? offset : 0;

				if (fromOffset > 0)
				{
					plane0.Slice(fromOffset, length).CopyEvenTo(plane0);
					plane1.Slice(fromOffset, length).CopyEvenTo(plane1);
				}
				else
				{
					int constrainedWidth_2 = 2 * constrainedWidth;

					for (int y = actualLastBlitLineY; y >= actualFirstBlitLineY; y--)
					{
						int o = y * _stride;

						plane0.Slice(o, constrainedWidth_2).CopyEvenTo(plane0.Slice(o + toOffset));
						plane1.Slice(o, constrainedWidth_2).CopyEvenTo(plane1.Slice(o + toOffset));
					}
				}

				if (actualFirstBlitLineY > scrollRect.Y1)
				{
					int actualFillLines = actualFirstBlitLineY - scrollRect.Y1;

					length = actualFillLines * _stride;

					plane0.Slice(scrollRect.Y1 * _stride, length).FillEven((byte)' ');
					plane1.Slice(scrollRect.Y1 * _stride, length).FillEven(fillAttribute);
				}

				if (actualLastBlitLineY > scrollRect.Y2)
				{
					int actualFillLines = scrollRect.Y2 - actualLastBlitLineY;

					length = actualFillLines * _stride;

					plane0.Slice((actualLastBlitLineY + 1) * _stride, length).FillEven((byte)' ');
					plane1.Slice((actualLastBlitLineY + 1) * _stride, length).FillEven(fillAttribute);
				}
			}
			else
			{
				int loopY1 = numLines > 0 ? scrollRect.Y2 : scrollRect.Y1;
				int loopY2 = numLines > 0 ? scrollRect.Y1 : scrollRect.Y2;
				int loopDY = numLines > 0 ? -1 : +1;
				int loopDO = loopDY * _stride;

				int blitOffset = -numLines * _stride;

				int loopOStart = loopY1 * _stride + 2 * scrollRect.X1;
				int loopYEnd = loopY2 + loopDY;

				int constrainedWidth_2 = 2 * constrainedWidth;

				for (int y = loopY1, o = loopOStart; y != loopYEnd; y += loopDY, o += loopDO)
				{
					if ((y >= firstBlitLineY) && (y <= lastBlitLineY))
					{
						plane0.Slice(o + blitOffset, constrainedWidth_2).CopyEvenTo(plane0.Slice(o));
						plane1.Slice(o + blitOffset, constrainedWidth_2).CopyEvenTo(plane1.Slice(o));
					}
					else
					{
						plane0.Slice(o, constrainedWidth_2).FillEven((byte)' ');
						plane1.Slice(o, constrainedWidth_2).FillEven(fillAttribute);
					}
				}
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
			int pointerCharacterX = Machine.MouseDriver.ScaledPointerX;
			int pointerCharacterY = Machine.MouseDriver.ScaledPointerY;

			int pointerOffset = pointerCharacterY * _stride + 2 * pointerCharacterX;

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
