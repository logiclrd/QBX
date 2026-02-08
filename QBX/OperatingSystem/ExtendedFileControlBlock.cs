using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using QBX.Firmware.Fonts;
using QBX.Hardware;

namespace QBX.OperatingSystem;

public class ExtendedFileControlBlock : FileControlBlock
{
	public const byte Signature = 0xFF;
	// 5 reserved bytes
	public byte AttributeByte;

	public override void Serialize(SystemMemory memory)
	{
		memory[MemoryAddress] = Signature;
		memory[MemoryAddress + 1] = 0;
		memory[MemoryAddress + 2] = 0;
		memory[MemoryAddress + 3] = 0;
		memory[MemoryAddress + 4] = 0;
		memory[MemoryAddress + 5] = 0;
		memory[MemoryAddress + 6] = AttributeByte;

		Serialize(memory, MemoryAddress + 7);
	}
}
