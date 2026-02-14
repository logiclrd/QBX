using System;

using QBX.Hardware;
using QBX.OperatingSystem.FileStructures;

namespace QBX.OperatingSystem.FileDescriptors;

public delegate void FileReadFunctor(FileBuffer buffer);
public delegate int FileWriteFunctor(ReadOnlySpan<byte> buffer);

public abstract class FileDescriptor
{
	protected virtual void ReadCore(FileBuffer buffer) { }
	protected virtual int WriteCore(ReadOnlySpan<byte> buffer) => 0;
	protected virtual void CloseCore() { }

	public IOMode IOMode { set; private get; }
	public bool AtSoftEOF;

	public void SetIOMode(IOMode ioMode)
	{
		IOMode = ioMode;

		if (ioMode == IOMode.Binary)
			AtSoftEOF = false;
	}

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

	public virtual void SetAttributes(FileAttributes attributes)
	{
		throw new DOSException(DOSError.InvalidFunction);
	}

	public uint Seek(int offset, MoveMethod moveMethod)
	{
		FlushWriteBuffer();

		ReadBuffer.NumUsed = 0;

		if (offset != 0)
			AtSoftEOF = false;

		return SeekCore(offset, moveMethod);
	}

	public uint Seek(uint offset, MoveMethod moveMethod)
	{
		FlushWriteBuffer();

		ReadBuffer.NumUsed = 0;

		if (offset != 0)
			AtSoftEOF = false;

		return SeekCore(offset, moveMethod);
	}

	protected abstract uint SeekCore(int offset, MoveMethod moveMethod);
	protected abstract uint SeekCore(uint offset, MoveMethod moveMethod);

	void FillReadBuffer()
	{
		if (CanRead)
		{
			if (!ReadBuffer.IsFull)
				ReadCore(ReadBuffer);
		}
	}

	public void FlushWriteBuffer(bool flushToDisk = false)
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

			if (flushToDisk)
				FlushToDisk();
		}
	}

	protected virtual void FlushToDisk() { }

	public byte ReadByte()
	{
		if (!TryReadByte(out var b))
			throw new System.IO.EndOfStreamException();

		return b;
	}

	public bool TryReadByte(out byte b)
	{
		if (AtSoftEOF)
		{
			b = 0;
			return false;
		}

		while (ReadBuffer.IsEmpty)
			FillReadBuffer();

		bool ret = ReadBuffer.TryPull(out b);

		if (ret && (b == 26) && (IOMode == IOMode.ASCII))
		{
			AtSoftEOF = true;
			return false;
		}

		return ret;
	}

	public int Read(Span<byte> buffer)
	{
		while (ReadBuffer.IsEmpty)
			FillReadBuffer();

		return ReadBuffer.Pull(buffer);
	}

	public int Read(int readSize, IMemory systemMemory, int address)
	{
		if (ReadBuffer.IsEmpty)
			FillReadBuffer();

		int count = 0;

		while ((readSize > 0) && !ReadBuffer.IsEmpty)
		{
			systemMemory[address++] = ReadBuffer.Pull();
			count++;
		}

		return count;
	}

	public bool ReadExactly(int readSize, IMemory systemMemory, int address)
	{
		while (readSize > 0)
		{
			int chunkSize = Read(readSize, systemMemory, address);

			if (chunkSize == 0)
				return false;

			readSize -= chunkSize;
			address += chunkSize;
		}

		return true;
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

	public void Write(int writeSize, IMemory systemMemory, int address)
	{
		while (writeSize > 0)
		{
			if (WriteBuffer.IsFull)
				FlushWriteBuffer();

			WriteBuffer.Push(systemMemory[address++]);
		}
	}

	public void Close()
	{
		CloseCore();
	}
}
