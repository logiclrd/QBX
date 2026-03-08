using System;
using System.IO;
using System.Runtime.InteropServices;

using QBX.OperatingSystem.FileStructures;

using Microsoft.Win32.SafeHandles;

namespace QBX.OperatingSystem.FileDescriptors;

public class RegularFileDescriptor(string path, string physicalPath, FileStream stream) : FileDescriptor(path)
{
	public string PhysicalPath = physicalPath;

	public override bool CanRead => stream.CanRead;
	public override bool CanWrite => stream.CanWrite;
	public override bool CanSeek => stream.CanSeek;

	public override bool ReadyToRead => !AtSoftEOF && (stream.Position < stream.Length);
	public override bool ReadyToWrite => true;

	public override long FilePointer => (_negativeFilePointer != 0) ? _negativeFilePointer : (stream.Position - ReadBuffer.NumUsed + WriteBuffer.NumUsed);

	public long Length => stream.Length;

	long _negativeFilePointer = 0;

	public SafeFileHandle Handle => stream.SafeFileHandle;

	public bool IsPristine = true;

	public override void SetAttributes(FileStructures.FileAttributes attributes)
	{
		VerifyOpen();

		File.SetAttributes(stream.SafeFileHandle, attributes.ToSystemFileAttributes());
	}

	public override void Lock(uint offset, uint length)
	{
		VerifyOpen();

		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			throw new DOSException(DOSError.NotSupported);

		stream.Lock(offset, length);
	}

	public override void Unlock(uint offset, uint length)
	{
		VerifyOpen();

		if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			throw new DOSException(DOSError.NotSupported);

		stream.Unlock(offset, length);
	}

	protected override void CancelReadBuffer()
	{
		if (_negativeFilePointer == 0)
			stream.Position -= ReadBuffer.NumUsed;

		base.CancelReadBuffer();
	}

	protected override void FlushToDisk()
	{
		VerifyOpen();

		stream.Flush(flushToDisk: true);
	}

	protected override uint SeekCore(int offset, MoveMethod moveMethod)
	{
		long newPosition = offset;

		switch (moveMethod)
		{
			case MoveMethod.FromCurrent: newPosition += stream.Position; break;
			case MoveMethod.FromEnd: newPosition += stream.Length; break;
		}

		if (newPosition < 0)
		{
			_negativeFilePointer = newPosition;

			return unchecked((uint)_negativeFilePointer);
		}
		else
		{
			uint newValue = unchecked((uint)stream.Seek(offset, moveMethod.ToSeekOrigin()));

			_negativeFilePointer = 0;

			return newValue;
		}
	}

	protected override uint SeekCore(uint offset, MoveMethod moveMethod)
	{
		uint newValue = unchecked((uint)stream.Seek(offset, moveMethod.ToSeekOrigin()));

		_negativeFilePointer = 0;

		return newValue;
	}

	protected override void ReadCore(ref FileBuffer buffer)
	{
		int readSize = buffer.ContiguousAvailable;

		if (readSize > 0)
		{
			int numRead = stream.Read(buffer.GetBufferSpan(buffer.NextFree, readSize));

			if (numRead == 0)
				throw new EndOfStreamException();

			buffer.Use(numRead);
		}
	}

	protected override int WriteCore(ReadOnlySpan<byte> buffer)
	{
		stream.Write(buffer);
		return buffer.Length;
	}

	protected override void CloseCore()
	{
		stream.Close();
	}

	protected override void MarkDirty()
	{
		IsPristine = false;
	}
}
