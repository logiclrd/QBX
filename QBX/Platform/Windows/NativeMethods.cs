using System;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

using Microsoft.Win32.SafeHandles;

using QBX.Utility;

namespace QBX.Platform.Windows;

public static class NativeMethods
{
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32")]
	public static extern IntPtr GetCurrentProcess();
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = false)]
	public static extern int CreatePseudoConsole(COORD size, SafePipeHandle hInput, SafePipeHandle hOutput, int dwFlags, out SafePseudoConsoleHandle phPC);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32")]
	public static extern void ClosePseudoConsole(IntPtr hPC);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	public static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, int dwFlags_ReservedZero, ref int lpSize);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	public static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, int dwFlags_ReservedZero, UIntPtr Attribute, IntPtr lpValue, UIntPtr cbSize, IntPtr lpPreviousValue_ReservedZero, IntPtr lpReturnSize_ReservedZero);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32")]
	public static extern void DeleteProcThreadAttributeList(IntPtr lpAttributeList);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern bool CreateProcessW(string? lpApplicationName, [In] StringBuilder lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, ProcessCreationFlags dwCreationFlags, IntPtr lpEnvironment, string? lpCurrentDirectory, ref STARTUPINFOEX lpStartupInfo, ref PROCESS_INFORMATION lpProcessInformation);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	public static extern bool GenerateConsoleCtrlEvent(ConsoleCtrlEvent dwCtrlEvent, int dwProcessGroupId);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	public static extern bool AttachConsole(int dwProcessId);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	public static extern IntPtr GetStdHandle(StandardHandles nStdHandle);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	public static extern bool GetConsoleMode(IntPtr hConsoleHandle, out ConsoleInputModes lpMode);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	public static extern bool SetConsoleMode(IntPtr hConsoleHandle, ConsoleInputModes dwMode);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	public static extern bool GetConsoleScreenBufferInfo(IntPtr hConsoleOutput, out CONSOLE_SCREEN_BUFFER_INFO lpConsoleScreenBufferInfo);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	public static extern bool ReadConsoleOutputW(IntPtr hConsoleOutput, [Out] CHAR_INFO[] lpBuffer, COORD dwBufferSize, COORD dwBufferCoord, ref SMALL_RECT lpReadRegion);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	public static extern bool WriteConsoleInputW(IntPtr hConsoleInput, ref INPUT_RECORD__KEY lpBuffer, int nLength, out int lpNumberOfEventsWritten);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	public static extern bool FreeConsole();
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	public static extern bool DuplicateHandle(IntPtr hSourceProcessHandle, IntPtr hSourceHandle, IntPtr hTargetProcessHandle, out IntPtr lpTargetHandle, int dwDesiredAccess, bool bInheritHandle, DuplicateOptions dwOptions);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	public static extern void CloseHandle(IntPtr hObject);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern SafeFileHandle CreateFileW(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern int GetFinalPathNameByHandleW(SafeFileHandle hFile, StringBuilder lpszFilePath, int cchFilePath, uint dwFlags);

	public const IntPtr INVALID_HANDLE_VALUE = -1;

	public const int ERROR_NONE = 0;
	public const int ERROR_INSUFFICIENT_BUFFER = 122;

	public const uint FILE_SHARE_READWRITE = 0x00000003;
	public const uint FILE_SHARE_DELETE = 0x00000004;

	public const uint OPEN_EXISTING = 3;

	public const uint FILE_FLAG_BACKUP_SEMANTICS = 0x02000000;

	public const uint FILE_NAME_NORMALIZED = 0x0;

	public const uint VOLUME_NAME_NONE = 0x4;
}
