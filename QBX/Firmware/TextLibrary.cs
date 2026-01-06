using System;
using System.Buffers;
using System.Text;

using QBX.Fonts;
using QBX.Hardware;

namespace QBX.Firmware;

public class TextLibrary : VisualLibrary
{
	public TextLibrary(GraphicsArray array)
		: base(array)
	{
	}

	public int CursorAddress => CursorY * Width + CursorX;

	public bool MovePhysicalCursor = true;

	public byte Attributes = 7;

	public override void RefreshParameters()
	{
		if (Array.CRTController.CharacterHeight == 0)
			return;

		Width = Array.CRTController.Registers.EndHorizontalDisplay + 1;
		Height = Array.CRTController.NumScanLines / Array.CRTController.CharacterHeight;

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
		Attributes = unchecked((byte)((foreground & 15) | ((Attributes & 15) << 4)));
	}

	public void SetBackgroundAttribute(int background)
	{
		Attributes = unchecked((byte)((Attributes & 15) | ((background & 15) << 4)));
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

	public void MoveCursor(int x, int y)
	{
		CursorX = x;
		CursorY = y;

		if (MovePhysicalCursor)
			UpdatePhysicalCursor();
	}

	public void UpdatePhysicalCursor()
	{
		Array.CRTController.CursorAddress = CursorY * Width + CursorX;
	}

	public void WriteAt(int x, int y, string text)
	{
		MoveCursor(x, y);
		Write(text);
	}

	public void WriteAttributesAt(int x, int y, int charCount)
	{
		MoveCursor(x, y);
		WriteAttributes(charCount);
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

		Write(span);
	}

	public void Write(string text)
	{
		var buffer = PrepareBuffer();

		_cp437.GetBytes(text.AsSpan(), buffer);

		Write(buffer.WrittenSpan);
	}

	public void Write(ReadOnlyMemory<char> text)
	{
		var buffer = PrepareBuffer();

		_cp437.GetBytes(text.Span, buffer);

		Write(buffer.WrittenSpan);
	}

	public void Write(ReadOnlySpan<char> text)
	{
		var buffer = PrepareBuffer();

		_cp437.GetBytes(text, buffer);

		Write(buffer.WrittenSpan);
	}

	public void Write(string text, int offset, int count)
	{
		var buffer = PrepareBuffer();

		_cp437.GetBytes(text.AsSpan().Slice(offset, count), buffer);

		Write(buffer.WrittenSpan);
	}

	public void Write(StringBuilder builder, int offset, int count)
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

				Write(thisChunk);

				offset = 0;
				count -= thisChunk.Length;

				if (count == 0)
					break;
			}
		}
	}

	public void Write(char ch)
	{
		int byteCount = _cp437.GetMaxByteCount(1);

		var buffer = PrepareBuffer();

		Span<char> chars = stackalloc char[] { ch };

		_cp437.GetBytes(chars, buffer);

		Write(buffer.WrittenSpan);
	}

	public void Write(byte b)
	{
		var buffer = PrepareBuffer();

		var span = buffer.GetSpan(1);

		span[0] = b;

		Write(span.Slice(0, 1));
	}

	public void Write(byte[] buffer)
	{
		Write(buffer, 0, buffer.Length);
	}

	public void Write(byte[] buffer, int offset, int count)
	{
		Write(buffer.AsSpan().Slice(offset, count));
	}

	public void Write(ReadOnlySpan<byte> buffer)
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
					plane0.Slice(Width).CopyTo(plane0);
					plane1.Slice(Width).CopyTo(plane1);

					o -= Width;
				}
			}
		}

		MoveCursor(cursorX, cursorY);
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
