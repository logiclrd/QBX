using System.IO;
using System.Runtime.CompilerServices;

using QBX.ExecutionEngine.Execution;
using QBX.Hardware;

namespace QBX.OperatingSystem.FileStructures;

public class DOSFileInfo
{
	[InlineArray(length: 11)]
	public struct SearchPatternBytes
	{
		byte _element0;
	}

	[InlineArray(length: 21 - 16 /* search attribute, search pattern, search ID */)]
	public struct ReservedBytes
	{
		byte _element0;
	}

	public FileAttributes Reserved_SearchAttributes;
	public SearchPatternBytes Reserved_SearchPattern;
	public int Reserved_SearchID;
	public ReservedBytes Reserved;
	public FileAttributes Attributes;
	public FileTime FileTime;
	public FileDate FileDate;
	public uint Size;
	public StringValue FileName = StringValue.CreateFixedLength(length: 13);

	public void Serialize(IMemory memory, int address)
	{
		var stream = new SystemMemoryStream(memory, address, length: 43);

		var writer = new BinaryWriter(stream);

		writer.Write((byte)Reserved_SearchAttributes);
		writer.Write(Reserved_SearchPattern);
		writer.Write(Reserved_SearchID);
		writer.Write(Reserved);
		writer.Write((byte)Attributes);
		writer.Write(FileTime.Raw);
		writer.Write(FileDate.Raw);
		writer.Write(Size);
		writer.Write(FileName.AsSpan());
	}

	public void Deserialize(IMemory memory, int address)
	{
		var stream = new SystemMemoryStream(memory, address, length: 43);

		var reader = new BinaryReader(stream);

		Reserved_SearchAttributes = (FileAttributes)reader.ReadByte();
		reader.ReadExactly(Reserved_SearchPattern);
		Reserved_SearchID = reader.ReadInt32();
		reader.ReadExactly(Reserved);
		Attributes = (FileAttributes)reader.ReadByte();
		FileTime.Raw = reader.ReadUInt16();
		FileDate.Raw = reader.ReadUInt16();
		Size = reader.ReadUInt32();
		reader.ReadExactly(FileName.AsSpan());
	}
}
