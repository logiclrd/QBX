using System;

using QBX.OperatingSystem.FileStructures;

namespace QBX.OperatingSystem.FileDescriptors;

public class NullFileDescriptor() : FileDescriptor("NUL")
{
	protected override bool CanRead => true;
	protected override bool CanWrite => true;

	protected override uint SeekCore(int offset, MoveMethod moveMethod) => 0;
	protected override uint SeekCore(uint offset, MoveMethod moveMethod) => 0;

	protected override void ReadCore(FileBuffer buffer) { }
	protected override int WriteCore(ReadOnlySpan<byte> buffer) => buffer.Length;
}
