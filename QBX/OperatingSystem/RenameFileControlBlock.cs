using System;
using System.IO;

using QBX.Firmware.Fonts;
using QBX.Hardware;

namespace QBX.OperatingSystem;

public class RenameFileControlBlock
{
	public byte DriveIdentifier;
	public byte[] OldFileNameBytes = new byte[11]; // FILENAMEEXT, the dot is implicit
	public byte[] NewFileNameBytes = new byte[11];

	[ThreadStatic]
	static char[]? s_fileNameBuffer;

	public string GetDriveLetter()
	{
		s_fileNameBuffer ??= new char[12];

		s_fileNameBuffer[0] = (char)(DriveIdentifier + 64);
		s_fileNameBuffer[1] = ':';

		return new string(s_fileNameBuffer, 0, 2);
	}

	public string GetOldFileName() => FileControlBlock.GetFileName(OldFileNameBytes);
	public string GetNewFileName() => FileControlBlock.GetFileName(NewFileNameBytes);

	public void SetOldFileName(string newValue) => FileControlBlock.SetFileName(newValue, OldFileNameBytes);
	public void SetNewFileName(string newValue) => FileControlBlock.SetFileName(newValue, NewFileNameBytes);

	public static RenameFileControlBlock Deserialize(SystemMemory memory, int address)
	{
		var rfcb = new RenameFileControlBlock();

		var stream = new SystemMemoryStream(memory, address, 36);

		rfcb.DriveIdentifier = (byte)stream.ReadByte();
		stream.ReadExactly(rfcb.OldFileNameBytes);
		stream.ReadExactly(stackalloc byte[5]);
		stream.ReadExactly(rfcb.NewFileNameBytes);
		stream.ReadExactly(stackalloc byte[9]);

		return rfcb;
	}
}
