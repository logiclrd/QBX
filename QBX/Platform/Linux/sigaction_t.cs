using System;
using System.Runtime.InteropServices;

namespace QBX.Platform.Linux;

[StructLayout(LayoutKind.Sequential)]
public struct sigaction_t
{
	public IntPtr sa_handler;
	public sigset_t sa_mask = new sigset_t();
	public SigActionFlags sa_flags;
	public IntPtr sa_restorer;

	public sigaction_t() {}
}

