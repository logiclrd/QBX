using System;

namespace QBX.Platform.Windows;

[Flags]
public enum DuplicateOptions
{
	DUPLICATE_CLOSE_SOURCE = 1,
	DUPLICATE_SAME_ACCESS = 2,
}

