using System;

namespace QBX.OperatingSystem;

public delegate void FileReadFunctor(FileBuffer buffer);
public delegate int FileWriteFunctor(ReadOnlySpan<byte> buffer);

public class FileDescriptor
{
	public FileReadFunctor? ReadFunctor;
	public FileWriteFunctor? WriteFunctor;

	public FileBuffer ReadBuffer = new FileBuffer(512);
	public FileBuffer WriteBuffer = new FileBuffer(512);

	public FileDescriptor()
	{
	}

	public FileDescriptor(FileReadFunctor? readFunctor, FileWriteFunctor? writeFunctor)
	{
		ReadFunctor = readFunctor;
		WriteFunctor = writeFunctor;
	}

	void FillReadBuffer()
	{
		if (ReadFunctor != null)
		{
			if (!ReadBuffer.IsFull)
				ReadFunctor(ReadBuffer);
		}
	}

	public void FlushWriteBuffer()
	{
		if (WriteFunctor != null)
		{
			int firstUsed = WriteBuffer.NextFree - WriteBuffer.NumUsed;

			int numWritten;

			if (firstUsed < 0)
			{
				int firstPart = -firstUsed;

				numWritten = WriteFunctor(WriteBuffer.GetBufferSpan(firstUsed, firstPart));

				if (numWritten <= 0)
					throw new Exception("Write failed"); // TODO: proper type

				WriteBuffer.Free(numWritten);

				if (numWritten < firstPart)
					return;

				firstUsed = 0;
			}

			numWritten = WriteFunctor(WriteBuffer.GetBufferSpan(firstUsed, WriteBuffer.NumUsed));

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
}
