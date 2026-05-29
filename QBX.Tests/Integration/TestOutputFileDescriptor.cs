using System.Diagnostics;

using QBX.ExecutionEngine.Execution;

using QBX.Firmware.Fonts;

using QBX.OperatingSystem;
using QBX.OperatingSystem.FileDescriptors;
using QBX.OperatingSystem.FileStructures;

namespace QBX.Tests.Integration;

public class TestOutputFileDescriptor() : FileDescriptor("TESTOUT$")
{
	public readonly StringValue CapturedOutput = new StringValue();

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

	protected override int WriteCore(ReadOnlySpan<byte> buffer)
	{
		CapturedOutput.Append(buffer);

		return buffer.Length;
	}
}
