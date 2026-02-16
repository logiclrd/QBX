using System;

namespace QBX.OperatingSystem.FileStructures;

[Flags]
public enum OpenMode : ushort
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

	Flags_NoInherit        = 0x0080,
	Flags_NoCriticalErrors = 0x2000,
	Flags_Commit           = 0x4000,
	FlagsMask              = 0xFF80,
}
