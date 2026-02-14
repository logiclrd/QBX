using System;

using QBX.OperatingSystem.FileStructures;
using QBX.Parser;

namespace QBX.OperatingSystem.FileDescriptors;

public class ClockFileDescriptor() : FileDescriptor("CLOCK$")
{
	protected override bool CanRead => true;
	protected override bool CanWrite => true;

	protected override uint SeekCore(int offset, MoveMethod moveMethod) => 0;
	protected override uint SeekCore(uint offset, MoveMethod moveMethod) => 0;

	// Observed behaviour on DOS 6 in a VM
	static byte[] Fill = { 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE };

	protected override void ReadCore(FileBuffer buffer)
	{
		buffer.Push(Fill.AsSpan().Slice(0, Math.Min(Fill.Length, buffer.Available)));
	}

	// TODO: What is the correct behaviour here?
	protected override int WriteCore(ReadOnlySpan<byte> buffer) => buffer.Length;
}
