using System;

namespace QBX.OperatingSystem.FileStructures;

[Flags]
public enum FileAccess : byte
{
	Access_ReadOnly  = 0x00,
	Access_WriteOnly = 0x01,
	Access_ReadWrite = 0x02,
	AccessMask       = 0x03,

	Share_Compatibility = 0x00,
	Share_DenyReadWrite = 0x10,
	Share_DenyWrite     = 0x20,
	Share_DenyRead      = 0x30,
	Share_DenyNone      = 0x40,
	ShareMask           = 0x70,

	Flags_NoInherit = 0x80,
	FlagsMask       = 0x80,
}
