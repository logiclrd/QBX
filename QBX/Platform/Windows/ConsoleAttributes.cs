using System;

namespace QBX.Platform.Windows;

[Flags]
public enum ConsoleAttributes : int
{
	ForegroundBlack = 0x00,
	ForegroundBlue = 0x01,
	ForegroundGreen = 0x02,
	ForegroundRed = 0x04,
	ForegroundIntensity = 0x08,

	ForegroundCyan = ForegroundBlue | ForegroundGreen,
	ForegroundYellow = ForegroundRed | ForegroundGreen,
	ForegroundMagenta = ForegroundRed | ForegroundBlue,
	ForegroundWhite = ForegroundBlue | ForegroundGreen | ForegroundRed,

	BackgroundBlack = 0x00,
	BackgroundBlue = 0x10,
	BackgroundGreen = 0x20,
	BackgroundRed = 0x40,
	BackgroundIntensity = 0x80,

	BackgroundCyan = BackgroundBlue | BackgroundGreen,
	BackgroundYellow = BackgroundRed | BackgroundGreen,
	BackgroundMagenta = BackgroundRed | BackgroundBlue,
	BackgroundWhite = BackgroundBlue | BackgroundGreen | BackgroundRed,
}
