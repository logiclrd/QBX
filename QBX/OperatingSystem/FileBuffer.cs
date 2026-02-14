using System;

namespace QBX.OperatingSystem;

public struct FileBuffer(int size)
{
	public int NextFree;
	public int NumUsed;

	public int Available => size - NumUsed;
	public bool IsEmpty => NumUsed == 0;
	public bool IsFull => NumUsed == size;

	public int ContiguousAvailable => Math.Min(Available, size - NextFree);

	byte[] _data = new byte[size];

	public int Size => size;

	public byte this[int index]
	{
		get
		{
			if ((index < 0) || (index >= NumUsed))
				throw new IndexOutOfRangeException();

			return _data[(size + NextFree - NumUsed + index) % size];
		}
		set
		{
			if ((index < 0) || (index >= NumUsed))
				throw new IndexOutOfRangeException();

			_data[(size + NextFree - NumUsed + index) % size] = value;
		}
	}

	public void Push(byte data)
	{
		if (Available == 0)
			throw new InvalidOperationException("Insufficient buffer space");

		_data[NextFree] = data;
		Use(1);
	}

	public void Push(ReadOnlySpan<byte> data)
	{
		if (data.Length > Available)
			throw new InvalidOperationException("Insufficient buffer space");

		int contiguousAvailable = size - NextFree;

		if (contiguousAvailable < data.Length)
		{
			data.Slice(0, contiguousAvailable).CopyTo(_data.AsSpan().Slice(NextFree));
			Use(contiguousAvailable);
			data = data.Slice(contiguousAvailable);
		}

		data.CopyTo(_data.AsSpan().Slice(NextFree));
		Use(data.Length);
	}

	public byte Pull()
	{
		if (!TryPull(out var b))
			throw new InvalidOperationException("Buffer is empty");

		return b;
	}

	public bool TryPull(out byte b)
	{
		if (NumUsed == 0)
		{
			b = default;
			return false;
		}

		int firstUsed = (NextFree + size - NumUsed) % size;

		b = _data[firstUsed];

		Free(1);

		return true;
	}

	public int Pull(Span<byte> buffer)
	{
		if (NumUsed == 0)
			throw new InvalidOperationException("Buffer is empty");

		int firstUsed = (NextFree + size - NumUsed) % size;

		int contiguousUsed = Math.Min(NumUsed, size - firstUsed);

		int copySize = Math.Min(contiguousUsed, buffer.Length);

		GetBufferSpan(firstUsed, copySize).CopyTo(buffer);

		Free(copySize);

		return copySize;
	}

	public void Use(int numBytes)
	{
		if (NumUsed + numBytes > size)
			throw new InvalidOperationException();

		NextFree = (NextFree + numBytes) % size;
		NumUsed += numBytes;
	}

	public void Free(int numBytes)
	{
		if (numBytes > NumUsed)
			throw new InvalidOperationException();

		NumUsed -= numBytes;
	}

	public Span<byte> GetBufferSpan(int offset, int count)
		=> _data.AsSpan().Slice(offset, count);

	public void CopyTo(Span<byte> buffer)
	{
		var dataSpan = _data.AsSpan();

		int firstUsed = NextFree - NumUsed;

		if (firstUsed >= 0)
			dataSpan.Slice(firstUsed, NumUsed).CopyTo(buffer);
		else
		{
			int firstPart = -firstUsed;
			int secondPart = NumUsed - firstPart;

			firstUsed += size;

			dataSpan.Slice(firstUsed, firstPart).CopyTo(buffer);
			dataSpan.Slice(0, secondPart).CopyTo(buffer.Slice(firstPart));
		}
	}
}
