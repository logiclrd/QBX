using System;
using System.Diagnostics;

using QBX.Firmware.Fonts;
using QBX.OperatingSystem.FileStructures;

namespace QBX.OperatingSystem.FileDescriptors;

public class DebugFileDescriptor() : FileDescriptor("DEBUG$")
{
	public override bool WriteThrough => true;

	public override bool CanRead => false;
	public override bool CanWrite => true;

	public override bool ReadyToRead => false;
	public override bool ReadyToWrite => true;

	protected override bool ReadAndWriteAreIndependent => true;

	public override IDisposable? NonBlocking() => null;
	public IDisposable? WaitForCarriageReturn() => null;

	protected override uint SeekCore(int offset, MoveMethod moveMethod)
	{
		throw new DOSException(DOSError.InvalidFunction);
	}

	protected override uint SeekCore(uint offset, MoveMethod moveMethod)
	{
		throw new DOSException(DOSError.InvalidFunction);
	}

	protected override void ReadCore(ref FileBuffer buffer)
	{
		throw new DOSException(DOSError.InvalidFunction);
	}

	static CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

	protected override int WriteCore(ReadOnlySpan<byte> buffer)
	{
		Debug.Write(s_cp437.GetString(buffer));

		return buffer.Length;
	}
}
