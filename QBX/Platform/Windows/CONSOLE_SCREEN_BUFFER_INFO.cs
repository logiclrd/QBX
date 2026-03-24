using System.Runtime.InteropServices;

namespace QBX.Platform.Windows;

[StructLayout(LayoutKind.Sequential)]
public struct CONSOLE_SCREEN_BUFFER_INFO
{
	public COORD dwSize;
	public COORD dwCursorPosition;
	public short wAttributes;
	public SMALL_RECT srWindow;
	public COORD dwMaximumWindowSize;
}

