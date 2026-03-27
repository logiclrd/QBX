using System;
using System.Runtime.InteropServices;

namespace QBX.Platform.Linux;

public static class NativeMethods
{
	[DllImport("c", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
	public static extern int openpty(out int amaster, out int aslave, string? name, ref termios termp, ref winsize winp);
	[DllImport("c", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
	public static extern int fork();
	[DllImport("c", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
	public static extern int dup2(int oldfd, int newfd);
	[DllImport("c", CallingConvention = CallingConvention.Cdecl)]
	public static extern int close(int fd);
	[DllImport("c", CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
	public static extern int execvp(nint path, nint argv);
	[DllImport("c", CallingConvention = CallingConvention.Cdecl)]
	public static extern int waitpid(int pid, out int status, waitoptions_t options);
	[DllImport("c", CallingConvention = CallingConvention.Cdecl)]
	public static extern int sigfillset(out sigset_t set);
	[DllImport("c", CallingConvention = CallingConvention.Cdecl)]
	public static extern int sigaddset(ref sigset_t set, int signum);
	[DllImport("c", CallingConvention = CallingConvention.Cdecl)]
	public static extern int sigdelset(ref sigset_t set, int signum);
	[DllImport("c", CallingConvention = CallingConvention.Cdecl)]
	public static extern int pthread_sigmask(ProcMaskHow how, [In] ref sigset_t set, out sigset_t oldset);
	[DllImport("c", CallingConvention = CallingConvention.Cdecl)]
	public static extern int pthread_sigmask(ProcMaskHow how, [In] ref sigset_t set, IntPtr oldset);
	[DllImport("c", CallingConvention = CallingConvention.Cdecl)]
	public static extern int pthread_sigmask(ProcMaskHow how, IntPtr set, IntPtr oldset);
	[DllImport("c", CallingConvention = CallingConvention.Cdecl)]
	public static extern int sigaction(int signum, IntPtr act, out sigaction_t oldact);
	[DllImport("c", CallingConvention = CallingConvention.Cdecl)]
	public static extern int sigaction(int signum, IntPtr act, IntPtr oldact);
	[DllImport("c", CallingConvention = CallingConvention.Cdecl)]
	public static extern int sigaction(int signum, [In] ref sigaction_t act, IntPtr oldact);

	public const byte INTR = 3;
	public const byte QUIT = 28;
	public const byte ERASE = 127;
	public const byte KILL = 21;
	public const byte EOF = 4;
	public const byte TIME = 0;
	public const byte MIN = 0;
	public const byte SWITCH_none = 0;
	public const byte START = 19;
	public const byte STOP = 17;
	public const byte SUSP = 26;
	public const byte EOL_NUL = 0;
	public const byte REPRINT = 18;
	public const byte DISCARD = 15;
	public const byte WERASE = 23;
	public const byte LNEXT = 22;

	public const int STDIN_FILENO = 0;
	public const int STDOUT_FILENO = 1;
	public const int STDERR_FILENO = 2;

	public const nint SIG_DFL = 0; // Default action.
	public const nint SIG_IGN = 1; // Ignore signal.
}

