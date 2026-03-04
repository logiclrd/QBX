using System;
using System.IO;

using QBX.ExecutionEngine.Execution;
using QBX.Hardware;
using QBX.OperatingSystem.Memory;

namespace QBX.OperatingSystem.Processes;

public class LoadExec
{
	public const int Size = 14;

	public EnvironmentBlock Environment = new EnvironmentBlock();
	public string CommandTail = "";
	public SegmentedAddress FCB1Address;
	public SegmentedAddress FCB2Address;

	public void Serialize(IMemory systemMemory, int address, IMemoryManager memoryManager, ushort ownerPSPSegment)
	{
		var environmentBlock = Environment.Encode();

		var environmentBlockAddress = memoryManager.AllocateMemory(environmentBlock.Length, ownerPSPSegment);

		if ((environmentBlockAddress % MemoryManager.ParagraphSize) != 0)
			throw new Exception("Internal error: Allocation is not paragraph-aligned");

		for (int i = 0; i < environmentBlock.Length; i++)
			systemMemory[environmentBlockAddress + i] = environmentBlock[i];

		var commandTailBytes = new StringValue(CommandTail);

		commandTailBytes.Append(0);

		var commandTailAddress = memoryManager.AllocateMemory(commandTailBytes.Length, ownerPSPSegment);

		for (int i = 0; i < commandTailBytes.Length; i++)
			systemMemory[commandTailAddress + i] = commandTailBytes[i];

		var commandTailSegmentedAddress = new SegmentedAddress(commandTailAddress);

		var stream = new SystemMemoryStream(systemMemory, address, Size);

		var writer = new BinaryWriter(stream);

		writer.Write((ushort)(environmentBlockAddress / MemoryManager.ParagraphSize));
		writer.Write(commandTailSegmentedAddress.Offset);
		writer.Write(commandTailSegmentedAddress.Segment);
		writer.Write(FCB1Address.Offset);
		writer.Write(FCB1Address.Segment);
		writer.Write(FCB2Address.Offset);
		writer.Write(FCB2Address.Segment);
	}

	public static LoadExec Deserialize(IMemory systemMemory, int address, DOS context)
	{
		var stream = new SystemMemoryStream(systemMemory, address, Size);

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
