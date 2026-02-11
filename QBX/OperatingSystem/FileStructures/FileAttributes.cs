using System;

namespace QBX.OperatingSystem.FileStructures;

[Flags]
public enum FileAttributes : byte
{
	ReadOnly = 1,
	Hidden = 2,
	System = 4,
	Volume = 8,
	Directory = 16,
	Archive = 32,
}
