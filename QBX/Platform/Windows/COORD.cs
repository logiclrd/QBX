using System.Runtime.InteropServices;

namespace QBX.Platform.Windows;

[StructLayout(LayoutKind.Sequential)]
public struct COORD
{
	public short X; // Column (horizontal)
	public short Y; // Row (vertical)
}

