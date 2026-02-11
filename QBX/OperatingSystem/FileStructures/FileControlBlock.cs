using System;
using System.IO;
using QBX.Firmware.Fonts;
using QBX.Hardware;

namespace QBX.OperatingSystem.FileStructures;

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

	public string GetFileName() => GetFileName(FileNameBytes);

	public static string GetFileName(byte[] fileNameBytes)
	{
		s_fileNameBuffer ??= new char[12];

		int length = 8;

		while ((length > 0) && PathCharacter.IsSpace(fileNameBytes[length - 1]))
			length--;

		int offset = 0;

		while ((offset < length) && PathCharacter.IsValid(fileNameBytes[offset]))
		{
			s_fileNameBuffer[offset] = CP437Encoding.GetCharSemantic(fileNameBytes[offset]);
			offset++;
		}

		length = 3;

		while ((length > 0) && PathCharacter.IsSpace(fileNameBytes[8 + length - 1]))
			length--;

		if ((length > 0) && PathCharacter.IsValid(fileNameBytes[8]))
		{
			s_fileNameBuffer[offset++] = '.';

			int extensionOffset = 0;

			while ((extensionOffset < length) && PathCharacter.IsValid(fileNameBytes[8 + extensionOffset]))
			{
				s_fileNameBuffer[offset] = CP437Encoding.GetCharSemantic(fileNameBytes[8 + extensionOffset]);
				offset++;
				extensionOffset++;
			}
		}

		return new string(s_fileNameBuffer, 0, offset);
	}

	public void SetFileName(string fileName) => SetFileName(fileName, FileNameBytes);

	public static void SetFileName(ReadOnlySpan<char> fileName, Span<byte> fileNameBytes)
	{
		int dot = fileName.LastIndexOf('.');

		int extraDot = fileName.IndexOf('.');

		if (extraDot != dot)
			throw new Exception("Invalid filename");

		fileNameBytes.Clear();

		int nameLength = Math.Min(dot, 8);
		int extLength = Math.Min(fileName.Length - dot - 1, 3);

		var inSpan = fileName;
		var outSpan = fileNameBytes;

		for (int i = 0; i < nameLength; i++)
			outSpan[i] = CP437Encoding.GetByteSemantic(inSpan[i]);

		inSpan = inSpan.Slice(dot + 1);
		outSpan = outSpan.Slice(8);

		for (int i = 0; i < extLength; i++)
			outSpan[i] = CP437Encoding.GetByteSemantic(inSpan[i]);
	}

	public static void SetFileName(ReadOnlySpan<byte> fileName, Span<byte> fileNameBytes)
	{
		int dot = fileName.LastIndexOf((byte)'.');

		int extraDot = fileName.IndexOf((byte)'.');

		if (extraDot != dot)
			throw new Exception("Invalid filename");

		fileNameBytes.Clear();

		int nameLength = Math.Min(dot, 8);

		var inSpan = fileName;
		var outSpan = fileNameBytes;

		inSpan.Slice(0, nameLength).CopyTo(outSpan);
		inSpan.Slice(dot + 1).CopyTo(outSpan.Slice(8));
	}

	public int RecordPointer => CurrentBlockNumber * 128 + CurrentRecordNumber;

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
