using System;
using System.IO;

using QBX.OperatingSystem.FileStructures;

using Microsoft.Win32.SafeHandles;

namespace QBX.OperatingSystem.FileDescriptors;

public class RegularFileDescriptor(string path, FileStream stream) : FileDescriptor(path)
{
	public override bool CanRead => stream.CanRead;
	public override bool CanWrite => stream.CanWrite;

	public override bool ReadyToRead => !AtSoftEOF && (stream.Position < stream.Length);
	public override bool ReadyToWrite => true;

	public SafeFileHandle Handle => stream.SafeFileHandle;

	public bool IsPristine = true;

	public override void SetAttributes(FileStructures.FileAttributes attributes)
	{
		VerifyOpen();

		File.SetAttributes(stream.SafeFileHandle, attributes.ToSystemFileAttributes());
	}

	protected override void FlushToDisk()
	{
		VerifyOpen();

		stream.Flush(flushToDisk: true);
	}

	protected override uint SeekCore(int offset, MoveMethod moveMethod)
	{
		return unchecked((uint)stream.Seek(offset, moveMethod.ToSeekOrigin()));
	}

	protected override uint SeekCore(uint offset, MoveMethod moveMethod)
	{
		return unchecked((uint)stream.Seek(offset, moveMethod.ToSeekOrigin()));
	}

	protected override void ReadCore(FileBuffer buffer)
	{
		int readSize = buffer.ContiguousAvailable;

		if (readSize > 0)
		{
			int numRead = stream.Read(buffer.GetBufferSpan(buffer.NextFree, readSize));

			buffer.Use(numRead);
		}
	}

	protected override int WriteCore(ReadOnlySpan<byte> buffer)
	{
		IsPristine = false;
		stream.Write(buffer);
		return buffer.Length;
	}

	protected override void CloseCore()
	{
		stream.Close();
	}
}
