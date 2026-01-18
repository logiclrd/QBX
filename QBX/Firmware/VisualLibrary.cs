using System;
using System.Buffers;
using System.Text;

using QBX.Firmware.Fonts;
using QBX.Hardware;

namespace QBX.Firmware;

public abstract class VisualLibrary
{
	public readonly Machine Machine;
	public readonly GraphicsArray Array;

	public VisualLibrary(Machine machine)
	{
		Machine = machine;

		Array = machine.GraphicsArray;

		RefreshParameters();
	}

	public int Width;
	public int Height;

	public int CharacterWidth;
	public int CharacterHeight;

	public int StartAddress;

	public int CharacterLineWindowStart;
	public int CharacterLineWindowEnd;

	public int CursorX = 0;
	public int CursorY = 0;

	public virtual void RefreshParameters()
	{
		CharacterLineWindowStart = 0;
		CharacterLineWindowEnd = CharacterHeight - 1;
	}

	public void Clear()
	{
		ClearImplementation();
		MoveCursor(0, 0);
	}

	public void ClearCharacterLineWindow()
	{
		ClearImplementation(CharacterLineWindowStart, CharacterLineWindowEnd);
		MoveCursor(0, CharacterLineWindowStart);
	}

	protected abstract void ClearImplementation(int fromCharacterLine = 0, int toCharacterLine = -1);

	public bool SetActivePage(int pageNumber)
	{
		int pageSize = Video.ComputePageSize(Array);
		int pageCount = 16384 / pageSize;

		if ((pageNumber >= 0) && (pageNumber < pageCount))
		{
			StartAddress = pageNumber * pageSize;
			RefreshParameters();

			return true;
		}

		return false;
	}

	public void ResetCharacterLineWindow()
	{
		UpdateCharacterLineWindow(0, CharacterHeight - 1);
	}

	public void UpdateCharacterLineWindow(int windowStart, int windowEnd)
	{
		if (windowStart > windowEnd)
			(windowStart, windowEnd) = (windowEnd, windowStart);

		CharacterLineWindowStart = int.Clamp(windowStart, 0, Height - 1);
		CharacterLineWindowEnd = int.Clamp(windowEnd, 0, Height - 1);

		// Clamp CursorY
		MoveCursor(CursorX, CursorY);
	}

	protected virtual void MoveCursorHandlePhysicalCursor()
	{
		// Overridden for text modes
	}

	public void MoveCursor(int x, int y)
	{
		x = Math.Clamp(x, 0, CharacterWidth - 1);
		y = Math.Clamp(y, CharacterLineWindowStart, CharacterLineWindowEnd);

		CursorX = x;
		CursorY = y;

		_delayedNewLine = false;

		MoveCursorHandlePhysicalCursor();
	}

	public void WriteTextAt(int x, int y, byte ch)
	{
		MoveCursor(x, y);
		WriteText(ch);
	}

	public void WriteTextAt(int x, int y, char ch)
	{
		MoveCursor(x, y);
		WriteText(ch);
	}

	public void WriteTextAt(int x, int y, string text)
	{
		MoveCursor(x, y);
		WriteText(text);
	}

	Encoding _cp437 = new CP437Encoding();

	[ThreadStatic]
	static ArrayBufferWriter<byte>? s_buffer;

	static ArrayBufferWriter<byte> PrepareBuffer()
	{
		if (s_buffer == null)
			s_buffer = new ArrayBufferWriter<byte>();

		s_buffer.ResetWrittenCount();

		return s_buffer;
	}

	public void WriteNumber(int value, int numDigits)
	{
		// I exist to avoid string.Format allocations during frame render.

		var buffer = PrepareBuffer();

		var span = buffer.GetSpan(numDigits).Slice(0, numDigits);

		for (int i = 0, o = numDigits - 1; i < numDigits; i++, o--)
		{
			span[o] = unchecked((byte)('0' + (value % 10)));
			value /= 10;
		}

		WriteText(span);
	}

	public void WriteText(string text)
	{
		var buffer = PrepareBuffer();

		_cp437.GetBytes(text.AsSpan(), buffer);

		WriteText(buffer.WrittenSpan);
	}

	public void WriteText(ReadOnlyMemory<char> text)
	{
		var buffer = PrepareBuffer();

		_cp437.GetBytes(text.Span, buffer);

		WriteText(buffer.WrittenSpan);
	}

	public void WriteText(ReadOnlySpan<char> text)
	{
		var buffer = PrepareBuffer();

		_cp437.GetBytes(text, buffer);

		WriteText(buffer.WrittenSpan);
	}

	public void WriteText(string text, int offset, int count)
	{
		var buffer = PrepareBuffer();

		_cp437.GetBytes(text.AsSpan().Slice(offset, count), buffer);

		WriteText(buffer.WrittenSpan);
	}

	public void WriteText(StringBuilder builder, int offset, int count)
	{
		foreach (var chunk in builder.GetChunks())
		{
			if (offset >= chunk.Length)
				offset -= chunk.Length;
			else
			{
				int thisChunkLength = chunk.Length - offset;

				if (thisChunkLength > count)
					thisChunkLength = count;

				var thisChunk = chunk.Slice(offset, thisChunkLength);

				WriteText(thisChunk);

				offset = 0;
				count -= thisChunk.Length;

				if (count == 0)
					break;
			}
		}
	}

	public void WriteText(char ch)
	{
		int byteCount = _cp437.GetMaxByteCount(1);

		var buffer = PrepareBuffer();

		Span<char> chars = stackalloc char[] { ch };

		_cp437.GetBytes(chars, buffer);

		WriteText(buffer.WrittenSpan);
	}

	public void WriteText(byte b)
	{
		var buffer = PrepareBuffer();

		var span = buffer.GetSpan(1);

		span[0] = b;

		WriteText(span.Slice(0, 1));
	}

	public void WriteText(byte[] buffer)
	{
		WriteText(buffer, 0, buffer.Length);
	}

	public void WriteText(byte[] buffer, int offset, int count)
	{
		WriteText(buffer.AsSpan().Slice(offset, count));
	}

	public abstract void WriteText(ReadOnlySpan<byte> buffer);

	public string ReadLine(Keyboard input)
	{
		var graphics = this as GraphicsLibrary;

		bool cursorVisible = false;

		void DrawCursor(int x, int y, int on)
		{
			if (graphics != null)
			{
				int x1 = x * Array.Sequencer.CharacterWidth;
				int y1 = y * graphics.CharacterScans;

				int x2 = x1 + Array.Sequencer.CharacterWidth - 1;
				int y2 = y1 + graphics.CharacterScans - 1;

				graphics.FillBox(x1, y1, x2, y2, on * graphics.DrawingAttribute);
			}
		}

		void ShowCursor(int x, int y)
		{
			HideCursor();

			MoveCursor(x, y);

			DrawCursor(CursorX, CursorY, 1);
			cursorVisible = true;

			// TODO: support editing the middle of the string
			// => cursor is drawn by inverting pixels of characters for some scans
			// => insert mode off: all scans
			// => insert mode on: bottom half
		}

		void HideCursor()
		{
			if (cursorVisible)
			{
				DrawCursor(CursorX, CursorY, 0);
				cursorVisible = false;
			}
		}

		var buffer = new StringBuilder();

		int x = CursorX;
		int y = CursorY;

		// TODO: arrow keys, home/end
		while (true)
		{
			ShowCursor(x, y);

			input.WaitForInput();

			var evt = input.GetNextEvent();

			HideCursor();

			if ((evt == null) || evt.IsRelease)
				continue;

			if (evt.ScanCode == ScanCode.Backspace)
			{
				if (buffer.Length > 0)
				{
					buffer.Length--;
					x--;
					if (x < 0)
					{
						x += CharacterWidth;
						y--;

						if (y < 0)
							y = 0;
					}

					WriteTextAt(x, y, ' ');
				}
			}
			else if (evt.ScanCode == ScanCode.Return)
			{
				NewLine();
				break;
			}
			else if (evt.TextCharacter != '\0')
			{
				buffer.Append(evt.TextCharacter);

				WriteText(evt.TextCharacter);

				x = CursorX;
				y = CursorY;
			}
		}

		return buffer.ToString();
	}

	public void AdvanceCursor()
	{
		CursorX++;

		if (CursorX == CharacterWidth)
			PassiveNewLine();

		MoveCursorHandlePhysicalCursor();
	}

	public void NewLine()
	{
		_delayedNewLine = false;

		int newX = CursorX;
		int newY = CursorY;

		newX = 0;

		if (newY == CharacterLineWindowEnd)
			ScrollText();
		else
			newY++;

		MoveCursor(newX, newY);
	}

	bool _delayedNewLine = false;

	public void PassiveNewLine()
	{
		if ((CursorX + 1 >= CharacterWidth)
		 && (CursorY >= CharacterLineWindowEnd))
		{
			CursorX = CharacterWidth - 1;
			_delayedNewLine = true;
		}
		else
			NewLine();
	}

	public void ResolvePassiveNewLine()
	{
		if (_delayedNewLine)
			NewLine();
	}

	public abstract void ScrollText();
}
