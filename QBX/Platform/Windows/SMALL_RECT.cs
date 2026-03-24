using System.Runtime.InteropServices;

namespace QBX.Platform.Windows;

[StructLayout(LayoutKind.Sequential)]
public struct SMALL_RECT
{
	public short Left;
	public short Top;
	public short Right;
	public short Bottom;
}

