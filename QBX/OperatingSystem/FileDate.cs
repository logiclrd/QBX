using System;

namespace QBX.OperatingSystem;

public struct FileDate
{
	public ushort Raw;

	public int Year
	{
		get => (Raw >> 9) + 1980;
		set => Raw = unchecked((ushort)((Raw & 0x01FF) | (((value - 1980) & 0x7F) << 9)));
	}

	public int Month
	{
		get => (Raw >> 5) & 0xF;
		set => Raw = unchecked((ushort)((Raw & 0xFE1F) | ((value & 0x0F) << 5)));
	}

	public int Day
	{
		get => Raw & 0x1F;
		set => Raw = unchecked((ushort)((Raw & 0xFFE0) | (value & 0x1F)));
	}

	public FileDate Set(int year, int month, int day)
	{
		Raw = unchecked((ushort)(
			(((year - 1980) & 0x7F) << 9) |
			((month & 0x0F) << 5) |
			(day & 0x1F)));

		return this;
	}

	public FileDate Set(DateTime dateTime)
		=> Set(dateTime.Year, dateTime.Month,	dateTime.Day);
}
