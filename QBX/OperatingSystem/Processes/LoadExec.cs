using System.IO;

using QBX.Hardware;
using QBX.OperatingSystem.Memory;

namespace QBX.OperatingSystem.Processes;

public class LoadExec
{
	public EnvironmentBlock Environment = new EnvironmentBlock();
	public string CommandTail = "";
	public SegmentedAddress FCB1Address;
	public SegmentedAddress FCB2Address;

	public static LoadExec Deserialize(IMemory systemMemory, int address, DOS context)
	{
		var stream = new SystemMemoryStream(systemMemory, address, 14);

		var reader = new BinaryReader(stream);

		var ret = new LoadExec();

		int environmentBlockSegment = reader.ReadInt16();

		ret.Environment.Decode(systemMemory, environmentBlockSegment * MemoryManager.ParagraphSize, context);

		if (context.LastError != DOSError.None)
			return ret;

		var commandTailAddress = new SegmentedAddress();

		commandTailAddress.Offset = reader.ReadUInt16();
		commandTailAddress.Segment = reader.ReadUInt16();

		ret.CommandTail = context.ReadStringZ(systemMemory, commandTailAddress.ToLinearAddress()).ToString();

		if (context.LastError != DOSError.None)
			return ret;

		ret.FCB1Address.Offset = reader.ReadUInt16();
		ret.FCB1Address.Segment = reader.ReadUInt16();

		ret.FCB2Address.Offset = reader.ReadUInt16();
		ret.FCB2Address.Segment = reader.ReadUInt16();

		return ret;
	}
}
