using System;

namespace QBX.OperatingSystem.FileDescriptors;

public delegate void FileReadFunctor(FileBuffer buffer);
public delegate int FileWriteFunctor(ReadOnlySpan<byte> buffer);

public abstract class FileDescriptor
{
	protected virtual void ReadCore(FileBuffer buffer) { }
	protected virtual int WriteCore(ReadOnlySpan<byte> buffer) => 0;
	protected virtual void CloseCore() { }

	protected virtual bool CanRead => false;
	protected virtual bool CanWrite => false;

	public FileBuffer ReadBuffer = new FileBuffer(512);
	public FileBuffer WriteBuffer = new FileBuffer(512);

	public int Column;
	public virtual bool WriteThrough { get; }

	public FileDescriptor()
	{
	}

	public virtual IDisposable? NonBlocking() => null;

	public bool WouldHaveBlocked = false;

	void FillReadBuffer()
	{
		if (CanRead)
		{
			if (!ReadBuffer.IsFull)
				ReadCore(ReadBuffer);
		}
	}

	public void FlushWriteBuffer()
	{
		if (CanWrite)
		{
			int firstUsed = WriteBuffer.NextFree - WriteBuffer.NumUsed;

			int numWritten;

			if (firstUsed < 0)
			{
				int firstPart = -firstUsed;

				numWritten = WriteCore(WriteBuffer.GetBufferSpan(firstUsed, firstPart));

				if (numWritten <= 0)
					throw new Exception("Write failed"); // TODO: proper type

				WriteBuffer.Free(numWritten);

				if (numWritten < firstPart)
					return;

				firstUsed = 0;
			}

			numWritten = WriteCore(WriteBuffer.GetBufferSpan(firstUsed, WriteBuffer.NumUsed));

			if (numWritten <= 0)
				throw new Exception("Write failed"); // TODO: proper type

			WriteBuffer.Free(numWritten);
		}
	}

	public byte ReadByte()
	{
		while (ReadBuffer.IsEmpty)
			FillReadBuffer();

		return ReadBuffer.Pull();
	}

	public int Read(Span<byte> buffer)
	{
		while (ReadBuffer.IsEmpty)
			FillReadBuffer();

		return ReadBuffer.Pull(buffer);
	}

	public void WriteByte(byte b)
	{
		WriteBuffer.Push(b);

		if (WriteThrough || WriteBuffer.IsFull)
			FlushWriteBuffer();
	}

	public void Write(ReadOnlySpan<byte> buffer)
	{
		while (buffer.Length > 0)
		{
			int available = WriteBuffer.Available;

			int writeSize = Math.Min(available, buffer.Length);

			WriteBuffer.Push(buffer.Slice(0, writeSize));

			buffer = buffer.Slice(writeSize);

			if (WriteThrough || WriteBuffer.IsFull)
				FlushWriteBuffer();
		}
	}

	public void Close()
	{
		CloseCore();
	}
}
