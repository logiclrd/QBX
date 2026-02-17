using System;
using System.IO;

using QBX.Firmware.Fonts;
using QBX.Hardware;

namespace QBX.OperatingSystem.FileStructures;

public class FileControlBlock
{
	public const int Size = 36;

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

	public void SetFileName(string fileName) => SetFileName(fileName, ref DriveIdentifier, FileNameBytes);

	public static void SetFileName(string fileName, Span<byte> fileNameBytes)
	{
		int offset = 0;

		byte driveIdentifierIgnored = 0;

		ParseFileName(
			(idx) => (offset + idx >= fileName.Length) ? (byte)0 : CP437Encoding.GetByteSemantic(fileName[offset + idx]),
			(testLength) => fileName.Length > offset + testLength,
			(numCh) => offset += numCh,
			ref driveIdentifierIgnored, fileNameBytes,
			out _, out _,
			ParseFlags.DoNotSetDefaultDriveIdentifier);
	}

	public static void SetFileName(string fileName, ref byte driveIdentifier, Span<byte> fileNameBytes)
	{
		int offset = 0;

		ParseFileName(
			(idx) => (offset + idx >= fileName.Length) ? (byte)0 : CP437Encoding.GetByteSemantic(fileName[offset + idx]),
			(testLength) => fileName.Length > testLength + offset,
			(numCh) => offset += numCh,
			ref driveIdentifier, fileNameBytes,
			out _, out _,
			ParseFlags.DoNotSetDefaultDriveIdentifier);
	}

	public static void ParseFileName(
		Func<int, byte> readInputChar, Func<int, bool> lengthIsAtLeast, Action<int> advanceInput,
		ref byte driveIdentifier, Span<byte> fileNameBytes,
		out bool containsWildcards, out bool invalidDriveLetter,
		ParseFlags flags)
	{
		containsWildcards = false;
		invalidDriveLetter = false;

		int i;

		// Parse Filename fills the fcbDriveId, fcbFileName, and fcbExtent fields of the
		// specified FCB structure unless the ParseControl parameter specifies otherwise.
		// To fill these fields, the function strips any leading white-space characters (spaces)
		// and tabs) from the string pointed to by ParseInput, then uses the remaining char-
		// acters to create the drive number, filename, and filename extension.
		while (lengthIsAtLeast(1))
		{
			byte ch = readInputChar(0);

			if ((ch == (byte)' ') || (ch == (byte)'\t'))
				advanceInput(1);
			else
				break;
		}

		//                                                                      If bit 0 in
		// ParseControl is set, the function also strips exactly one filename separator if
		// one appears before the first non-white-space character. The following are valid
		// filename separators:
		//
		// : . ; , = +
		if ((flags & ParseFlags.IgnoreLeadingSeparators) != 0)
		{
			if (lengthIsAtLeast(1))
			{
				switch (readInputChar(0))
				{
					case (byte)':':
					case (byte)'.':
					case (byte)';':
					case (byte)',':
					case (byte)'=':
					case (byte)'+':
						advanceInput(1);
						break;
				}
			}
		}

		if (readInputChar(1) == (byte)':')
		{
			driveIdentifier = unchecked((byte)(readInputChar(0) - 64));
			advanceInput(2);

			invalidDriveLetter = (driveIdentifier < 1) || (driveIdentifier > 26);
		}
		else
		{
			if ((flags & ParseFlags.DoNotSetDefaultDriveIdentifier) == 0)
				driveIdentifier = 0;
		}

		// Once Parse Filename begins to convert a filename, it continues to read charac-
		// ters from the string until it encounters a white-space character, a filename sep-
		// arator, a control character (ASCII 01h through 1Fh), or one of the following
		// characters:
		//
		// / " [ ] < > |
		int inputLength = 0;

		bool haveDot = false;

		for (i = 1; lengthIsAtLeast(i); i++)
		{
			byte b = readInputChar(i);

			bool atEnd = (b < 0x20);

			if (!atEnd)
			{
				switch (b)
				{
					case (byte)' ': case (byte)'\t':
					case (byte)':': /*case (byte)'.':*/ case (byte)';': case (byte)',': case (byte)'=': case (byte)'+':
					case (byte)'/': case (byte)'"': case (byte)'[': case (byte)']': case (byte)'<': case (byte)'>': case (byte)'|':
						atEnd = true;
						break;
					case (byte)'.':
						atEnd = haveDot;
						haveDot = true;
						break;
				}
			}

			if (atEnd)
				break;
		}

		inputLength = i;

		int dot = inputLength - 1;

		for (i = inputLength - 1; i >= 0; i--)
		{
			if (readInputChar(i) == (byte)'.')
				break;

			dot = i - 1;
		}

		if (dot < 0)
			dot = inputLength;

		int nameLength = Math.Min(dot, 8);
		int extLength = Math.Min(inputLength - dot - 1, 3);

		var outSpan = fileNameBytes;

		for (i = 0; i < nameLength; i++)
		{
			outSpan[i] = readInputChar(i);

			if (outSpan[i] == (byte)'*')
			{
				while (i < 8)
					outSpan[i++] = (byte)'?';

				containsWildcards = true;

				break;
			}
		}

		// If the filename has fewer than eight characters, the function fills the
		// remaining bytes in the fcbFileName field with space characters (ASCII 20h).
		if ((nameLength > 0) || ((flags & ParseFlags.DoNotClearOnInvalidFileName) == 0))
			while (i < 8)
				outSpan[i++] = (byte)' ';

		advanceInput(nameLength + 1);

		outSpan = outSpan.Slice(8);

		for (i = 0; i < extLength; i++)
		{
			outSpan[i] = readInputChar(i);

			if (outSpan[i] == (byte)'*')
			{
				while (i < 3)
					outSpan[i++] = (byte)'?';

				containsWildcards = true;

				break;
			}
		}

		//                                                                             If
		// the filename extension has fewer than three characters, the function fills the
		// remaining bytes in the fcbExtent field with space characters.
		if ((extLength > 0) || ((flags & ParseFlags.DoNotClearOnInvalidExtension) == 0))
			while (i < 3)
				outSpan[i++] = (byte)' ';

		advanceInput(extLength);
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

	public uint RecordPointer => CurrentBlockNumber * 128u + CurrentRecordNumber;

	public int MemoryAddress;

	public static FileControlBlock Deserialize(IMemory memory, int address)
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

	public virtual void Serialize(IMemory memory)
		=> Serialize(memory, MemoryAddress);

	protected void Serialize(IMemory memory, int address)
	{
		var stream = new SystemMemoryStream(memory, address, Size);

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

	protected void DeserializeCore(IMemory memory, int address)
	{
		var stream = new SystemMemoryStream(memory, address, Size);

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
