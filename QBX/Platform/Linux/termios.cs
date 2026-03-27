using System.Runtime.InteropServices;

namespace QBX.Platform.Linux;

[StructLayout(LayoutKind.Sequential, Pack = 4)]
public struct termios
{
	public const int NCCS = 32;

	public tcflag_i_t c_iflag;
	public tcflag_o_t c_oflag;
	public tcflag_c_t c_cflag;
	public tcflag_l_t c_lflag;

	public byte c_line;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = NCCS)]
	public byte[] c_cc;

	public uint c_ispeed;
	public uint c_ospeed;
}

