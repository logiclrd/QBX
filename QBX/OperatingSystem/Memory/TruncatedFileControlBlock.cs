using System;
using System.Runtime.InteropServices;

namespace QBX.OperatingSystem.Memory;

[StructLayout(LayoutKind.Explicit, Size = 16)]
public struct TruncatedFileControlBlock
{
	[FieldOffset(0)] public byte DriveIdentifier;
	[FieldOffset(1)] public TruncatedFileControlBlockFileName FileNameBytes; // FILENAMEEXT, the dot is implicit
	[FieldOffset(12)] public ushort CurrentBlockNumber;
	[FieldOffset(14)] public ushort RecordSize;

	public void ParseFileName(ReadOnlySpan<byte> bytes)
	{
		if ((bytes.Length >= 2) && (bytes[1] == (byte)':'))
		{
			DriveIdentifier = (byte)(bytes[0] - 'A' + 1);
			bytes = bytes.Slice(2);
		}

		FileControlBlock.SetFileName(bytes, FileNameBytes);
	}
}
