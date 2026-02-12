using System;

namespace QBX.OperatingSystem.FileStructures;

[Flags]
public enum ParseFlags : byte
{
	IgnoreLeadingSeparators = 1,
	DoNotSetDefaultDriveIdentifier = 2,
	DoNotClearOnInvalidFileName = 4,
	DoNotClearOnInvalidExtension = 8,
}
