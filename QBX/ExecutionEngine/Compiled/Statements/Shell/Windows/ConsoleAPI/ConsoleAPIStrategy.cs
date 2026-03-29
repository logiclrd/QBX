using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using QBX.Firmware;
using QBX.Hardware;
using QBX.Platform.Windows;
using QBX.Utility;

using static QBX.Platform.Windows.NativeMethods;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;

namespace QBX.ExecutionEngine.Compiled.Statements.Shell.Windows.ConsoleAPI;

public class ConsoleAPIStrategy : WindowsConsoleShellStrategy
{
	//public override ProcessCreationFlags AdditionalProcessCreationFlags => ProcessCreationFlags.NewProcessGroup;

	public static readonly TimeSpan PollingInterval = TimeSpan.FromMilliseconds(1000);

	[SupportedOSPlatform(PlatformNames.Windows)]
	public override void Execute(ExecutionContext context, SafePseudoConsoleHandle hPC, AnonymousPipeServerStream ptyStdinPipe, AnonymousPipeServerStream ptyStdoutPipe, PROCESS_INFORMATION processInformation)
	{
		void SendBreakSignal()
		{
			GenerateConsoleCtrlEvent(
				ConsoleCtrlEvent.CTRL_BREAK_EVENT,
				dwProcessGroupId: 0);
		}

		using (context.Machine.DOS.TakeOverBreakEventForScope(SendBreakSignal))
		{
			var processExit = new ProcessWaitHandle(processInformation.hProcess);

			var cancellationTokenSource = new CancellationTokenSource();

			try
			{
				var cancellationToken = cancellationTokenSource.Token;

				var savedSemantics = context.VisualLibrary.CRLFSemantics;

				context.VisualLibrary.CRLFSemantics = Firmware.CRLFSemantics.Terminal;

				try
				{
					var textLibrary = context.VisualLibrary as TextLibrary;

					using (textLibrary?.ShowCursorForScope())
					{
						// TODO: cursor in graphics modes

						var proxyProcessID = StartProxy(
							context.Machine.Keyboard,
							context.VisualLibrary,
							cancellationToken,
							ptyStdinPipe,
							ptyStdoutPipe,
							targetProcessID: processInformation.dwProcessId);

						using (var proxyProcess = Process.GetProcessById(proxyProcessID))
						{
							try
							{
								processExit.WaitOne();

								hPC.Close();

								cancellationTokenSource.Cancel();
							}
							finally
							{
								try
								{
									proxyProcess.WaitForExit(TimeSpan.FromSeconds(1));
									proxyProcess.Kill();
								}
								catch { }
							}
						}
					}
				}
				finally
				{
					context.VisualLibrary.CRLFSemantics = savedSemantics;
				}
			}
			finally
			{
				cancellationTokenSource.Cancel();
			}
		}
	}

	public const string ProxyCommandLineSwitch = "/SHELLPROXYPROCESS ";

	[SupportedOSPlatform(PlatformNames.Windows)]
	public int StartProxy(Keyboard keyboard, VisualLibrary visualLibrary, CancellationToken cancellationToken, PipeStream ptyStdinStream, PipeStream ptyStdoutStream, int targetProcessID)
	{
		var stdinPipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
		var stdoutPipe = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);

		CreateInputTask(
			() => new ProxyInjector(stdinPipe),
			new KeyboardKeyEventSource(keyboard),
			cancellationToken);

		CreateBufferSnapshotReceiverTask(
			stdoutPipe,
			visualLibrary);

		// The PTY stdout pipe handles are not inheritable (per the design of AnonymousPipeServerStream).
		// We need to make inheritable duplicates and then close them once the child has inherited them.
		bool success = DuplicateHandle(
			hSourceProcessHandle: GetCurrentProcess(),
			hSourceHandle: ptyStdinStream.SafePipeHandle.DangerousGetHandle(),
			hTargetProcessHandle: GetCurrentProcess(),
			lpTargetHandle: out var ptyStdinHandleInheritable,
			dwDesiredAccess: default,
			bInheritHandle: true,
			dwOptions: DuplicateOptions.DUPLICATE_SAME_ACCESS);

		if (!success)
			throw new Win32Exception();

		success = DuplicateHandle(
			hSourceProcessHandle: GetCurrentProcess(),
			hSourceHandle: ptyStdoutStream.SafePipeHandle.DangerousGetHandle(),
			hTargetProcessHandle: GetCurrentProcess(),
			lpTargetHandle: out var ptyStdoutHandleInheritable,
			dwDesiredAccess: default,
			bInheritHandle: true,
			dwOptions: DuplicateOptions.DUPLICATE_SAME_ACCESS);

		if (!success)
			throw new Win32Exception();

		try
		{
			string arguments = string.Join(" ",
				stdinPipe.GetClientHandleAsString(),
				stdoutPipe.GetClientHandleAsString(),
				(long)ptyStdinHandleInheritable,
				(long)ptyStdoutHandleInheritable,
				targetProcessID);

			return CreateChildProcessLimitInheritedHandles(
				fileName: Environment.ProcessPath ?? throw new Exception("Sanity failure"),
				arguments: ProxyCommandLineSwitch + arguments,
				stdinPipe.ClientSafePipeHandle.DangerousGetHandle(),
				stdoutPipe.ClientSafePipeHandle.DangerousGetHandle(),
				ptyStdinHandleInheritable,
				ptyStdoutHandleInheritable);
		}
		finally
		{
			CloseHandle(ptyStdinHandleInheritable);
			CloseHandle(ptyStdoutHandleInheritable);
		}
	}

	Task CreateBufferSnapshotReceiverTask(Stream pipe, VisualLibrary target)
	{
		return Task.Run(
			() =>
			{
				try
				{
					var deltaEmitter = new ConsoleUpdateSpanWritesGenerator(target);

					while (true)
					{
						var snapshot = ConsoleBufferSnapshot.Deserialize(pipe);

						deltaEmitter.ApplySnapshot(snapshot);
					}
				}
				catch { }
			});
	}

	[SupportedOSPlatform(PlatformNames.Windows)]
	static int CreateChildProcessLimitInheritedHandles(
		string fileName,
		string arguments,
		params IntPtr[] inheritHandles)
	{
		var startupInfo = new STARTUPINFOEX();

		int attributeListSize = 0;

		bool success = InitializeProcThreadAttributeList(
			IntPtr.Zero,
			dwAttributeCount: 1,
			dwFlags_ReservedZero: 0,
			ref attributeListSize);

		if (success || (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER))
			throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to calculate proc thread attribute list structure size");

		using (var attributeListBuffer = new GlobalHeapMemoryAllocation(attributeListSize))
		using (var inheritHandlesBuffer = new GlobalHeapMemoryAllocation(inheritHandles.Length * IntPtr.Size))
		{
			success = InitializeProcThreadAttributeList(
				attributeListBuffer,
				dwAttributeCount: 1,
				dwFlags_ReservedZero: 0,
				ref attributeListSize);

			if (!success)
				throw new Win32Exception();

			try
			{
				startupInfo.lpAttributeList = attributeListBuffer;

				for (int i=0; i < inheritHandles.Length; i++)
					Marshal.WriteIntPtr(inheritHandlesBuffer, i * IntPtr.Size, inheritHandles[i]);

				success = UpdateProcThreadAttribute(
					startupInfo.lpAttributeList,
					dwFlags_ReservedZero: 0,
					ProcThreadAttributeMacroValue.PROC_THREAD_ATTRIBUTE_HANDLE_LIST,
					inheritHandlesBuffer,
					(UIntPtr)inheritHandlesBuffer.Size,
					lpPreviousValue_ReservedZero: IntPtr.Zero,
					lpReturnSize_ReservedZero: IntPtr.Zero);

				if (!success)
					throw new Win32Exception();

				// CreateProcessW requires the arguments to include the program filename as the
				// first token, and also expects a mutable buffer for arguments, so we can
				// just use the StringBuilder used to build the string as the arguments parameter.
				var commandLineBuffer = new StringBuilder();

				if (fileName.Contains(' '))
					commandLineBuffer.Append('"').Append(fileName).Append('"');
				else
					commandLineBuffer.Append(fileName);

				commandLineBuffer.Append(' ').Append(arguments).Append('\0');

				var processSecurityAttributes = new SECURITY_ATTRIBUTES();
				var threadSecurityAttributes = new SECURITY_ATTRIBUTES();

				var processInformation = new PROCESS_INFORMATION();

				success = CreateProcessW(
					fileName,
					commandLineBuffer,
					lpProcessAttributes: ref processSecurityAttributes,
					lpThreadAttributes: ref threadSecurityAttributes,
					bInheritHandles: true,
					ProcessCreationFlags.DefaultErrorMode |
					ProcessCreationFlags.NoWindow |
					ProcessCreationFlags.ExtendedStartupInfoPresent |
					ProcessCreationFlags.InheritParentAffinity,
					lpEnvironment: IntPtr.Zero, // copy this process'
					lpCurrentDirectory: null, // use this process'
					ref startupInfo,
					ref processInformation);

				if (!success)
					throw new Win32Exception();

				CloseHandle(processInformation.hProcess);
				CloseHandle(processInformation.hThread);

				return processInformation.dwProcessId;
			}
			finally
			{
				DeleteProcThreadAttributeList(attributeListBuffer);
			}
		}
	}

	static ReadOnlySpan<char> GetCommandLineTail(string commandLine)
	{
		var span = commandLine.AsSpan();
		var tail = span;

		if (span[0] == '"')
		{
			int quote = span.Slice(1).IndexOf('"');

			if (quote < 0)
				return null;

			tail = span.Slice(quote + 2).TrimStart();
		}
		else
		{
			int space = commandLine.IndexOf(' ');

			if (space < 0)
				return null;

			tail = commandLine.AsSpan().Slice(space + 1).TrimStart();
		}

		return tail;
	}

	public static bool IsProxyCommandLine(string commandLine)
	{
		return
			RuntimeInformation.IsOSPlatform(OSPlatform.Windows) &&
			GetCommandLineTail(commandLine).StartsWith(ProxyCommandLineSwitch);
	}

	[SupportedOSPlatform(PlatformNames.Windows)]
	public static int ExecuteProxy()
	{
		var commandLine = GetCommandLineTail(Environment.CommandLine);

		if (!commandLine.StartsWith(ProxyCommandLineSwitch))
			return 2;

		var arguments = commandLine.Slice(ProxyCommandLineSwitch.Length).ToString().Split(' ');

		string stdinClientHandleString = arguments[0];
		string stdoutClientHandleString = arguments[1];
		string ptyStdinHandleString = arguments[2];
		string ptyStdoutHandleString = arguments[3];
		string processIDAsString = arguments[4];

		if (!int.TryParse(processIDAsString, out int processID))
			return 3;

		AttachToProcessConsole(processID);

		Console.CancelKeyPress +=
			(sender, e) =>
			{
				e.Cancel = true;
			};

		var hStdIn = GetStdHandle(StandardHandles.STD_INPUT_HANDLE);

		GetConsoleMode(hStdIn, out var inputMode);
		SetConsoleMode(hStdIn, inputMode | ConsoleInputModes.ENABLE_PROCESSED_INPUT);

		var inputEventInputPipe = new AnonymousPipeClientStream(PipeDirection.In, stdinClientHandleString);
		var snapshotOutputPipe = new AnonymousPipeClientStream(PipeDirection.Out, stdoutClientHandleString);

		var ptyStdinPipe = new AnonymousPipeClientStream(PipeDirection.Out, ptyStdinHandleString);
		var ptyStdoutPipe = new AnonymousPipeClientStream(PipeDirection.In, ptyStdoutHandleString);

		var triggerEvent = new AutoResetEvent(initialState: true);

		CreateSnapshotTriggerTask(ptyStdoutPipe, triggerEvent);
		CreateBufferSnapshotSenderTask(snapshotOutputPipe, triggerEvent);

		PumpInput(
			() => new ConsoleInputInjector(ptyStdinPipe),
			new ProxyKeyEventReceiver(inputEventInputPipe),
			CancellationToken.None);

		return 0;
	}

	[SupportedOSPlatform(PlatformNames.Windows)]
	static void AttachToProcessConsole(int processID)
	{
		var deadline = DateTime.UtcNow.AddMilliseconds(500);

		bool success = false;

		while (DateTime.UtcNow < deadline)
		{
			success = AttachConsole(processID);

			if (success)
				break;
		}

		if (!success)
			throw new Win32Exception();
	}

	static Task CreateSnapshotTriggerTask(Stream ptyStdoutPipe, AutoResetEvent triggerEvent)
	{
		return Task.Run(
			() =>
			{
				byte[] ptyDiscardBuffer = new byte[4096];

				while (true)
				{
					int numPtyBytes = ptyStdoutPipe.Read(ptyDiscardBuffer, 0, ptyDiscardBuffer.Length);

					triggerEvent.Set();
				}
			});
	}

	[SupportedOSPlatform(PlatformNames.Windows)]
	static Task CreateBufferSnapshotSenderTask(Stream snapshotOutputPipe, AutoResetEvent triggerEvent)
	{
		return Task.Run(
			() =>
			{
				var consoleBuffer = new ConsoleDeltaGenerator();

				try
				{
					while (true)
					{
						triggerEvent.WaitOne(PollingInterval);

						if (consoleBuffer.TryGetChangeSnapshot(out var snapshot))
							snapshot.Serialize(snapshotOutputPipe);
					}
				}
				catch { }
			});
	}
}
