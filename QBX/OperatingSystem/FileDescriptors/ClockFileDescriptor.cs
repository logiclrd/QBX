using System;

using QBX.OperatingSystem.FileStructures;
using QBX.Parser;

namespace QBX.OperatingSystem.FileDescriptors;

public class ClockFileDescriptor() : FileDescriptor("CLOCK$")
{
	public override bool CanRead => true;
	public override bool CanWrite => true;

	public override bool ReadyToRead => true;
	public override bool ReadyToWrite => false;

	protected override uint SeekCore(int offset, MoveMethod moveMethod) => 0;
	protected override uint SeekCore(uint offset, MoveMethod moveMethod) => 0;

	// Observed behaviour on DOS 6 in a VM
	static byte[] Fill = { 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE, 0xCE };

	protected override void ReadCore(ref FileBuffer buffer)
	{
		buffer.Push(Fill.AsSpan().Slice(0, Math.Min(Fill.Length, buffer.Available)));
	}

	// TODO: What is the correct behaviour here?
	protected override int WriteCore(ReadOnlySpan<byte> buffer) => buffer.Length;
}
