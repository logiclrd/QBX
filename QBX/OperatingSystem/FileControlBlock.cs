using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using QBX.Firmware.Fonts;
using QBX.Hardware;

namespace QBX.OperatingSystem;

public class FileControlBlock
{
	public byte DriveIdentifier;
	public byte[] FileNameBytes = new byte[11]; // FILENAMEEXT, the dot is implicit
	public ushort CurrentBlockNumber;
	public ushort RecordSize = 128;
	public uint FileSize;
	public FileDate DateStamp;
	public FileTime TimeStamp;
	public int FileHandle; // "Reserved 1"
	public int Reserved;   // "Reserved 2"
	public byte CurrentRecordNumber;
	public uint RandomRecordNumber;

	[ThreadStatic]
	static char[]? s_fileNameBuffer;

	public string GetDriveLetter()
	{
		s_fileNameBuffer ??= new char[12];

		s_fileNameBuffer[0] = (char)(DriveIdentifier + 64);
		s_fileNameBuffer[1] = ':';

		return new string(s_fileNameBuffer, 0, 2);
	}

	public string GetFileName()
	{
		s_fileNameBuffer ??= new char[12];

		int length = 8;

		while ((length > 0) && PathCharacter.IsSpace(FileNameBytes[length - 1]))
			length--;

		int offset = 0;

		while ((offset < length) && PathCharacter.IsValid(FileNameBytes[offset]))
		{
			s_fileNameBuffer[offset] = CP437Encoding.GetCharSemantic(FileNameBytes[offset]);
			offset++;
		}

		length = 3;

		while ((length > 0) && PathCharacter.IsSpace(FileNameBytes[8 + length - 1]))
			length--;

		if ((length > 0) && PathCharacter.IsValid(FileNameBytes[8]))
		{
			s_fileNameBuffer[offset++] = '.';

			int extensionOffset = 0;

			while ((extensionOffset < length) && PathCharacter.IsValid(FileNameBytes[8 + extensionOffset]))
			{
				s_fileNameBuffer[offset] = CP437Encoding.GetCharSemantic(FileNameBytes[8 + extensionOffset]);
				offset++;
				extensionOffset++;
			}
		}

		return new string(s_fileNameBuffer, 0, offset);
	}

	public int RecordPointer => CurrentBlockNumber * RecordSize + RecordPointer;

	public int MemoryAddress;

	[ThreadStatic]
	static byte[]? s_data;

	public static FileControlBlock Deserialize(SystemMemory memory, int address)
	{
		FileControlBlock fcb;

		if (memory[address] == ExtendedFileControlBlock.Signature)
		{
			fcb =
				new ExtendedFileControlBlock()
				{
					MemoryAddress = address,
					AttributeByte = memory[address + 6]
				};

			address += 7;
		}
		else
		{
			fcb =
				new FileControlBlock()
				{
					MemoryAddress = address,
				};
		}

		fcb.Deserialize(memory);

		return fcb;
	}

	public virtual void Serialize(SystemMemory memory)
		=> Serialize(memory, MemoryAddress);

	protected void Serialize(SystemMemory memory, int address)
	{
		s_data ??= new byte[36];

		var dataSpan = s_data.AsSpan();

		dataSpan[0] = DriveIdentifier;
		dataSpan = dataSpan.Slice(1);

		FileNameBytes.CopyTo(dataSpan);
		dataSpan = dataSpan.Slice(11);

		var ushortSpan = MemoryMarshal.Cast<byte, ushort>(dataSpan);

		ushortSpan[0] = CurrentBlockNumber;
		ushortSpan[1] = RecordSize;
		ushortSpan = ushortSpan.Slice(2);

		var uintSpan = MemoryMarshal.Cast<ushort, uint>(ushortSpan);

		uintSpan[0] = FileSize;
		uintSpan = uintSpan.Slice(1);

		ushortSpan = MemoryMarshal.Cast<uint, ushort>(uintSpan);

		ushortSpan[0] = DateStamp.Raw;
		ushortSpan[1] = TimeStamp.Raw;
		ushortSpan = ushortSpan.Slice(2);

		var intSpan = MemoryMarshal.Cast<ushort, int>(ushortSpan);

		intSpan[0] = FileHandle;
		intSpan[1] = Reserved;
		intSpan = intSpan.Slice(2);

		dataSpan = MemoryMarshal.Cast<int, byte>(intSpan);

		dataSpan[0] = CurrentRecordNumber;
		dataSpan[1] = unchecked((byte)RandomRecordNumber);
		dataSpan[2] = unchecked((byte)(RandomRecordNumber >> 8));
		dataSpan[3] = unchecked((byte)(RandomRecordNumber >> 16));

		for (int i = 0; i < 36; i++)
			memory[address + i] = dataSpan[i];
	}

	void Deserialize(SystemMemory memory)
	{
		s_data ??= new byte[36];

		var dataSpan = s_data.AsSpan();

		for (int i = 0; i < 36; i++)
			dataSpan[i] = memory[MemoryAddress + i];

		DriveIdentifier = dataSpan[0];
		dataSpan = dataSpan.Slice(1);

		dataSpan.Slice(0, 11).CopyTo(FileNameBytes);
		dataSpan = dataSpan.Slice(11);

		var ushortSpan = MemoryMarshal.Cast<byte, ushort>(dataSpan);

		CurrentBlockNumber = ushortSpan[0];
		RecordSize = ushortSpan[1];
		ushortSpan = ushortSpan.Slice(2);

		var uintSpan = MemoryMarshal.Cast<ushort, uint>(ushortSpan);

		FileSize = uintSpan[0];
		uintSpan = uintSpan.Slice(1);

		ushortSpan = MemoryMarshal.Cast<uint, ushort>(uintSpan);

		DateStamp.Raw = ushortSpan[0];
		TimeStamp.Raw = ushortSpan[1];
		ushortSpan = ushortSpan.Slice(2);

		var intSpan = MemoryMarshal.Cast<ushort, int>(ushortSpan);

		FileHandle = intSpan[0];
		Reserved = intSpan[1];
		intSpan = intSpan.Slice(2);

		uintSpan = MemoryMarshal.Cast<int, uint>(intSpan);

		RandomRecordNumber = uintSpan[0];

		// Unpack final fields, separating a byte and a 24-bit integer.
		CurrentRecordNumber = unchecked((byte)RandomRecordNumber);
		RandomRecordNumber >>= 8;
	}
}
