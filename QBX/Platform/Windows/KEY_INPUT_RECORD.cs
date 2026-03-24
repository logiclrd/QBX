using System.Runtime.InteropServices;

namespace QBX.Platform.Windows;

[StructLayout(LayoutKind.Sequential)]
public struct KEY_INPUT_RECORD
{
	public bool bKeyDown;
	public short wRepeatCount;
	public short wVirtualKeyCode;
	public short wVirtualScanCode;
	public char UnicodeChar;
	public KeyInputRecordModifiers dwControlKeyState;
}
