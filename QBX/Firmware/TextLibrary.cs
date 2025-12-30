using QBX.Fonts;
using QBX.Hardware;

using System.Text;

namespace QBX.Firmware;

public class TextLibrary
{
	GraphicsArray _array;

	public GraphicsArray Array => _array;

	public TextLibrary(GraphicsArray array)
	{
		_array = array;

		RefreshParameters();
	}


	public int Width;
	public int Height;

	public int CursorX = 0;
	public int CursorY = 0;

	public int CursorAddress => CursorY * Width + CursorX;

	public byte Attributes = 7;

	public void RefreshParameters()
	{
		Width = _array.CRTController.Registers.EndHorizontalDisplay + 1;
		Height = _array.CRTController.NumScanLines / _array.CRTController.CharacterHeight;

		int cursorAddress = _array.CRTController.CursorAddress;

		CursorY = cursorAddress / Width;
		CursorX = cursorAddress % Width;
	}

	public void SetAttributes(int foreground, int background)
	{
		Attributes = unchecked((byte)((foreground & 15) | ((background & 15) << 4)));
	}

	public void MoveCursor(int x, int y)
	{
		CursorX = x;
		CursorY = y;

		_array.CRTController.CursorAddress = CursorY * Width + CursorX;
	}

	public void WriteAt(int x, int y, string text)
	{
		MoveCursor(x, y);
		Write(text);
	}

	Encoding _cp437 = new CP437Encoding();

	[ThreadStatic]
	static byte[]? s_buffer;

	static byte[] EnsureBuffer(int count)
	{
		if ((s_buffer == null) || (s_buffer.Length < count))
			s_buffer = new byte[count * 2];

		return s_buffer;
	}

	public void Write(string text)
	{
		var buffer = EnsureBuffer(text.Length);

		int byteCount = _cp437.GetBytes(text, 0, text.Length, buffer, 0);

		Write(buffer, 0, byteCount);
	}

	public void Write(byte[] buffer)
	{
		Write(buffer, 0, buffer.Length);
	}

	public void Write(char ch)
	{
		int byteCount = _cp437.GetMaxByteCount(1);

		var buffer = EnsureBuffer(byteCount);

		Span<char> chars = stackalloc char[] { ch };

		byteCount = _cp437.GetBytes(chars, buffer);

		Write(buffer, 0, byteCount);
	}

	public void Write(byte b)
	{
		var buffer = EnsureBuffer(1);

		buffer[0] = b;

		Write(buffer, 0, 1);
	}

	public void Write(byte[] buffer, int offset, int count)
	{
		int o = CursorAddress;

		Span<byte> vramSpan = _array.VRAM;

		var plane0 = vramSpan.Slice(0x00000, 0x10000);
		var plane1 = vramSpan.Slice(0x10000, 0x10000);

		int cursorX = CursorX;
		int cursorY = CursorY;

		var attributes = Attributes;

		while (count > 0)
		{
			int remainingChars = Width - cursorX;

			int spanLength = Math.Min(count, remainingChars);

			for (int i = 0; i < spanLength; i++)
			{
				plane0[o] = buffer[offset];
				plane1[o] = attributes;

				o++;
				offset++;
				cursorX++;
			}

			count -= spanLength;

			if (count > 0)
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
}
