using System;

namespace QBX.OperatingSystem.FileStructures;

[Flags]
public enum OpenActionTaken
{
	Opened = 1,
	Created = 2,
	Replaced = 3,
}
