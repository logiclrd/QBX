using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

using Microsoft.Win32.SafeHandles;

using QBX.ExecutionEngine.Compiled.Functions;
using QBX.ExecutionEngine.Compiled.Statements.Shell.Unix;
using QBX.Platform.Linux;
using QBX.Utility;

using static QBX.Platform.Linux.NativeMethods;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;

namespace QBX.ExecutionEngine.Compiled.Statements;

public partial class ShellStatement : Executable
{
	[SupportedOSPlatform(PlatformNames.Linux)]
	[SupportedOSPlatform(PlatformNames.MacOS)]
	public void RunChildProcessLinux(ExecutionContext context, string fileName, string arguments)
	{
		var term = new termios();

		term.c_iflag =
			tcflag_i_t.InterruptOnBreak |
			tcflag_i_t.IgnoreFramingAndParityErrors |
			tcflag_i_t.TranslateInputCarriageReturnToNewLine |
			tcflag_i_t.ResumeStoppedOutputOnAny |
			tcflag_i_t.BellOnInputQueueFull;

		term.c_oflag =
			tcflag_o_t.TranslateOutputNewLineToCRNL;

		term.c_cflag =
			tcflag_c_t.CharacterSize_8 |
			tcflag_c_t.EnableReceiver |
			tcflag_c_t.HangUpAfterClose |
			tcflag_c_t.IgnoreModeControl;

		term.c_lflag =
			tcflag_l_t.SignalOnSignalCharacters |
			tcflag_l_t.CanonicalMode |
			tcflag_l_t.EchoInputCharacters |
			tcflag_l_t.EnableEraseCharacters |
			tcflag_l_t.EnableKillLineCharacter |
			tcflag_l_t.DisplayControlCharactersWithCaret |
			tcflag_l_t.EnableInputPreProcessing;

		term.c_cc = new byte[termios.NCCS];

		byte[] standardControlCharacters =
			[ INTR, QUIT, ERASE, KILL, EOF, TIME, MIN, SWITCH_none, START, STOP, SUSP, EOL_NUL, REPRINT, DISCARD, WERASE, LNEXT, EOL_NUL ];

		standardControlCharacters.CopyTo(term.c_cc);

		var win = new winsize();

		win.Columns = 80;
		win.Rows = 25;
		win.PixelsX = 640;
		win.PixelsY = 400;

		int result = openpty(
			out int masterFD,
			out int slaveFD,
			name: null,
			ref term,
			ref win);

		if (result != 0)
			throw new Win32Exception();

		using (var masterFDHandle = new SafeFileHandle(masterFD, ownsHandle: true))
		using (var slaveFDHandle = new SafeFileHandle(slaveFD, ownsHandle: true))
		using (var ptyStream = new FileStream(masterFDHandle, FileAccess.ReadWrite))
		{
			int childPID = ForkExec(fileName, arguments, masterFD, slaveFD);

			slaveFDHandle.Close();

			var strategy = new TTYStrategy();

			strategy.Execute(
				context,
				ptyStream,
				childPID);
		}
	}

	private static int ForkExec(string fileName, string arguments, int masterFD, int slaveFD)
	{
		string[] argv = ConstructArgumentVector(fileName, arguments);

		// Ensure that P/Invoke thunks are in place for the functions we need to call before exec,
		// because it might not be safe/possible for this dynamic process to run in the forked
		// child context.
		close(-1);
		dup2(-1, -1);
		sigaction(Signals.SIGCHLD, 0, out var oldAct);
		sigaction(Signals.SIGCHLD, ref oldAct, 0);

		sigset_t dummy = new sigset_t();

		sigaddset(ref dummy, Signals.SIGCONT);

		pthread_sigmask(ProcMaskHow.SIG_UNBLOCK, ref dummy, 0);

		// To avoid heap allocations post-fork, manually marshal the execvp arguments ahead of time.
		nint fileNamePtr = 0;
		nint argvPtr = 0;
		nint sa_default_ptr = 0;
		nint old_signal_set_ptr = 0;

		try
		{
			fileNamePtr = Marshal.StringToHGlobalAnsi(fileName);

			int argvSlots = argv.Length + 1; // include NULL terminator

			argvPtr = Marshal.AllocHGlobal(argvSlots * nint.Size);

			// Zero fill the entire array first, so that the cleanup code can deal with initialization aborting.
			for (int i=0; i < argvSlots; i++)
				Marshal.WriteIntPtr(argvPtr, i * nint.Size, 0);

			for (int i=0; i < argv.Length; i++)
			{
				nint argPtr = Marshal.StringToHGlobalAnsi(argv[i]);

				Marshal.WriteIntPtr(argvPtr, i * nint.Size, argPtr);
			}

			sigaction_t sa_default = new sigaction_t();

			sa_default.sa_handler = SIG_DFL;

			sa_default_ptr = Marshal.AllocHGlobal(Marshal.SizeOf(sa_default));

			Marshal.StructureToPtr(sa_default, sa_default_ptr, fDeleteOld: false);

			old_signal_set_ptr = Marshal.AllocHGlobal(Marshal.SizeOf<sigset_t>());

			// The following code cannot be debugged conventionally:
			// - Debugging depends on the use of SIGTRAP to interrupt execution.
			// - The fork() call returns twice, and one of those returns is not attached to any debugger.

			// The fork child must not be signalled until it calls exec(): the CLR's signal handlers do not
			// handle being raised in the child process correctly.
			sigfillset(out var signal_set);
			pthread_sigmask(ProcMaskHow.SIG_SETMASK, ref signal_set, out var old_signal_set);

			Marshal.StructureToPtr(old_signal_set, old_signal_set_ptr, fDeleteOld: false);

			// The next line creates the child process. Only this thread is cloned into the child process.
			int childPID = fork();

			if (childPID != 0)
			{
				//pthread_sigmask(ProcMaskHow.SIG_SETMASK, ref old_signal_set, IntPtr.Zero);

				if (childPID < 0)
					throw new Win32Exception();
				if (childPID > 0)
					return childPID;
			}

			System.Threading.Thread.Sleep(10000);

			// We're now in the child process context. As a .NET process, the current process is completely broken.
			// No throwing of exceptions or returning from this function. :-)

			try
			{
				if (dup2(slaveFD, STDIN_FILENO) < 0)
					Environment.Exit(Marshal.GetLastPInvokeError());
				if (dup2(slaveFD, STDOUT_FILENO) < 0)
					Environment.Exit(Marshal.GetLastPInvokeError());
				if (dup2(slaveFD, STDERR_FILENO) < 0)
					Environment.Exit(Marshal.GetLastPInvokeError());

				// Reinitialize signal handler configuration. Replace all custom handlers with SIG_DFL.
				for (int sig = 1; sig < Signals.NSIG; ++sig)
				{
					if ((sig == Signals.SIGKILL)
					 || (sig == Signals.SIGSTOP))
						continue;

					if (sigaction(sig, 0, out var sa_old) == 0)
					{
						var oldhandler = sa_old.sa_handler;

						if ((oldhandler != SIG_IGN)
						 && (oldhandler != SIG_DFL))
						{
							// It has a custom handler, put the default handler back.
							// We check first to preserve flags on default handlers.
							sigaction(sig, sa_default_ptr, 0);
						}
					}
				}

				pthread_sigmask(ProcMaskHow.SIG_SETMASK, old_signal_set_ptr, oldset: IntPtr.Zero);

				//execvp(fileNamePtr, argvPtr);
			}
			finally
			{
				Environment.Exit(Marshal.GetLastPInvokeError());

				// The analyzer needs to know this method doesn't ever return.
				throw new Exception("Sanity failure");
			}
		}
		finally
		{
			// This can never run in the child process. If the execvp works, then this
			// frame of execution evaporates before unwinding. If the execvp fails, then
			// the inner try's finally block calls exit().

			if (fileNamePtr != 0)
				Marshal.FreeHGlobal(fileNamePtr);

			if (argvPtr != 0)
			{
				for (int i=0; i < argv.Length; i++)
				{
					nint argPtr = Marshal.ReadIntPtr(argvPtr, i * nint.Size);

					if (argPtr != 0)
						Marshal.FreeHGlobal(argPtr);
				}

				Marshal.FreeHGlobal(argvPtr);
			}

			if (sa_default_ptr != 0)
				Marshal.FreeHGlobal(sa_default_ptr);
			if (old_signal_set_ptr != 0)
				Marshal.FreeHGlobal(old_signal_set_ptr);
		}
	}

	static char[] ArgumentDelimiters = [ ' ', '\t', '\n', '"' ];

	static string[] ConstructArgumentVector(string fileName, string commandTail)
	{
		var argv = new List<string>();

		argv.Add(fileName);

		var argBuffer = new StringBuilder();

		var tailSpan = commandTail.AsSpan();

		while (tailSpan.Length > 0)
		{
			if (tailSpan[0] == '"')
			{
				int endQuote = tailSpan.Slice(1).IndexOf('"');

				if (endQuote < 0)
				{
					argBuffer.Append(tailSpan.Slice(1));
					break;
				}

				argBuffer.Append(tailSpan.Slice(1,  endQuote));
				tailSpan = tailSpan.Slice(1 + endQuote);
			}
			else if (ArgumentDelimiters.Contains(tailSpan[0]))
			{
				if (argBuffer.Length > 0)
				{
					argv.Add(argBuffer.ToString());
					argBuffer.Length = 0;
				}

				tailSpan = tailSpan.Slice(1);
			}
			else
			{
				int space = tailSpan.IndexOfAny(ArgumentDelimiters);

				if (space < 0)
				{
					argBuffer.Append(tailSpan);
					break;
				}

				argBuffer.Append(tailSpan.Slice(0, space));
				tailSpan = tailSpan.Slice(space);
			}
		}

		if (argBuffer.Length > 0)
			argv.Add(argBuffer.ToString());

		return argv.ToArray();
	}
}
