using QBX.Hardware;

namespace QBX.OperatingSystem.FileStructures;

public class ExtendedFileControlBlock : FileControlBlock
{
	public const byte Signature = 0xFF;
	// 5 reserved bytes
	public FileAttributes Attributes;

	public override void Serialize(IMemory memory)
	{
		memory[MemoryAddress] = Signature;
		memory[MemoryAddress + 1] = 0;
		memory[MemoryAddress + 2] = 0;
		memory[MemoryAddress + 3] = 0;
		memory[MemoryAddress + 4] = 0;
		memory[MemoryAddress + 5] = 0;
		memory[MemoryAddress + 6] = (byte)Attributes;

		Serialize(memory, MemoryAddress + 7);
	}
}
