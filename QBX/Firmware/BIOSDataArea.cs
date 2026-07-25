using System;
using System.Buffers.Binary;

using QBX.Hardware;

namespace QBX.Firmware;

public class BIOSDataArea(Machine machine)
{
	public BDAEquipmentFlags1 EquipmentFlags1
	{
		get => (BDAEquipmentFlags1)machine.MemoryBus[0x410];
		set => machine.MemoryBus[0x410] = (byte)value;
	}

	public BDAEquipmentFlags2 EquipmentFlags2
	{
		get => (BDAEquipmentFlags2)machine.MemoryBus[0x411];
		set => machine.MemoryBus[0x411] = (byte)value;
	}

	public int MemorySize
	{
		get
		{
			Span<byte> memorySizeBytes = stackalloc byte[2];

			memorySizeBytes[0] = machine.MemoryBus[0x413];
			memorySizeBytes[1] = machine.MemoryBus[0x414];

			int memorySizeKilobytes = BinaryPrimitives.ReadInt16LittleEndian(memorySizeBytes);

			return memorySizeKilobytes * 1024;
		}
		set
		{
			int memorySizeKilobytes = (value + 1023) / 1024;

			Span<byte> memorySizeBytes = stackalloc byte[2];

			BinaryPrimitives.WriteInt16LittleEndian(memorySizeBytes, (short)memorySizeKilobytes);

			machine.MemoryBus[0x413] = memorySizeBytes[0];
			machine.MemoryBus[0x414] = memorySizeBytes[1];
		}
	}

	// Keyboard status flags (0x417, 0x418, 0x460, 0x461) are handled by the KeyboardStatus class
	// Keyboard alt input (0x419) is handled by the KeyboardAltNumPadEntry class

	public int VideoMode
	{
		get => machine.MemoryBus[0x449];
		set => machine.MemoryBus[0x449] = unchecked((byte)value);
	}

	public int VideoCharacterWidth
	{
		get => machine.MemoryBus[0x44A];
		set => machine.MemoryBus[0x44A] = unchecked((byte)value);
	}

	public int VideoCharacterHeight
	{
		get => machine.MemoryBus[0x484] + 1;
		set => machine.MemoryBus[0x484] = unchecked((byte)(value - 1));
	}

	public int VideoCharacterScans
	{
		get
		{
			Span<byte> word = stackalloc byte[2];

			word[0] = machine.MemoryBus[0x485];
			word[1] = machine.MemoryBus[0x486];

			return BinaryPrimitives.ReadInt16LittleEndian(word);
		}
		set
		{
			Span<byte> word = stackalloc byte[2];

			BinaryPrimitives.WriteInt16LittleEndian(word, unchecked((short)value));

			machine.MemoryBus[0x485] = word[0];
			machine.MemoryBus[0x486] = word[1];
		}
	}

	// Actual bytes used
	public ushort VideoPageSize
	{
		get
		{
			Span<byte> word = stackalloc byte[2];

			word[0] = machine.MemoryBus[0x44C];
			word[1] = machine.MemoryBus[0x44D];

			return BinaryPrimitives.ReadUInt16LittleEndian(word);
		}
		set
		{
			Span<byte> word = stackalloc byte[2];

			BinaryPrimitives.WriteUInt16LittleEndian(word, value);

			machine.MemoryBus[0x44C] = word[0];
			machine.MemoryBus[0x44D] = word[1];
		}
	}

	public ushort VideoStartAddress
	{
		get
		{
			Span<byte> word = stackalloc byte[2];

			word[0] = machine.MemoryBus[0x44E];
			word[1] = machine.MemoryBus[0x44F];

			return BinaryPrimitives.ReadUInt16LittleEndian(word);
		}
		set
		{
			Span<byte> word = stackalloc byte[2];

			BinaryPrimitives.WriteUInt16LittleEndian(word, value);

			machine.MemoryBus[0x44E] = word[0];
			machine.MemoryBus[0x44F] = word[1];
		}
	}

	public class CursorAddressIndexer(Machine machine)
	{
		public short this[int pageNumber]
		{
			get
			{
				Span<byte> word = stackalloc byte[2];

				int fieldAddress = 0x450 + 2 * (pageNumber & 7);

				word[0] = machine.MemoryBus[fieldAddress + 0];
				word[1] = machine.MemoryBus[fieldAddress + 1];

				return BinaryPrimitives.ReadInt16LittleEndian(word);
			}
			set
			{
				Span<byte> word = stackalloc byte[2];

				BinaryPrimitives.WriteInt16LittleEndian(word, value);

				int fieldAddress = 0x450 + 2 * (pageNumber & 7);

				machine.MemoryBus[fieldAddress + 0] = word[0];
				machine.MemoryBus[fieldAddress + 1] = word[1];
			}
		}
	}

	public readonly CursorAddressIndexer VideoCursorAddress = new CursorAddressIndexer(machine);

	public int VideoCursorStartScan
	{
		get => machine.MemoryBus[0x461];
		set => machine.MemoryBus[0x461] = unchecked((byte)value);
	}

	public int VideoCursorEndScan
	{
		get => machine.MemoryBus[0x460];
		set => machine.MemoryBus[0x460] = unchecked((byte)value);
	}

	public int VideoActivePage
	{
		get => machine.MemoryBus[0x462];
		set => machine.MemoryBus[0x462] = unchecked((byte)value);
	}

	public short VideoCRTControllerBasePort
	{
		get
		{
			Span<byte> word = stackalloc byte[2];

			word[0] = machine.MemoryBus[0x463];
			word[1] = machine.MemoryBus[0x464];

			return BinaryPrimitives.ReadInt16LittleEndian(word);
		}
		set
		{
			Span<byte> word = stackalloc byte[2];

			BinaryPrimitives.WriteInt16LittleEndian(word, value);

			machine.MemoryBus[0x463] = word[0];
			machine.MemoryBus[0x464] = word[1];
		}
	}

	public BDAVideoModeOptions VideoModeOptions
	{
		get => (BDAVideoModeOptions)machine.MemoryBus[0x487];
		set => machine.MemoryBus[0x487] = unchecked((byte)value);
	}

	public BDAVideoDisplayDataFlags VideoDisplayData
	{
		get => (BDAVideoDisplayDataFlags)machine.MemoryBus[0x489];
		set => machine.MemoryBus[0x489] = unchecked((byte)value);
	}

	// TODO: CGA emulation?
	//  40:65  byte   6845 CRT mode control register value (port 3x8h)
	//                EGA/VGA values emulate those of the MDA/CGA
	//  40:66  byte   CGA current color palette mask setting (port 3d9h)
	//                EGA and VGA values emulate the CGA

	public BDAVideoDisplayCombinationCode VideoDisplayCombinationCode
	{
		get => (BDAVideoDisplayCombinationCode)machine.MemoryBus[0x48A];
		set => machine.MemoryBus[0x48A] = unchecked((byte)value);
	}

	public void SetBreakFlag()
	{
		machine.MemoryBus[0x471] |= 0x80;
	}
}
