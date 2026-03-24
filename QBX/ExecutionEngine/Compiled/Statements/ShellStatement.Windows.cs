using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;

using Microsoft.Win32.SafeHandles;

using QBX.Firmware;
using QBX.Hardware;
using QBX.Utility;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;

namespace QBX.ExecutionEngine.Compiled.Statements;

public partial class ShellStatement : Executable
{
	[SupportedOSPlatform(PlatformNames.Windows)]
	public void RunChildProcessWindows(ExecutionContext context, string fileName, string arguments)
	{
		var size =
			new COORD()
			{
				X = (short)context.VisualLibrary.CharacterWidth,
				Y = (short)context.VisualLibrary.CharacterHeight,
			};

		using (var stdinPipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable))
		using (var stdoutPipe = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable))
		{
			var stdinReadSide = stdinPipe.ClientSafePipeHandle;
			var stdoutWriteSide = stdoutPipe.ClientSafePipeHandle;

			int hResult = CreatePseudoConsole(
				size,
				stdinReadSide,
				stdoutWriteSide,
				dwFlags: 0,
				out var hPC);

			if (hResult != 0)
				throw new Win32Exception(hResult);

			try
			{
				stdinReadSide.Close();
				stdoutWriteSide.Close();

				var startupInfo = new STARTUPINFOEX();

				startupInfo.StartupInfo.dwXCountChars = size.X;
				startupInfo.StartupInfo.dwYCountChars = size.Y;
				startupInfo.StartupInfo.dwFillAttribute = ConsoleAttributes.ForegroundWhite | ConsoleAttributes.BackgroundBlack;
				startupInfo.StartupInfo.dwFlags =
					StartFlags.UseCountChars |
					StartFlags.UseFillAttribute;

				int attributeListSize = 0;

				bool success = InitializeProcThreadAttributeList(
					IntPtr.Zero,
					dwAttributeCount: 1,
					dwFlags_ReservedZero: 0,
					ref attributeListSize);

				if (success || (Marshal.GetLastWin32Error() != ERROR_INSUFFICIENT_BUFFER))
					throw new Win32Exception(Marshal.GetLastWin32Error(), "Unable to calculate proc thread attribute list structure size");

				using (var attributeListBuffer = new GlobalHeapMemoryAllocation(attributeListSize))
				using (var pseudoConsoleHandleBuffer = new GlobalHeapMemoryAllocation(IntPtr.Size))
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

						Marshal.WriteIntPtr(pseudoConsoleHandleBuffer, 0, hPC);

						success = UpdateProcThreadAttribute(
							startupInfo.lpAttributeList,
							dwFlags_ReservedZero: 0,
							ProcThreadAttributeMacroValue.PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE,
							hPC,
							(UIntPtr)IntPtr.Size,
							lpPreviousValue_ReservedZero: IntPtr.Zero,
							lpReturnSize_ReservedZero: IntPtr.Zero);

						if (!success)
							throw new Win32Exception();

						char[] argumentsBuffer = new char[arguments.Length + 1];

						arguments.CopyTo(argumentsBuffer);

						var processSecurityAttributes = new SECURITY_ATTRIBUTES();
						var threadSecurityAttributes = new SECURITY_ATTRIBUTES();

						var processInformation = new PROCESS_INFORMATION();

						success = CreateProcessW(
							fileName,
							argumentsBuffer,
							lpProcessAttributes: ref processSecurityAttributes,
							lpThreadAttributes: ref threadSecurityAttributes,
							bInheritHandles: false,
							ProcessCreationFlags.DefaultErrorMode |
							ProcessCreationFlags.NewProcessGroup |
							ProcessCreationFlags.ExtendedStartupInfoPresent |
							ProcessCreationFlags.InheritParentAffinity,
							lpEnvironment: IntPtr.Zero, // copy this process'
							lpCurrentDirectory: null, // use this process'
							ref startupInfo,
							ref processInformation);

						if (!success)
							throw new Win32Exception();

						try
						{
							void SendBreakSignal()
							{
								GenerateConsoleCtrlEvent(
									ConsoleCtrlEvent.ControlBreakEvent,
									dwProcessGroupId: processInformation.dwProcessId);
							}

							using (context.Machine.DOS.TakeOverBreakEventForScope(SendBreakSignal))
							{
								var cancellationTokenSource = new CancellationTokenSource();

								var cancellationToken = cancellationTokenSource.Token;

								var savedSemantics = context.VisualLibrary.CRLFSemantics;

								context.VisualLibrary.CRLFSemantics = Firmware.CRLFSemantics.Terminal;

								try
								{
									var textLibrary = context.VisualLibrary as TextLibrary;

									using (textLibrary?.ShowCursorForScope())
									{
										// TODO: cursor in graphics modes

										var proxyProcessID = StartInputProxy(
											context.Machine.Keyboard,
											cancellationToken,
											targetProcessID: processInformation.dwProcessId);

										try
										{
											var outputTask = CreateOutputTask(
												stdoutPipe.ReadByte,
												i => (byte)i,
												new TerminalControlSequenceProcessor().ProcessByte,
												new Lock(),
												context.VisualLibrary);

											new ProcessWaitHandle(processInformation.hProcess).WaitOne();

											ClosePseudoConsole(hPC);
											hPC = IntPtr.Zero;

											outputTask.Wait();

											cancellationTokenSource.Cancel();
										}
										finally
										{
											try
											{
												Process.GetProcessById(proxyProcessID).Kill();
											}
											catch
											{
												// https://github.com/dotnet/runtime/issues/101582
											}
										}
									}
								}
								finally
								{
									context.VisualLibrary.CRLFSemantics = savedSemantics;
								}
							}
						}
						finally
						{
							CloseHandle(processInformation.hProcess);
							CloseHandle(processInformation.hThread);
						}
					}
					finally
					{
						DeleteProcThreadAttributeList(attributeListBuffer);
					}
				}
			}
			finally
			{
				if (hPC != IntPtr.Zero)
					ClosePseudoConsole(hPC);
			}
		}
	}

	class ProcessWaitHandle : WaitHandle
	{
		public ProcessWaitHandle(IntPtr hProcess)
		{
			SafeWaitHandle = new SafeWaitHandle(hProcess, ownsHandle: false);
		}
	}

	class GlobalHeapMemoryAllocation : IDisposable
	{
		public int Size { get; }
		public IntPtr Address { get; private set; }

		public GlobalHeapMemoryAllocation(int size)
		{
			Size = size;
			Address = Marshal.AllocHGlobal(size);
		}

		public void Dispose()
		{
			if (Address != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(Address);
				Address = IntPtr.Zero;
			}
		}

		public static implicit operator IntPtr(GlobalHeapMemoryAllocation memoryAllocation)
			=> memoryAllocation.Address;
	}

	class ProxyInjector : InputInjector
	{
		BinaryWriter _writer;

		public ProxyInjector(Stream stream)
		{
			_writer = new BinaryWriter(stream);
		}

		public override void Inject(KeyEvent evt)
		{
			evt.Serialize(_writer);
		}

		public override void Dispose()
		{
			_writer.Close();
		}
	}

	public const string ProxyCommandLineSwitch = "/SHELLPROXYPROCESS ";

	[SupportedOSPlatform(PlatformNames.Windows)]
	public int StartInputProxy(Keyboard keyboard, CancellationToken cancellationToken, int targetProcessID)
	{
		var pipe = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);

		CreateInputTask(
			() => new ProxyInjector(pipe),
			new KeyboardKeyEventSource(keyboard),
			cancellationToken);

		return CreateChildProcessLimitInheritedHandles(
			fileName: Environment.ProcessPath ?? throw new Exception("Sanity failure"),
			arguments: ProxyCommandLineSwitch + pipe.GetClientHandleAsString() + " " + targetProcessID,
			pipe.ClientSafePipeHandle.DangerousGetHandle());
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

				var commandLineBuilder = new StringBuilder();

				if (fileName.Contains(' '))
					commandLineBuilder.Append('"').Append(fileName).Append('"');
				else
					commandLineBuilder.Append(fileName);

				commandLineBuilder.Append(' ').Append(arguments).Append('\0');

				char[] commandLineBuffer = commandLineBuilder.ToString().ToCharArray();

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
					ProcessCreationFlags.NewProcessGroup |
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
		return GetCommandLineTail(commandLine).StartsWith(ProxyCommandLineSwitch);
	}

	[SupportedOSPlatform(PlatformNames.Windows)]
	public static int ExecuteInputProxy()
	{
		var commandLine = GetCommandLineTail(Environment.CommandLine);

		if (!commandLine.StartsWith(ProxyCommandLineSwitch))
			return 2;

		var arguments = commandLine.Slice(ProxyCommandLineSwitch.Length).ToString().Split(' ');

		string clientHandleString = arguments[0];
		string processIDAsString = arguments[1];

		if (!int.TryParse(processIDAsString, out int processID))
			return 3;

		PumpInput(
			() => new ConsoleInputInjector(processID),
			new ProxyKeyEventReceiver(clientHandleString),
			CancellationToken.None);

		return 0;
	}

	class ProxyKeyEventReceiver(BinaryReader reader) : IKeyEventSource
	{
		public ProxyKeyEventReceiver(string pipeHandleAsString)
			: this(new AnonymousPipeClientStream(pipeHandleAsString))
		{
		}

		public ProxyKeyEventReceiver(Stream stream)
			: this(new BinaryReader(stream))
		{
		}

		public KeyEvent? ReceiveNextEvent(CancellationToken cancellationToken)
		{
			try
			{
				return KeyEvent.Deserialize(reader);
			}
			catch
			{
				return null;
			}
		}
	}

	[SupportedOSPlatform(PlatformNames.Windows)]
	class ConsoleInputInjector : InputInjector
	{
		public ConsoleInputInjector(int processID)
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

			_hStdIn = GetStdHandle(StandardHandles.STD_INPUT_HANDLE);

			if (_hStdIn == INVALID_HANDLE_VALUE)
				throw new Win32Exception();
		}

		IntPtr _hStdIn;

		public override void Inject(KeyEvent evt)
		{
			var record = new INPUT_RECORD__KEY();

			record.KeyEvent.bKeyDown = !evt.IsRelease;
			record.KeyEvent.wRepeatCount = 1;
			record.KeyEvent.wVirtualKeyCode = (short)GetVirtualKeyCode(evt.SDLScanCode);
			record.KeyEvent.wVirtualScanCode = (short)evt.ScanCode;
			record.KeyEvent.UnicodeChar = evt.TextCharacter;
			record.KeyEvent.dwControlKeyState = TranslateModifiers(evt.Modifiers);

			bool success = WriteConsoleInputW(_hStdIn, ref record, 1, out _);

			if (!success)
				throw new Win32Exception();
		}

		public override void Dispose()
		{
			FreeConsole();
		}
	}
}

