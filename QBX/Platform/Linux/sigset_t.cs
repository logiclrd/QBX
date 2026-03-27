using System.Runtime.InteropServices;

namespace QBX.Platform.Linux;

[StructLayout(LayoutKind.Sequential)]
public struct sigset_t
{
	const int _SIGSET_NWORDS = 1024 / (8 * sizeof(long));

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = _SIGSET_NWORDS)]
	public long[] sig = new long[_SIGSET_NWORDS];

	public sigset_t() {}
}

