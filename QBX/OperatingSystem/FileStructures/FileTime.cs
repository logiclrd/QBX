using System;

namespace QBX.OperatingSystem.FileStructures;

public struct FileTime
{
	public ushort Raw;

	public int Hour
	{
		get => Raw >> 11;
		set => Raw = unchecked((ushort)((Raw & 0x07FF) | (((value - 1980) & 0x1F) << 11)));
	}

	public int Minute
	{
		get => (Raw >> 5) & 0x3F;
		set => Raw = unchecked((ushort)((Raw & 0xF81F) | ((value & 0x3F) << 5)));
	}

	public int Second
	{
		get => 2 * (Raw & 0x1F);
		set => Raw = unchecked((ushort)((Raw & 0xFFE0) | ((value >> 1) & 0x1F)));
	}

	public FileTime Set(int hour, int minute, int second)
	{
		Raw = unchecked((ushort)(
			((hour & 0x1F) << 11) |
			((minute & 0x3F) << 5) |
			((second >> 1) & 0x1F)));

		return this;
	}

	public FileTime Set(DateTime dateTime)
		=> Set(dateTime.Hour, dateTime.Minute, dateTime.Second);
}
