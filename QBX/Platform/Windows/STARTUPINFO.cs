using System;
using System.Runtime.InteropServices;

namespace QBX.Platform.Windows;

[StructLayout(LayoutKind.Sequential)]
public struct STARTUPINFO
{
	public static readonly int Size = Marshal.SizeOf<STARTUPINFO>();

	public int cb;
	public readonly string? lpReserved = null;
	public string? lpDesktop;
	public string? lpTitle;
	public int dwX;
	public int dwY;
	public int dwXSize;
	public int dwYSize;
	public int dwXCountChars;
	public int dwYCountChars;
	public ConsoleAttributes dwFillAttribute;
	public StartFlags dwFlags;
	public short wShowWindow;
	public readonly short cbReserved2;
	public readonly IntPtr lpReserved2;
	public IntPtr hStdInput;
	public IntPtr hStdOutput;
	public IntPtr hStdError;

	public STARTUPINFO()
	{
		cb = Size;
	}
}
