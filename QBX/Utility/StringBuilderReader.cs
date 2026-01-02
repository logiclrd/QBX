using System.Text;

namespace QBX.Utility;

public class StringBuilderReader : TextReader
{
	StringBuilder _source;
	StringBuilder.ChunkEnumerator _chunks;
	int _currentChunkOffset = 0;
	int _currentOverallOffset = 0;
	bool _isEndOfString;
	bool _isOpen;

	public StringBuilderReader(StringBuilder source)
	{
		_source = source;
		_chunks = source.GetChunks();
		_isOpen = true;

		AdvanceChunk();
	}

	void AdvanceChunk()
	{
		while (!_isEndOfString)
		{
			if (_chunks.MoveNext())
			{
				if (_chunks.Current.Length > 0)
					break;
			}
			else
				_isEndOfString = true;
		}
	}

	public override void Close()
	{
		_isOpen = false;
	}

	void EnsureIsOpen()
	{
		if (!_isOpen)
			throw new InvalidOperationException("StringBuilderReader is closed.");
	}

	public override int Peek()
	{
		EnsureIsOpen();

		if (_isEndOfString)
			return -1;

		return _chunks.Current.Span[_currentChunkOffset];
	}

	public override int Read()
	{
		EnsureIsOpen();

		if (_isEndOfString)
			return -1;

		char ch = _chunks.Current.Span[_currentChunkOffset];

		_currentChunkOffset++;
		_currentOverallOffset++;

		if (_currentChunkOffset >= _chunks.Current.Length)
		{
			_currentChunkOffset = 0;
			AdvanceChunk();
		}

		return ch;
	}

	public override int Read(char[]? buffer, int index, int count)
	{
		ArgumentNullException.ThrowIfNull(buffer);

		ArgumentOutOfRangeException.ThrowIfNegative(index);
		ArgumentOutOfRangeException.ThrowIfNegative(count);

		if (buffer.Length - index < count)
			throw new ArgumentException("Invalid offset and length");

		return Read(buffer.AsSpan().Slice(index, count));
	}

	public override int Read(Span<char> buffer)
	{
		int copied = 0;

		while (buffer.Length > 0)
		{
			if (_isEndOfString)
				return copied;

			var currentChunk = _chunks.Current;
			int remaining = currentChunk.Length - _currentChunkOffset;

			if (remaining > buffer.Length)
			{
				currentChunk.Span.Slice(_currentChunkOffset, buffer.Length).CopyTo(buffer);
				_currentChunkOffset += buffer.Length;
				_currentOverallOffset += buffer.Length;
				buffer = buffer.Slice(buffer.Length);
				copied += buffer.Length;
			}
			else
			{
				currentChunk.Span.Slice(_currentChunkOffset, remaining).CopyTo(buffer);
				_currentOverallOffset += remaining;
				buffer = buffer.Slice(remaining);
				copied += remaining;

				_currentChunkOffset = 0;
				AdvanceChunk();
			}
		}

		return copied;
	}

	public override string ReadToEnd()
	{
		_isEndOfString = true;
		return _source.ToString(_currentOverallOffset, _source.Length - _currentOverallOffset);
	}

	public override int ReadBlock(char[] buffer, int index, int count)
	{
		int remaining = _source.Length - _currentOverallOffset;

		if (remaining < count)
			throw new InvalidOperationException("Cannot do a blocking read for more characters than exist in the StringBuilder");

		return Read(buffer, index, count);
	}

	public override int ReadBlock(Span<char> buffer)
	{
		int remaining = _source.Length - _currentOverallOffset;

		if (remaining < buffer.Length)
			throw new InvalidOperationException("Cannot do a blocking read for more characters than exist in the StringBuilder");

		return Read(buffer);
	}
}
