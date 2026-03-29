using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

using QBX.Platform.Windows;
using QBX.Utility;

using QBX.ExecutionEngine.Compiled.Statements.Shell.Windows;
using QBX.ExecutionEngine.Compiled.Statements.Shell.Windows.ConsoleAPI;
using QBX.ExecutionEngine.Compiled.Statements.Shell.Windows.ConsolePTY;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;

using static QBX.Platform.Windows.NativeMethods;

namespace QBX.ExecutionEngine.Compiled.Statements;

public partial class ShellStatement : Executable
{
	public bool UseConsolePTYStrategy = false;

	[SupportedOSPlatform(PlatformNames.Windows)]
	public void RunChildProcessWindows(ExecutionContext context, string fileName, string[] arguments)
	{
		WindowsConsoleShellStrategy strategy =
			UseConsolePTYStrategy
			? new ConsolePTYStrategy()
			: new ConsoleAPIStrategy();

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

			using (hPC)
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

						Marshal.WriteIntPtr(pseudoConsoleHandleBuffer, 0, hPC.DangerousGetHandle());

						success = UpdateProcThreadAttribute(
							startupInfo.lpAttributeList,
							dwFlags_ReservedZero: 0,
							ProcThreadAttributeMacroValue.PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE,
							hPC.DangerousGetHandle(),
							(UIntPtr)IntPtr.Size,
							lpPreviousValue_ReservedZero: IntPtr.Zero,
							lpReturnSize_ReservedZero: IntPtr.Zero);

						if (!success)
							throw new Win32Exception();

						// CreateProcessW expects a mutable buffer for arguments.
						var argumentsBuffer = new StringBuilder();

						foreach (var argument in arguments)
						{
							if (argumentsBuffer.Length > 0)
								argumentsBuffer.Append(' ');

							argumentsBuffer.Append(argument);
						}

						argumentsBuffer.Append('\0');

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
							ProcessCreationFlags.ExtendedStartupInfoPresent |
							ProcessCreationFlags.InheritParentAffinity |
							strategy.AdditionalProcessCreationFlags,
							lpEnvironment: IntPtr.Zero, // copy this process'
							lpCurrentDirectory: null, // use this process'
							ref startupInfo,
							ref processInformation);

						if (!success)
							throw new Win32Exception();

						int processID = processInformation.dwProcessId;

						var processExit = new ProcessWaitHandle(processInformation.hProcess);

						try
						{
							strategy.Execute(context, hPC, stdinPipe, stdoutPipe, processInformation);
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

		}
	}
}

