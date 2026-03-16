using System;

using QBX.Hardware;
using QBX.OperatingSystem.FileStructures;

namespace QBX.OperatingSystem.FileDescriptors;

public delegate void FileReadFunctor(ref FileBuffer buffer);
public delegate int FileWriteFunctor(ReadOnlySpan<byte> buffer);

public abstract class FileDescriptor
{
	public int ReferenceCount;

	public string Path { get; private set; }

	protected virtual void ReadCore(ref FileBuffer buffer) { }
	protected virtual int WriteCore(ReadOnlySpan<byte> buffer) => 0;
	protected virtual void CloseCore() { }

	protected virtual void MarkDirty() { }

	public IOMode IOMode { get; private set; }
	public bool AtSoftEOF;
	public bool IsClosed;

	public void VerifyOpen()
	{
		if (IsClosed)
			throw new DOSException(DOSError.InvalidFunction);
	}

	public void SetIOMode(IOMode ioMode)
	{
		VerifyOpen();

		IOMode = ioMode;

		if (ioMode == IOMode.Binary)
			AtSoftEOF = false;
	}

	public virtual bool CanRead => false;
	public virtual bool CanWrite => false;
	public virtual bool CanSeek => false;

	public virtual bool ReadyToRead => false;
	public virtual bool ReadyToWrite => false;

	public virtual long FilePointer => 0;

	protected virtual bool ReadAndWriteAreIndependent => false;

	public FileBuffer ReadBuffer = new FileBuffer(512);
	public FileBuffer WriteBuffer = new FileBuffer(512);

	public int Column;
	public virtual bool WriteThrough { get; }

	public FileDescriptor(string path)
	{
		Path = path;
	}

	public virtual IDisposable? NonBlocking() => null;

	public bool WouldHaveBlocked = false;

	public virtual void SetAttributes(FileAttributes attributes)
	{
		throw new DOSException(DOSError.InvalidFunction);
	}

	public virtual void Lock(uint offset, uint length)
	{
		throw new DOSException(DOSError.InvalidFunction);
	}

	public virtual void Unlock(uint offset, uint length)
	{
		throw new DOSException(DOSError.InvalidFunction);
	}

	public uint Seek(int offset, MoveMethod moveMethod)
	{
		VerifyOpen();

		if (!CanSeek)
			throw new DOSException(DOSError.InvalidFunction);

		FlushWriteBuffer();
		CancelReadBuffer();

		if (offset != 0)
			AtSoftEOF = false;

		return SeekCore(offset, moveMethod);
	}

	public uint Seek(uint offset, MoveMethod moveMethod)
	{
		VerifyOpen();

		if (!CanSeek)
			throw new DOSException(DOSError.InvalidFunction);

		FlushWriteBuffer();
		CancelReadBuffer();

		if (offset != 0)
			AtSoftEOF = false;

		return SeekCore(offset, moveMethod);
	}

	protected abstract uint SeekCore(int offset, MoveMethod moveMethod);
	protected abstract uint SeekCore(uint offset, MoveMethod moveMethod);

	void FillReadBuffer()
	{
		VerifyOpen();

		if (CanRead)
		{
			if (!ReadBuffer.IsFull)
				ReadCore(ref ReadBuffer);
		}
	}

	protected virtual void CancelReadBuffer()
	{
		ReadBuffer.NumUsed = 0;
	}

	public void FlushWriteBuffer(bool flushToDisk = false)
	{
		VerifyOpen();

		if (CanWrite)
		{
			int firstUsed = WriteBuffer.NextFree - WriteBuffer.NumUsed;

			int numWritten;

			if (firstUsed < 0)
			{
				int firstPart = -firstUsed;

				numWritten = WriteCore(WriteBuffer.GetBufferSpan(WriteBuffer.Size + firstUsed, firstPart));

				if (numWritten <= 0)
					throw new Exception("Write failed"); // TODO: proper type

				WriteBuffer.Free(numWritten);

				if (numWritten < firstPart)
					return;

				firstUsed = 0;
			}

			if (WriteBuffer.NumUsed > 0)
			{
				numWritten = WriteCore(WriteBuffer.GetBufferSpan(firstUsed, WriteBuffer.NumUsed));

				if (numWritten <= 0)
					throw new Exception("Write failed"); // TODO: proper type

				WriteBuffer.Free(numWritten);
			}

			if (flushToDisk)
				FlushToDisk();
		}
	}

	protected virtual void FlushToDisk() { VerifyOpen(); }

	public void SetBufferSize(int bufferSize)
	{
		FlushWriteBuffer();

		ReadBuffer = new FileBuffer(bufferSize);
		WriteBuffer = new FileBuffer(bufferSize);
	}

	public virtual bool AtReadBoundary => false;

	public byte ReadByte()
	{
		TryReadByte(out var b);

		return b;
	}

	public bool TryReadByte(out byte b)
	{
		if (!CanRead)
			throw new DOSException(DOSError.InvalidAccess);

		VerifyOpen();

		if (AtSoftEOF || (FilePointer < 0))
		{
			b = 0;
			return false;
		}

		if (!ReadAndWriteAreIndependent)
			FlushWriteBuffer();

		while (ReadBuffer.IsEmpty && !WouldHaveBlocked)
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
		if (!CanRead)
			throw new DOSException(DOSError.InvalidAccess);

		VerifyOpen();

		if (FilePointer < 0)
			return 0; // Behaviour observed with DOS 6

		if (!ReadAndWriteAreIndependent)
			FlushWriteBuffer();

		while (ReadBuffer.IsEmpty)
			FillReadBuffer();

		return ReadBuffer.Pull(buffer);
	}

	public int Read(int readSize, IMemory systemMemory, int address)
	{
		if (!CanRead)
			throw new DOSException(DOSError.InvalidAccess);

		VerifyOpen();

		if (FilePointer < 0)
			return 0; // Behaviour observed with DOS 6

		if (!ReadAndWriteAreIndependent)
			FlushWriteBuffer();

		if (ReadBuffer.IsEmpty)
			FillReadBuffer();

		int count = 0;

		while ((readSize > 0) && !ReadBuffer.IsEmpty)
		{
			systemMemory[address++] = ReadBuffer.Pull();
			count++;
			readSize--;
		}

		return count;
	}

	public bool ReadExactly(int readSize, IMemory systemMemory, int address)
	{
		VerifyOpen();

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
		if (!CanWrite)
			throw new DOSException(DOSError.InvalidAccess);

		VerifyOpen();

		if (FilePointer < 0)
			return;

		if (!ReadAndWriteAreIndependent)
			CancelReadBuffer();

		WriteBuffer.Push(b);

		MarkDirty();

		if (WriteThrough || WriteBuffer.IsFull)
			FlushWriteBuffer();
	}

	public int Write(ReadOnlySpan<byte> buffer)
	{
		if (!CanWrite)
			throw new DOSException(DOSError.InvalidAccess);

		VerifyOpen();

		if (FilePointer < 0)
			return 0; // Behaviour observed with DOS 6

		if (!ReadAndWriteAreIndependent)
			CancelReadBuffer();

		int bytesWritten = 0;

		while (buffer.Length > 0)
		{
			int available = WriteBuffer.Available;

			int writeSize = Math.Min(available, buffer.Length);

			WriteBuffer.Push(buffer.Slice(0, writeSize));

			buffer = buffer.Slice(writeSize);
			bytesWritten += writeSize;

			MarkDirty();

			if (WriteThrough || WriteBuffer.IsFull)
				FlushWriteBuffer();
		}

		return bytesWritten;
	}

	public int Write(int writeSize, IMemory systemMemory, int address)
	{
		if (!CanWrite)
			throw new DOSException(DOSError.InvalidAccess);

		VerifyOpen();

		if (FilePointer < 0)
			return 0; // Behaviour observed with DOS 6

		if (!ReadAndWriteAreIndependent)
			CancelReadBuffer();

		int bytesWritten = 0;

		while (bytesWritten < writeSize)
		{
			if (WriteBuffer.IsFull)
				FlushWriteBuffer();

			WriteBuffer.Push(systemMemory[address++]);

			MarkDirty();

			bytesWritten++;
		}

		return bytesWritten;
	}

	public void Close()
	{
		if (!IsClosed)
		{
			if (CanWrite)
				FlushWriteBuffer();

			CloseCore();
			IsClosed = true;
		}
	}

	public virtual void TranslateByte(ref byte b) { }
}
