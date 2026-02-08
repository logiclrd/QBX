using System;
using System.IO;

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
	public int SearchID;   // "Reserved 2"
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

	public void SetFileName(string fileName)
	{
		int dot = fileName.LastIndexOf('.');

		int extraDot = fileName.IndexOf('.');

		if (extraDot != dot)
			throw new Exception("Invalid filename");

		FileNameBytes.AsSpan().Clear();

		int nameLength = Math.Min(dot, 8);
		int extLength = Math.Min(fileName.Length - dot - 1, 3);

		var inSpan = fileName.AsSpan();
		var outSpan = FileNameBytes.AsSpan();

		for (int i = 0; i < nameLength; i++)
			outSpan[i] = CP437Encoding.GetByteSemantic(inSpan[i]);

		inSpan = inSpan.Slice(dot + 1);
		outSpan = outSpan.Slice(8);

		for (int i = 0; i < extLength; i++)
			outSpan[i] = CP437Encoding.GetByteSemantic(inSpan[i]);
	}

	public int RecordPointer => CurrentBlockNumber * RecordSize + RecordPointer;

	public int MemoryAddress;

	public static FileControlBlock Deserialize(SystemMemory memory, int address)
	{
		FileControlBlock fcb;

		if (memory[address] == ExtendedFileControlBlock.Signature)
		{
			fcb =
				new ExtendedFileControlBlock()
				{
					MemoryAddress = address,
					Attributes = (FileAttributes)memory[address + 6]
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

		fcb.DeserializeCore(memory, address);

		return fcb;
	}

	public virtual void Serialize(SystemMemory memory)
		=> Serialize(memory, MemoryAddress);

	protected void Serialize(SystemMemory memory, int address)
	{
		var stream = new SystemMemoryStream(memory, address, 36);

		stream.WriteByte(DriveIdentifier);
		stream.Write(FileNameBytes);

		var writer = new BinaryWriter(stream);

		writer.Write(CurrentBlockNumber);
		writer.Write(RecordSize);
		writer.Write(FileSize);
		writer.Write(DateStamp.Raw);
		writer.Write(TimeStamp.Raw);
		writer.Write(FileHandle);
		writer.Write(SearchID);

		writer.Write(CurrentRecordNumber);
		writer.Write(unchecked((byte)RandomRecordNumber));
		writer.Write(unchecked((byte)(RandomRecordNumber >> 8)));
		writer.Write(unchecked((byte)(RandomRecordNumber >> 16)));
	}

	protected void DeserializeCore(SystemMemory memory, int address)
	{
		var stream = new SystemMemoryStream(memory, address, 36);

		DriveIdentifier = (byte)stream.ReadByte();
		stream.ReadExactly(FileNameBytes);

		var reader = new BinaryReader(stream);

		CurrentBlockNumber = reader.ReadUInt16();
		RecordSize = reader.ReadUInt16();
		FileSize = reader.ReadUInt32();
		DateStamp.Raw = reader.ReadUInt16();
		TimeStamp.Raw = reader.ReadUInt16();
		FileHandle = reader.ReadInt32();
		SearchID = reader.ReadInt32();
		RandomRecordNumber = reader.ReadUInt32();

		// Unpack final fields, separating a byte and a 24-bit integer.
		CurrentRecordNumber = unchecked((byte)RandomRecordNumber);
		RandomRecordNumber >>= 8;
	}
}
