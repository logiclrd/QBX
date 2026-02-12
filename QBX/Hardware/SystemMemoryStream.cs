using System;
using System.IO;

namespace QBX.Hardware;

public class SystemMemoryStream(IMemory memory, int memoryOffset, int length) : Stream
{
	public override bool CanRead => true;
	public override bool CanSeek => true;
	public override bool CanWrite => true;

	public override long Length => length;

	int _position = 0;

	public override long Position { get => _position; set => Seek(_position, SeekOrigin.Begin); }

	public override void Flush() { }

	public override int Read(byte[] buffer, int offset, int count)
	{
		for (int i = 0; i < count; i++)
		{
			if (_position >= length)
				return i;

			buffer[offset + i] = memory[memoryOffset + _position];
			_position++;
		}

		return count;
	}

	public override long Seek(long offset, SeekOrigin origin)
	{
		int newPosition;

		switch (origin)
		{
			case SeekOrigin.Begin:
				newPosition = (int)offset;
				break;
			case SeekOrigin.Current:
				newPosition = (int)(_position + offset);
				break;
			case SeekOrigin.End:
				newPosition = (int)(length + offset);
				break;

			default:
				throw new ArgumentException(nameof(origin));
		}

		if ((newPosition < 0) || (newPosition >= length))
		throw new ArgumentOutOfRangeException(nameof(offset));

		_position = newPosition;

		return _position;
	}

	public override void SetLength(long value) => throw new NotSupportedException();

	public override void Write(byte[] buffer, int offset, int count)
	{
		if (_position + count > length)
			throw new ArgumentOutOfRangeException(nameof(offset));

		for (int i=0; i < count; i++)
			memory[memoryOffset + (_position++)] = buffer[offset + i];
	}
}
