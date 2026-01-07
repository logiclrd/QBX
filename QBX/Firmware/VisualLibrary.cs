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

	public int CursorX = 0;
	public int CursorY = 0;

	public abstract void RefreshParameters();

	public void Clear()
	{
		ClearImplementation();
		MoveCursor(0, 0);
	}

	protected abstract void ClearImplementation();

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

	protected virtual void MoveCursorHandlePhysicalCursor()
	{
		// Overridden for text modes
	}

	public void MoveCursor(int x, int y)
	{
		CursorX = x;
		CursorY = y;

		MoveCursorHandlePhysicalCursor();
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

	public void AdvanceCursor()
	{
		CursorX++;

		if (CursorX == CharacterWidth)
		{
			CursorX = 0;

			if (CursorY + 1 == CharacterHeight)
				ScrollText();
			else
				CursorY++;
		}

		MoveCursorHandlePhysicalCursor();
	}

	public abstract void ScrollText();
}
