using System;
using System.IO;
using System.Text;

namespace QBX.Utility;

public class CharacterArrayWriter : TextWriter
{
	char[] _buffer = new char[32];
	int _length;

	public override Encoding Encoding => Encoding.Unicode;

	public void Clear()
	{
		_length = 0;
	}

	public Span<char> GetBuffer()
		=> _buffer.AsSpan().Slice(0, _length);

	void ExpandBuffer(int count = 1)
	{
		int bufferLength = _buffer.Length;
		int minimumBufferLength = _length + count;

		while (bufferLength < minimumBufferLength)
			bufferLength = bufferLength + (bufferLength >> 1);

		if (bufferLength > _buffer.Length)
		{
			var newBuffer = new char[bufferLength];

			_buffer.CopyTo(newBuffer);
			_buffer = newBuffer;
		}
	}

	public override void Write(char value)
	{
		if (_length == _buffer.Length)
			ExpandBuffer();

		_buffer[_length++] = value;
	}

	public override void Write(char[] buffer, int index, int count)
	{
		ExpandBuffer(count);

		buffer.AsSpan().Slice(index, count).CopyTo(_buffer.AsSpan().Slice(_length));
		_length += count;
	}

	public override void Write(ReadOnlySpan<char> buffer)
	{
		ExpandBuffer(buffer.Length);

		buffer.CopyTo(_buffer.AsSpan().Slice(_length));
		_length += buffer.Length;
	}
}
