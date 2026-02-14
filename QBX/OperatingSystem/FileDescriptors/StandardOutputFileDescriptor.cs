using System;

using QBX.OperatingSystem.FileStructures;

namespace QBX.OperatingSystem.FileDescriptors;

public class StandardOutputFileDescriptor(DOS owner) : FileDescriptor
{
	public override bool WriteThrough => true;

	protected override bool CanWrite => true;

	protected override uint SeekCore(int offset, MoveMethod moveMethod)
	{
		throw new DOSException(DOSError.InvalidFunction);
	}

	protected override uint SeekCore(uint offset, MoveMethod moveMethod)
	{
		throw new DOSException(DOSError.InvalidFunction);
	}

	protected override int WriteCore(ReadOnlySpan<byte> buffer)
	{
		var visual = owner.Machine.VideoFirmware.VisualLibrary;

		bool savedControlCharacterFlag = visual.ProcessControlCharacters;

		try
		{
			visual.ProcessControlCharacters = true;
			visual.WriteText(buffer);

			return buffer.Length;
		}
		finally
		{
			visual.ProcessControlCharacters = savedControlCharacterFlag;
		}
	}
}
