using System;

using QBX.OperatingSystem.FileStructures;

namespace QBX.OperatingSystem.FileDescriptors;

public class NullFileDescriptor() : FileDescriptor("NUL")
{
	public override bool CanRead => true;
	public override bool CanWrite => true;

	public override bool ReadyToRead => false;
	public override bool ReadyToWrite => true;

	protected override uint SeekCore(int offset, MoveMethod moveMethod) => 0;
	protected override uint SeekCore(uint offset, MoveMethod moveMethod) => 0;

	protected override void ReadCore(ref FileBuffer buffer) { }
	protected override int WriteCore(ReadOnlySpan<byte> buffer) => buffer.Length;
}
