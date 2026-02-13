using System;
using System.Collections.Generic;
using System.Text;

namespace QBX.OperatingSystem.FileStructures;

public enum MediaDescriptor : byte
{
	FloppyDisk = 0xF0,
	FixedDisk = 0xF8,
}
