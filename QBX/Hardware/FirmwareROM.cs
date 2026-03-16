using System;
using System.Linq;

using QBX.Firmware;
using QBX.Parser;

namespace QBX.Hardware;

public class FirmwareROM : IMemory
{
	public const int MemoryMapSegment = 0xF000;
	public const int MemoryMapBaseAddress = MemoryMapSegment << 4;

	public const int CharacterROMOffset = 0xFA6E;

	public int Length => ROM.Length;

	byte[] ROM = new byte[64 * 1024];

	public FirmwareROM(Video videoFirmware)
	{
		// Character ROM
		var font8x8 = videoFirmware.GetFont(8).Slice(0, 128);

		var fontSpan = ROM.AsSpan().Slice(CharacterROMOffset, font8x8.Sum(glyph => glyph.Length));

		for (int i = 0; i < font8x8.Count; i++)
		{
			font8x8[i].AsSpan().CopyTo(fontSpan);
			fontSpan = fontSpan.Slice(font8x8[i].Length);
		}
	}

	public byte this[int address]
	{
		get
		{
			int offset = address - MemoryMapBaseAddress;

			if (offset < ROM.Length)
				return ROM[offset];

			return 0;
		}
		set
		{
			// No-op. This is read-only. :-)
		}
	}

	public bool TryGetSpan(int offset, int length, out Span<byte> span)
	{
		span = Span<byte>.Empty;
		return false;
	}

	public bool TryGetReadOnlySpan(int offset, int length, out ReadOnlySpan<byte> span)
	{
		offset -= MemoryMapBaseAddress;

		if ((offset >= 0)
		 && (offset < ROM.Length)
		 && (ROM.Length - offset <= length))
		{
			span = ROM.AsSpan().Slice(offset, length);
			return true;
		}

		span = ReadOnlySpan<byte>.Empty;
		return false;
	}
}
