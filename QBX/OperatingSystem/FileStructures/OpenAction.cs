using System;

namespace QBX.OperatingSystem.FileStructures;

[Flags]
public enum OpenAction : ushort
{
	Open = 0x01,
	Truncate = 0x02,
	Create = 0x10,
}
