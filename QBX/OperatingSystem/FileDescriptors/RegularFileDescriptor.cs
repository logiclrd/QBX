using System;
using System.IO;

using QBX.OperatingSystem.FileStructures;

namespace QBX.OperatingSystem.FileDescriptors;

public class RegularFileDescriptor(FileStream stream) : FileDescriptor
{
	protected override bool CanRead => stream.CanRead;
	protected override bool CanWrite => stream.CanWrite;

	public override void SetAttributes(FileStructures.FileAttributes attributes)
	{
		File.SetAttributes(stream.SafeFileHandle, attributes.ToSystemFileAttributes());
	}

	protected override void FlushToDisk()
	{
		stream.Flush(flushToDisk: true);
	}

	protected override void SeekCore(uint offset)
	{
		stream.Position = offset;
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
		stream.Write(buffer);
		return buffer.Length;
	}

	protected override void CloseCore()
	{
		stream.Close();
	}
}
