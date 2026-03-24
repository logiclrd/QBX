using System;
using System.Runtime.InteropServices;

namespace QBX.Platform.Windows;

[StructLayout(LayoutKind.Sequential)]
public struct STARTUPINFOEX
{
	public static readonly int ExtendedSize = Marshal.SizeOf<STARTUPINFOEX>();

	public STARTUPINFO StartupInfo;
	public IntPtr lpAttributeList;

	public STARTUPINFOEX()
	{
		StartupInfo.cb = ExtendedSize;
	}
}
