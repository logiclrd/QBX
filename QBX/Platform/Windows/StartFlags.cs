using System;

namespace QBX.Platform.Windows;

[Flags]
public enum StartFlags : int
{
	UseShowWindow = 0x0001,
	UseSize = 0x0002,
	UsePosition = 0x0004,
	UseCountChars = 0x0008,
	UseFillAttribute = 0x0010,
	RunFullScreen = 0x0020,
	ForceOnFeedback = 0x0040,
	ForceOffFeedback = 0x0080,
	UseStdHandles = 0x0100,
	UseHotKey = 0x0200,
	TitleIsLinkName = 0x0800,
	TitleIsAppID = 0x1000,
	PreventPinning = 0x2000,
	UntrustedSource = 0x8000,
}
