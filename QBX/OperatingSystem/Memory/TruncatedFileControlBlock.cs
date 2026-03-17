using System;
using System.Runtime.InteropServices;

using QBX.OperatingSystem.FileStructures;

namespace QBX.OperatingSystem.Memory;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct TruncatedFileControlBlock
{
	[FieldOffset(0)] public byte DriveIdentifier;
	[FieldOffset(1)] public TruncatedFileControlBlockFileName FileNameBytes; // FILENAMEEXT, the dot is implicit
	[FieldOffset(12)] public ushort CurrentBlockNumber;
	[FieldOffset(14)] public ushort RecordSize;

	public bool TryParseFileName(ReadOnlySpan<byte> bytes)
	{
		const byte NotSpecified = 0xFF;

		byte driveIdentifier = NotSpecified;

		if (PathCharacter.TryGetDriveLetter(bytes, out byte driveLetter))
		{
			driveIdentifier = (byte)(PathCharacter.ToUpper(driveLetter) - 'A' + 1);
			bytes = bytes.Slice(2);
		}

		if (!FileControlBlock.TrySetFileName(bytes, FileNameBytes))
			return false;

		if (driveIdentifier != NotSpecified)
			DriveIdentifier = driveIdentifier;

		return true;
	}
}
