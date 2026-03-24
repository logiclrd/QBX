using System.Runtime.InteropServices;

namespace QBX.Platform.Windows;

[StructLayout(LayoutKind.Sequential)]
public struct CHAR_INFO
{
	public char UnicodeChar;
	public short Attributes;
}
