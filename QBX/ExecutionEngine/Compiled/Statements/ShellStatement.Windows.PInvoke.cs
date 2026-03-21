using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Channels;

using Microsoft.Win32.SafeHandles;

using QBX.DevelopmentEnvironment;
using QBX.ExecutionEngine.Compiled.BitwiseOperators;
using QBX.ExecutionEngine.Compiled.RelationalOperators;
using QBX.Hardware;
using QBX.Utility;

using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace QBX.ExecutionEngine.Compiled.Statements;

public partial class ShellStatement : Executable
{
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = false)]
	static extern int CreatePseudoConsole(COORD size, SafePipeHandle hInput, SafePipeHandle hOutput, int dwFlags, out IntPtr phPC);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32")]
	static extern void ClosePseudoConsole(IntPtr hPC);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	static extern bool InitializeProcThreadAttributeList(IntPtr lpAttributeList, int dwAttributeCount, int dwFlags_ReservedZero, ref int lpSize);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	static extern bool UpdateProcThreadAttribute(IntPtr lpAttributeList, int dwFlags_ReservedZero, UIntPtr Attribute, IntPtr lpValue, UIntPtr cbSize, IntPtr lpPreviousValue_ReservedZero, IntPtr lpReturnSize_ReservedZero);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32")]
	static extern void DeleteProcThreadAttributeList(IntPtr lpAttributeList);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
	static extern bool CreateProcessW(string? lpApplicationName, char[] lpCommandLine, ref SECURITY_ATTRIBUTES lpProcessAttributes, ref SECURITY_ATTRIBUTES lpThreadAttributes, bool bInheritHandles, ProcessCreationFlags dwCreationFlags, IntPtr lpEnvironment, string? lpCurrentDirectory, ref STARTUPINFOEX lpStartupInfo, ref PROCESS_INFORMATION lpProcessInformation);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	static extern bool GenerateConsoleCtrlEvent(ConsoleCtrlEvent dwCtrlEvent, int dwProcessGroupId);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	static extern bool AttachConsole(int dwProcessId);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	static extern IntPtr GetStdHandle(StandardHandles nStdHandle);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	static extern bool WriteConsoleInputW(IntPtr hConsoleInput, ref INPUT_RECORD__KEY lpBuffer, int nLength, out int lpNumberOfEventsWritten);
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	static extern bool FreeConsole();
	[SupportedOSPlatform(PlatformNames.Windows), DllImport("kernel32", SetLastError = true)]
	static extern void CloseHandle(IntPtr hObject);

	[StructLayout(LayoutKind.Sequential)]
	struct COORD
	{
		public short X; // Column (horizontal)
		public short Y; // Row (vertical)
	}

	const int ERROR_INSUFFICIENT_BUFFER = 122;

	[StructLayout(LayoutKind.Sequential)]
	struct STARTUPINFO
	{
		public static readonly int Size = Marshal.SizeOf<STARTUPINFO>();

		public int cb;
		public readonly string? lpReserved = null;
		public string? lpDesktop;
		public string? lpTitle;
		public int dwX;
		public int dwY;
		public int dwXSize;
		public int dwYSize;
		public int dwXCountChars;
		public int dwYCountChars;
		public ConsoleAttributes dwFillAttribute;
		public StartFlags dwFlags;
		public short wShowWindow;
		public readonly short cbReserved2;
		public readonly IntPtr lpReserved2;
		public IntPtr hStdInput;
		public IntPtr hStdOutput;
		public IntPtr hStdError;

		public STARTUPINFO()
		{
			cb = Size;
		}
	}

	[Flags]
	enum ConsoleAttributes : int
	{
		ForegroundBlack = 0x00,
		ForegroundBlue = 0x01,
		ForegroundGreen = 0x02,
		ForegroundRed = 0x04,
		ForegroundIntensity = 0x08,

		ForegroundCyan = ForegroundBlue | ForegroundGreen,
		ForegroundYellow = ForegroundRed | ForegroundGreen,
		ForegroundMagenta = ForegroundRed | ForegroundBlue,
		ForegroundWhite = ForegroundBlue | ForegroundGreen | ForegroundRed,

		BackgroundBlack = 0x00,
		BackgroundBlue = 0x10,
		BackgroundGreen = 0x20,
		BackgroundRed = 0x40,
		BackgroundIntensity = 0x80,

		BackgroundCyan = BackgroundBlue | BackgroundGreen,
		BackgroundYellow = BackgroundRed | BackgroundGreen,
		BackgroundMagenta = BackgroundRed | BackgroundBlue,
		BackgroundWhite = BackgroundBlue | BackgroundGreen | BackgroundRed,
	}

	[Flags]
	enum StartFlags : int
	{
		UseShowWindow = 0x0001,
		UseSize = 0x0002,
		UsePosition = 0x0004,
		UseCountChars = 0x0008,
		UseFillAttribute = 0x0010,
		RunFullScreen = 0x0020,
		ForceOnFeedback = 0x0040,
		ForceOffFeedback = 0x0080,
		UseStdHandles = 0x0100,
		UseHotKey = 0x0200,
		TitleIsLinkName = 0x0800,
		TitleIsAppID = 0x1000,
		PreventPinning = 0x2000,
		UntrustedSource = 0x8000,
	}

	[StructLayout(LayoutKind.Sequential)]
	struct STARTUPINFOEX
	{
		public static readonly int ExtendedSize = Marshal.SizeOf<STARTUPINFOEX>();

		public STARTUPINFO StartupInfo;
		public IntPtr lpAttributeList;

		public STARTUPINFOEX()
		{
			StartupInfo.cb = ExtendedSize;
		}
	}

	class ProcThreadAttributeMacroValue
	{
		public const UIntPtr PROC_THREAD_ATTRIBUTE_PARENT_PROCESS = 0x00020000;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_HANDLE_LIST = 0x00020002;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_GROUP_AFFINITY = 0x00030003;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_PREFERRED_NODE = 0x00020004;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_IDEAL_PROCESSOR = 0x00030005;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_UMS_THREAD = 0x00030006;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_MITIGATION_POLICY = 0x00020007;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_SECURITY_CAPABILITIES = 0x00020009;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_PROTECTION_LEVEL = 0x0002000B;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_PSEUDOCONSOLE = 0x00020016;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_MACHINE_TYPE = 0x00020019;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_ENABLE_OPTIONAL_XSTATE_FEATURES = 0x0003001B;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_SVE_VECTOR_LENGTH = 0x0002001E;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_JOB_LIST = 0x0002000D;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_CHILD_PROCESS_POLICY = 0x0002000E;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_ALL_APPLICATION_PACKAGES_POLICY = 0x0002000F;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_WIN32K_FILTER = 0x00020010;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_DESKTOP_APP_POLICY = 0x00020012;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_MITIGATION_AUDIT_POLICY = 0x00020018;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_COMPONENT_FILTER = 0x0002001A;
		public const UIntPtr PROC_THREAD_ATTRIBUTE_TRUSTED_APP = 0x0002001D;
	}

	[StructLayout(LayoutKind.Sequential)]
	struct SECURITY_ATTRIBUTES
	{
		public int nLength;
		public IntPtr lpSecurityDescriptor;
		public int bInheritHandle;
	}

	enum ProcessCreationFlags
	{
		DebugProcess = 0x00000001,
		DebugOnlyThisProcess = 0x00000002,
		Suspended = 0x00000004,
		DetachedProcess = 0x00000008,
		NewConsole = 0x00000010,
		NewProcessGroup = 0x00000200,
		UnicodeEnvironment = 0x00000400,
		SeparateWOWVDM = 0x00000800,
		SharedWOWVDM = 0x00001000,
		InheritParentAffinity = 0x00010000,
		ProtectedProcess = 0x00040000,
		ExtendedStartupInfoPresent = 0x00080000,
		SecureProcess = 0x00400000,
		BreakAwayFromJob = 0x01000000,
		PreserveCodeAuthorizationLevel = 0x02000000,
		DefaultErrorMode = 0x04000000,
		NoWindow = 0x08000000,
	}

	[StructLayout(LayoutKind.Sequential)]
	struct PROCESS_INFORMATION
	{
		public IntPtr hProcess;
		public IntPtr hThread;
		public int dwProcessId;
		public int dwThreadId;
	}

	enum ConsoleCtrlEvent
	{
		ControlCEvent = 0,
		ControlBreakEvent = 1,
	}

	enum StandardHandles
	{
		STD_INPUT_HANDLE = -10,
		STD_OUTPUT_HANDLE = -11,
		STD_ERROR_HANDLE = -12,
	}

	static readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);

	[StructLayout(LayoutKind.Sequential)]
	struct INPUT_RECORD__KEY
	{
		public readonly InputEventTypes EventType = InputEventTypes.KEY_EVENT;
		public KEY_INPUT_RECORD KeyEvent;

		public INPUT_RECORD__KEY() { }
	}

	enum InputEventTypes
	{
		KEY_EVENT = 0x0001,
		MOUSE_EVENT = 0x0002,
		WINDOW_BUFFER_SIZE_EVENT = 0x0004,
		MENU_EVENT = 0x0008,
		FOCUS_EVENT = 0x0010,
	}

	[StructLayout(LayoutKind.Sequential)]
	struct KEY_INPUT_RECORD
	{
		public bool bKeyDown;
		public short wRepeatCount;
		public short wVirtualKeyCode;
		public short wVirtualScanCode;
		public char UnicodeChar;
		public KeyInputRecordModifiers dwControlKeyState;
	}

	enum KeyInputRecordModifiers
	{
		RIGHT_ALT_PRESSED = 0x0001,
		LEFT_ALT_PRESSED = 0x0002,
		RIGHT_CTRL_PRESSED = 0x0004,
		LEFT_CTRL_PRESSED = 0x0008,
		SHIFT_PRESSED = 0x0010,
		NUMLOCK_ON = 0x0020,
		SCROLLLOCK_ON = 0x0040,
		CAPSLOCK_ON = 0x0080,
		ENHANCED_KEY = 0x0100,
	}

	enum VirtualKeyCodes : short
	{
		VK_LBUTTON        = 0x01,
		VK_RBUTTON        = 0x02,
		VK_CANCEL         = 0x03,
		VK_MBUTTON        = 0x04,    /* NOT contiguous with L & RBUTTON */
		VK_XBUTTON1       = 0x05,    /* NOT contiguous with L & RBUTTON */
		VK_XBUTTON2       = 0x06,    /* NOT contiguous with L & RBUTTON */
		// 0x07 : reserved
		VK_BACK           = 0x08,
		VK_TAB            = 0x09,
		// 0x0A - 0x0B : reserved
		VK_CLEAR          = 0x0C,
		VK_RETURN         = 0x0D,
// 0x0E - 0x0F : unassigned
		VK_SHIFT          = 0x10,
		VK_CONTROL        = 0x11,
		VK_MENU           = 0x12,
		VK_PAUSE          = 0x13,
		VK_CAPITAL        = 0x14,
		VK_KANA           = 0x15,
		VK_IME_ON         = 0x16,
		VK_JUNJA          = 0x17,
		VK_FINAL          = 0x18,
		VK_HANJA          = 0x19,
		VK_KANJI          = 0x19,
		VK_IME_OFF        = 0x1A,
		VK_ESCAPE         = 0x1B,
		VK_CONVERT        = 0x1C,
		VK_NONCONVERT     = 0x1D,
		VK_ACCEPT         = 0x1E,
		VK_MODECHANGE     = 0x1F,
		VK_SPACE          = 0x20,
		VK_PRIOR          = 0x21,
		VK_NEXT           = 0x22,
		VK_END            = 0x23,
		VK_HOME           = 0x24,
		VK_LEFT           = 0x25,
		VK_UP             = 0x26,
		VK_RIGHT          = 0x27,
		VK_DOWN           = 0x28,
		VK_SELECT         = 0x29,
		VK_PRINT          = 0x2A,
		VK_EXECUTE        = 0x2B,
		VK_SNAPSHOT       = 0x2C,
		VK_INSERT         = 0x2D,
		VK_DELETE         = 0x2E,
		VK_HELP           = 0x2F,
		VK_0              = 0x30,
		VK_1              = 0x31,
		VK_2              = 0x32,
		VK_3              = 0x33,
		VK_4              = 0x34,
		VK_5              = 0x35,
		VK_6              = 0x36,
		VK_7              = 0x37,
		VK_8              = 0x38,
		VK_9              = 0x39,
		// 0x3A - 0x40 : unassigned
		VK_A              = 0x41,
		VK_B              = 0x42,
		VK_C              = 0x43,
		VK_D              = 0x44,
		VK_E              = 0x45,
		VK_F              = 0x46,
		VK_G              = 0x47,
		VK_H              = 0x48,
		VK_I              = 0x49,
		VK_J              = 0x4A,
		VK_K              = 0x4B,
		VK_L              = 0x4C,
		VK_M              = 0x4D,
		VK_N              = 0x4E,
		VK_O              = 0x4F,
		VK_P              = 0x50,
		VK_Q              = 0x51,
		VK_R              = 0x52,
		VK_S              = 0x53,
		VK_T              = 0x54,
		VK_U              = 0x55,
		VK_V              = 0x56,
		VK_W              = 0x57,
		VK_X              = 0x58,
		VK_Y              = 0x59,
		VK_Z              = 0x5A,
		VK_LWIN           = 0x5B,
		VK_RWIN           = 0x5C,
		VK_APPS           = 0x5D,
		// 0x5E : reserved
		VK_SLEEP          = 0x5F,
		VK_NUMPAD0        = 0x60,
		VK_NUMPAD1        = 0x61,
		VK_NUMPAD2        = 0x62,
		VK_NUMPAD3        = 0x63,
		VK_NUMPAD4        = 0x64,
		VK_NUMPAD5        = 0x65,
		VK_NUMPAD6        = 0x66,
		VK_NUMPAD7        = 0x67,
		VK_NUMPAD8        = 0x68,
		VK_NUMPAD9        = 0x69,
		VK_MULTIPLY       = 0x6A,
		VK_ADD            = 0x6B,
		VK_SEPARATOR      = 0x6C,
		VK_SUBTRACT       = 0x6D,
		VK_DECIMAL        = 0x6E,
		VK_DIVIDE         = 0x6F,
		VK_F1             = 0x70,
		VK_F2             = 0x71,
		VK_F3             = 0x72,
		VK_F4             = 0x73,
		VK_F5             = 0x74,
		VK_F6             = 0x75,
		VK_F7             = 0x76,
		VK_F8             = 0x77,
		VK_F9             = 0x78,
		VK_F10            = 0x79,
		VK_F11            = 0x7A,
		VK_F12            = 0x7B,
		VK_F13            = 0x7C,
		VK_F14            = 0x7D,
		VK_F15            = 0x7E,
		VK_F16            = 0x7F,
		VK_F17            = 0x80,
		VK_F18            = 0x81,
		VK_F19            = 0x82,
		VK_F20            = 0x83,
		VK_F21            = 0x84,
		VK_F22            = 0x85,
		VK_F23            = 0x86,
		VK_F24            = 0x87,
		VK_NAVIGATION_VIEW     = 0x88, // reserved
		VK_NAVIGATION_MENU     = 0x89, // reserved
		VK_NAVIGATION_UP       = 0x8A, // reserved
		VK_NAVIGATION_DOWN     = 0x8B, // reserved
		VK_NAVIGATION_LEFT     = 0x8C, // reserved
		VK_NAVIGATION_RIGHT    = 0x8D, // reserved
		VK_NAVIGATION_ACCEPT   = 0x8E, // reserved
		VK_NAVIGATION_CANCEL   = 0x8F, // reserved
		VK_NUMLOCK        = 0x90,
		VK_SCROLL         = 0x91,
		// NEC PC-9800 kbd definitions
		VK_OEM_NEC_EQUAL  = 0x92,   // '=' key on numpad
		// Fujitsu/OASYS kbd definitions
		VK_OEM_FJ_JISHO   = 0x92,   // 'Dictionary' key
		VK_OEM_FJ_MASSHOU = 0x93,   // 'Unregister word' key
		VK_OEM_FJ_TOUROKU = 0x94,   // 'Register word' key
		VK_OEM_FJ_LOYA    = 0x95,   // 'Left OYAYUBI' key
		VK_OEM_FJ_ROYA    = 0x96,   // 'Right OYAYUBI' key
		// 0x97 - 0x9F : unassigned

		/*
		 * VK_L* & VK_R* - left and right Alt, Ctrl and Shift virtual keys.
		 * Used only as parameters to GetAsyncKeyState() and GetKeyState().
		 * No other API or message will distinguish left and right keys in this way.
		 */
		VK_LSHIFT         = 0xA0,
		VK_RSHIFT         = 0xA1,
		VK_LCONTROL       = 0xA2,
		VK_RCONTROL       = 0xA3,
		VK_LMENU          = 0xA4,
		VK_RMENU          = 0xA5,

		VK_BROWSER_BACK        = 0xA6,
		VK_BROWSER_FORWARD     = 0xA7,
		VK_BROWSER_REFRESH     = 0xA8,
		VK_BROWSER_STOP        = 0xA9,
		VK_BROWSER_SEARCH      = 0xAA,
		VK_BROWSER_FAVORITES   = 0xAB,
		VK_BROWSER_HOME        = 0xAC,

		VK_VOLUME_MUTE         = 0xAD,
		VK_VOLUME_DOWN         = 0xAE,
		VK_VOLUME_UP           = 0xAF,
		VK_MEDIA_NEXT_TRACK    = 0xB0,
		VK_MEDIA_PREV_TRACK    = 0xB1,
		VK_MEDIA_STOP          = 0xB2,
		VK_MEDIA_PLAY_PAUSE    = 0xB3,
		VK_LAUNCH_MAIL         = 0xB4,
		VK_LAUNCH_MEDIA_SELECT = 0xB5,
		VK_LAUNCH_APP1         = 0xB6,
		VK_LAUNCH_APP2         = 0xB7,
		// 0xB8 - 0xB9 : reserved
		VK_OEM_1          = 0xBA,   // ';:' for US
		VK_OEM_PLUS       = 0xBB,   // '+' any country
		VK_OEM_COMMA      = 0xBC,   // ',' any country
		VK_OEM_MINUS      = 0xBD,   // '-' any country
		VK_OEM_PERIOD     = 0xBE,   // '.' any country
		VK_OEM_2          = 0xBF,   // '/?' for US
		VK_OEM_3          = 0xC0,   // '`~' for US
		// 0xC1 - 0xC2 : reserved

		/*
		 * 0xC3 - 0xDA : Gamepad input
		 */
		VK_GAMEPAD_A                         = 0xC3, // reserved
		VK_GAMEPAD_B                         = 0xC4, // reserved
		VK_GAMEPAD_X                         = 0xC5, // reserved
		VK_GAMEPAD_Y                         = 0xC6, // reserved
		VK_GAMEPAD_RIGHT_SHOULDER            = 0xC7, // reserved
		VK_GAMEPAD_LEFT_SHOULDER             = 0xC8, // reserved
		VK_GAMEPAD_LEFT_TRIGGER              = 0xC9, // reserved
		VK_GAMEPAD_RIGHT_TRIGGER             = 0xCA, // reserved
		VK_GAMEPAD_DPAD_UP                   = 0xCB, // reserved
		VK_GAMEPAD_DPAD_DOWN                 = 0xCC, // reserved
		VK_GAMEPAD_DPAD_LEFT                 = 0xCD, // reserved
		VK_GAMEPAD_DPAD_RIGHT                = 0xCE, // reserved
		VK_GAMEPAD_MENU                      = 0xCF, // reserved
		VK_GAMEPAD_VIEW                      = 0xD0, // reserved
		VK_GAMEPAD_LEFT_THUMBSTICK_BUTTON    = 0xD1, // reserved
		VK_GAMEPAD_RIGHT_THUMBSTICK_BUTTON   = 0xD2, // reserved
		VK_GAMEPAD_LEFT_THUMBSTICK_UP        = 0xD3, // reserved
		VK_GAMEPAD_LEFT_THUMBSTICK_DOWN      = 0xD4, // reserved
		VK_GAMEPAD_LEFT_THUMBSTICK_RIGHT     = 0xD5, // reserved
		VK_GAMEPAD_LEFT_THUMBSTICK_LEFT      = 0xD6, // reserved
		VK_GAMEPAD_RIGHT_THUMBSTICK_UP       = 0xD7, // reserved
		VK_GAMEPAD_RIGHT_THUMBSTICK_DOWN     = 0xD8, // reserved
		VK_GAMEPAD_RIGHT_THUMBSTICK_RIGHT    = 0xD9, // reserved
		VK_GAMEPAD_RIGHT_THUMBSTICK_LEFT     = 0xDA, // reserved

		VK_OEM_4          = 0xDB,  //  '[{' for US
		VK_OEM_5          = 0xDC,  //  '\|' for US
		VK_OEM_6          = 0xDD,  //  ']}' for US
		VK_OEM_7          = 0xDE,  //  ''"' for US
		VK_OEM_8          = 0xDF,

		// 0xE0 : reserved

		/*
		 * Various extended or enhanced keyboards
		 */
		VK_OEM_AX         = 0xE1,  //  'AX' key on Japanese AX kbd
		VK_OEM_102        = 0xE2,  //  "<>" or "\|" on RT 102-key kbd.
		VK_ICO_HELP       = 0xE3,  //  Help key on ICO
		VK_ICO_00         = 0xE4,  //  00 key on ICO
		VK_PROCESSKEY     = 0xE5,
		VK_ICO_CLEAR      = 0xE6,
		VK_PACKET         = 0xE7,
		// 0xE8 : unassigned

		/*
		 * Nokia/Ericsson definitions
		 */
		VK_OEM_RESET      = 0xE9,
		VK_OEM_JUMP       = 0xEA,
		VK_OEM_PA1        = 0xEB,
		VK_OEM_PA2        = 0xEC,
		VK_OEM_PA3        = 0xED,
		VK_OEM_WSCTRL     = 0xEE,
		VK_OEM_CUSEL      = 0xEF,
		VK_OEM_ATTN       = 0xF0,
		VK_OEM_FINISH     = 0xF1,
		VK_OEM_COPY       = 0xF2,
		VK_OEM_AUTO       = 0xF3,
		VK_OEM_ENLW       = 0xF4,
		VK_OEM_BACKTAB    = 0xF5,

		VK_ATTN           = 0xF6,
		VK_CRSEL          = 0xF7,
		VK_EXSEL          = 0xF8,
		VK_EREOF          = 0xF9,
		VK_PLAY           = 0xFA,
		VK_ZOOM           = 0xFB,
		VK_NONAME         = 0xFC,
		VK_PA1            = 0xFD,
		VK_OEM_CLEAR      = 0xFE,
	}

	static VirtualKeyCodes GetVirtualKeyCode(SDL3.SDL.Scancode sdlScanCode)
	{
		switch (sdlScanCode)
		{
			case SDL3.SDL.Scancode.A: return VirtualKeyCodes.VK_A;
			case SDL3.SDL.Scancode.B: return VirtualKeyCodes.VK_B;
			case SDL3.SDL.Scancode.C: return VirtualKeyCodes.VK_C;
			case SDL3.SDL.Scancode.D: return VirtualKeyCodes.VK_D;
			case SDL3.SDL.Scancode.E: return VirtualKeyCodes.VK_E;
			case SDL3.SDL.Scancode.F: return VirtualKeyCodes.VK_F;
			case SDL3.SDL.Scancode.G: return VirtualKeyCodes.VK_G;
			case SDL3.SDL.Scancode.H: return VirtualKeyCodes.VK_H;
			case SDL3.SDL.Scancode.I: return VirtualKeyCodes.VK_I;
			case SDL3.SDL.Scancode.J: return VirtualKeyCodes.VK_J;
			case SDL3.SDL.Scancode.K: return VirtualKeyCodes.VK_K;
			case SDL3.SDL.Scancode.L: return VirtualKeyCodes.VK_L;
			case SDL3.SDL.Scancode.M: return VirtualKeyCodes.VK_M;
			case SDL3.SDL.Scancode.N: return VirtualKeyCodes.VK_N;
			case SDL3.SDL.Scancode.O: return VirtualKeyCodes.VK_O;
			case SDL3.SDL.Scancode.P: return VirtualKeyCodes.VK_P;
			case SDL3.SDL.Scancode.Q: return VirtualKeyCodes.VK_Q;
			case SDL3.SDL.Scancode.R: return VirtualKeyCodes.VK_R;
			case SDL3.SDL.Scancode.S: return VirtualKeyCodes.VK_S;
			case SDL3.SDL.Scancode.T: return VirtualKeyCodes.VK_T;
			case SDL3.SDL.Scancode.U: return VirtualKeyCodes.VK_U;
			case SDL3.SDL.Scancode.V: return VirtualKeyCodes.VK_V;
			case SDL3.SDL.Scancode.W: return VirtualKeyCodes.VK_W;
			case SDL3.SDL.Scancode.X: return VirtualKeyCodes.VK_X;
			case SDL3.SDL.Scancode.Y: return VirtualKeyCodes.VK_Y;
			case SDL3.SDL.Scancode.Z: return VirtualKeyCodes.VK_Z;
			case SDL3.SDL.Scancode.Alpha1: return VirtualKeyCodes.VK_1;
			case SDL3.SDL.Scancode.Alpha2: return VirtualKeyCodes.VK_2;
			case SDL3.SDL.Scancode.Alpha3: return VirtualKeyCodes.VK_3;
			case SDL3.SDL.Scancode.Alpha4: return VirtualKeyCodes.VK_4;
			case SDL3.SDL.Scancode.Alpha5: return VirtualKeyCodes.VK_5;
			case SDL3.SDL.Scancode.Alpha6: return VirtualKeyCodes.VK_6;
			case SDL3.SDL.Scancode.Alpha7: return VirtualKeyCodes.VK_7;
			case SDL3.SDL.Scancode.Alpha8: return VirtualKeyCodes.VK_8;
			case SDL3.SDL.Scancode.Alpha9: return VirtualKeyCodes.VK_9;
			case SDL3.SDL.Scancode.Alpha0: return VirtualKeyCodes.VK_0;
			case SDL3.SDL.Scancode.Return: return VirtualKeyCodes.VK_RETURN;
			case SDL3.SDL.Scancode.Escape: return VirtualKeyCodes.VK_ESCAPE;
			case SDL3.SDL.Scancode.Backspace: return VirtualKeyCodes.VK_BACK;
			case SDL3.SDL.Scancode.Tab: return VirtualKeyCodes.VK_TAB;
			case SDL3.SDL.Scancode.Space: return VirtualKeyCodes.VK_SPACE;
			case SDL3.SDL.Scancode.Minus: return VirtualKeyCodes.VK_OEM_MINUS;
			case SDL3.SDL.Scancode.Equals: return VirtualKeyCodes.VK_OEM_PLUS;
			case SDL3.SDL.Scancode.Leftbracket: return VirtualKeyCodes.VK_OEM_4;
			case SDL3.SDL.Scancode.Rightbracket: return VirtualKeyCodes.VK_OEM_6;
			case SDL3.SDL.Scancode.Backslash: return VirtualKeyCodes.VK_OEM_5;
			case SDL3.SDL.Scancode.NonUsHash: return VirtualKeyCodes.VK_OEM_5;
			case SDL3.SDL.Scancode.Semicolon: return VirtualKeyCodes.VK_OEM_1;
			case SDL3.SDL.Scancode.Apostrophe: return VirtualKeyCodes.VK_OEM_7;
			case SDL3.SDL.Scancode.Grave: return VirtualKeyCodes.VK_OEM_3;
			case SDL3.SDL.Scancode.Comma: return VirtualKeyCodes.VK_OEM_COMMA;
			case SDL3.SDL.Scancode.Period: return VirtualKeyCodes.VK_OEM_PERIOD;
			case SDL3.SDL.Scancode.Slash: return VirtualKeyCodes.VK_OEM_2;
			case SDL3.SDL.Scancode.Capslock: return VirtualKeyCodes.VK_CAPITAL;
			case SDL3.SDL.Scancode.F1: return VirtualKeyCodes.VK_F1;
			case SDL3.SDL.Scancode.F2: return VirtualKeyCodes.VK_F2;
			case SDL3.SDL.Scancode.F3: return VirtualKeyCodes.VK_F3;
			case SDL3.SDL.Scancode.F4: return VirtualKeyCodes.VK_F4;
			case SDL3.SDL.Scancode.F5: return VirtualKeyCodes.VK_F5;
			case SDL3.SDL.Scancode.F6: return VirtualKeyCodes.VK_F6;
			case SDL3.SDL.Scancode.F7: return VirtualKeyCodes.VK_F7;
			case SDL3.SDL.Scancode.F8: return VirtualKeyCodes.VK_F8;
			case SDL3.SDL.Scancode.F9: return VirtualKeyCodes.VK_F9;
			case SDL3.SDL.Scancode.F10: return VirtualKeyCodes.VK_F10;
			case SDL3.SDL.Scancode.F11: return VirtualKeyCodes.VK_F11;
			case SDL3.SDL.Scancode.F12: return VirtualKeyCodes.VK_F12;
			case SDL3.SDL.Scancode.Printscreen: return VirtualKeyCodes.VK_PRINT;
			case SDL3.SDL.Scancode.Scrolllock: return VirtualKeyCodes.VK_SCROLL;
			case SDL3.SDL.Scancode.Pause: return VirtualKeyCodes.VK_PAUSE;
			case SDL3.SDL.Scancode.Insert: return VirtualKeyCodes.VK_INSERT;
			case SDL3.SDL.Scancode.Home: return VirtualKeyCodes.VK_HOME;
			case SDL3.SDL.Scancode.Pageup: return VirtualKeyCodes.VK_PRIOR;
			case SDL3.SDL.Scancode.Delete: return VirtualKeyCodes.VK_DELETE;
			case SDL3.SDL.Scancode.End: return VirtualKeyCodes.VK_END;
			case SDL3.SDL.Scancode.Pagedown: return VirtualKeyCodes.VK_NEXT;
			case SDL3.SDL.Scancode.Right: return VirtualKeyCodes.VK_RIGHT;
			case SDL3.SDL.Scancode.Left: return VirtualKeyCodes.VK_LEFT;
			case SDL3.SDL.Scancode.Down: return VirtualKeyCodes.VK_DOWN;
			case SDL3.SDL.Scancode.Up: return VirtualKeyCodes.VK_UP;
			case SDL3.SDL.Scancode.NumLockClear: return VirtualKeyCodes.VK_NUMLOCK;
			case SDL3.SDL.Scancode.KpDivide: return VirtualKeyCodes.VK_DIVIDE;
			case SDL3.SDL.Scancode.KpMultiply: return VirtualKeyCodes.VK_MULTIPLY;
			case SDL3.SDL.Scancode.KpMinus: return VirtualKeyCodes.VK_SUBTRACT;
			case SDL3.SDL.Scancode.KpPlus: return VirtualKeyCodes.VK_ADD;
			case SDL3.SDL.Scancode.KpEnter: return VirtualKeyCodes.VK_RETURN;
			case SDL3.SDL.Scancode.Kp1: return VirtualKeyCodes.VK_NUMPAD1;
			case SDL3.SDL.Scancode.Kp2: return VirtualKeyCodes.VK_NUMPAD2;
			case SDL3.SDL.Scancode.Kp3: return VirtualKeyCodes.VK_NUMPAD3;
			case SDL3.SDL.Scancode.Kp4: return VirtualKeyCodes.VK_NUMPAD4;
			case SDL3.SDL.Scancode.Kp5: return VirtualKeyCodes.VK_NUMPAD5;
			case SDL3.SDL.Scancode.Kp6: return VirtualKeyCodes.VK_NUMPAD6;
			case SDL3.SDL.Scancode.Kp7: return VirtualKeyCodes.VK_NUMPAD7;
			case SDL3.SDL.Scancode.Kp8: return VirtualKeyCodes.VK_NUMPAD8;
			case SDL3.SDL.Scancode.Kp9: return VirtualKeyCodes.VK_NUMPAD9;
			case SDL3.SDL.Scancode.Kp0: return VirtualKeyCodes.VK_NUMPAD0;
			case SDL3.SDL.Scancode.KpPercent: return VirtualKeyCodes.VK_DECIMAL;
			case SDL3.SDL.Scancode.NonUsBackSlash: return VirtualKeyCodes.VK_OEM_3;
			case SDL3.SDL.Scancode.Application: return VirtualKeyCodes.VK_APPS;
			case SDL3.SDL.Scancode.KpEquals: return VirtualKeyCodes.VK_OEM_PLUS;
			case SDL3.SDL.Scancode.F13: return VirtualKeyCodes.VK_F13;
			case SDL3.SDL.Scancode.F14: return VirtualKeyCodes.VK_F14;
			case SDL3.SDL.Scancode.F15: return VirtualKeyCodes.VK_F15;
			case SDL3.SDL.Scancode.F16: return VirtualKeyCodes.VK_F16;
			case SDL3.SDL.Scancode.F17: return VirtualKeyCodes.VK_F17;
			case SDL3.SDL.Scancode.F18: return VirtualKeyCodes.VK_F18;
			case SDL3.SDL.Scancode.F19: return VirtualKeyCodes.VK_F19;
			case SDL3.SDL.Scancode.F20: return VirtualKeyCodes.VK_F20;
			case SDL3.SDL.Scancode.F21: return VirtualKeyCodes.VK_F21;
			case SDL3.SDL.Scancode.F22: return VirtualKeyCodes.VK_F22;
			case SDL3.SDL.Scancode.F23: return VirtualKeyCodes.VK_F23;
			case SDL3.SDL.Scancode.F24: return VirtualKeyCodes.VK_F24;
			case SDL3.SDL.Scancode.Execute: return VirtualKeyCodes.VK_EXECUTE;
			case SDL3.SDL.Scancode.Help: return VirtualKeyCodes.VK_HELP;
			case SDL3.SDL.Scancode.Select: return VirtualKeyCodes.VK_SELECT;
			case SDL3.SDL.Scancode.Copy: return VirtualKeyCodes.VK_OEM_COPY;
			case SDL3.SDL.Scancode.Mute: return VirtualKeyCodes.VK_VOLUME_MUTE;
			case SDL3.SDL.Scancode.VolumeUp: return VirtualKeyCodes.VK_VOLUME_UP;
			case SDL3.SDL.Scancode.VolumeDown: return VirtualKeyCodes.VK_VOLUME_DOWN;
			case SDL3.SDL.Scancode.KpComma: return VirtualKeyCodes.VK_OEM_COMMA;
			case SDL3.SDL.Scancode.KpEqualsAs400: return VirtualKeyCodes.VK_OEM_PLUS;
			case SDL3.SDL.Scancode.Lang1: return VirtualKeyCodes.VK_KANA; // formerly named VK_HANGUL
			case SDL3.SDL.Scancode.Lang2: return VirtualKeyCodes.VK_HANJA;
			case SDL3.SDL.Scancode.Lang3: return VirtualKeyCodes.VK_KANA;
			case SDL3.SDL.Scancode.Lang4: return VirtualKeyCodes.VK_KANA;
			case SDL3.SDL.Scancode.Cancel: return VirtualKeyCodes.VK_CANCEL;
			case SDL3.SDL.Scancode.Clear: return VirtualKeyCodes.VK_CLEAR;
			case SDL3.SDL.Scancode.Prior: return VirtualKeyCodes.VK_PRIOR;
			case SDL3.SDL.Scancode.Return2: return VirtualKeyCodes.VK_RETURN;
			case SDL3.SDL.Scancode.Separator: return VirtualKeyCodes.VK_SEPARATOR;
			case SDL3.SDL.Scancode.DecimalSeparator: return VirtualKeyCodes.VK_DECIMAL;
			case SDL3.SDL.Scancode.KpTab: return VirtualKeyCodes.VK_TAB;
			case SDL3.SDL.Scancode.KpBackspace: return VirtualKeyCodes.VK_BACK;
			case SDL3.SDL.Scancode.KpA: return VirtualKeyCodes.VK_A;
			case SDL3.SDL.Scancode.KpB: return VirtualKeyCodes.VK_B;
			case SDL3.SDL.Scancode.KpC: return VirtualKeyCodes.VK_C;
			case SDL3.SDL.Scancode.KpD: return VirtualKeyCodes.VK_D;
			case SDL3.SDL.Scancode.KpE: return VirtualKeyCodes.VK_E;
			case SDL3.SDL.Scancode.KpF: return VirtualKeyCodes.VK_F;
			case SDL3.SDL.Scancode.KpSpace: return VirtualKeyCodes.VK_SPACE;
			case SDL3.SDL.Scancode.KpClear: return VirtualKeyCodes.VK_CLEAR;
			case SDL3.SDL.Scancode.LCtrl: return VirtualKeyCodes.VK_CONTROL;
			case SDL3.SDL.Scancode.LShift: return VirtualKeyCodes.VK_SHIFT;
			case SDL3.SDL.Scancode.LAlt: return VirtualKeyCodes.VK_MENU;
			case SDL3.SDL.Scancode.LGUI: return VirtualKeyCodes.VK_LWIN;
			case SDL3.SDL.Scancode.RCtrl: return VirtualKeyCodes.VK_CONTROL;
			case SDL3.SDL.Scancode.RShift: return VirtualKeyCodes.VK_SHIFT;
			case SDL3.SDL.Scancode.RAlt: return VirtualKeyCodes.VK_RMENU;
			case SDL3.SDL.Scancode.RGUI: return VirtualKeyCodes.VK_RWIN;
			case SDL3.SDL.Scancode.Sleep: return VirtualKeyCodes.VK_SLEEP;
		}

		return 0;
	}

	static KeyInputRecordModifiers TranslateModifiers(KeyModifiers modifiers)
	{
		var ret = default(KeyInputRecordModifiers);

		if (modifiers.CtrlKey)
			ret |= KeyInputRecordModifiers.LEFT_CTRL_PRESSED;
		if (modifiers.ShiftKey)
			ret |= KeyInputRecordModifiers.SHIFT_PRESSED;
		if (modifiers.AltKey)
			ret |= KeyInputRecordModifiers.LEFT_ALT_PRESSED;
		if (modifiers.CapsLock)
			ret |= KeyInputRecordModifiers.CAPSLOCK_ON;
		if (modifiers.NumLock)
			ret |= KeyInputRecordModifiers.NUMLOCK_ON;

		return ret;
	}
}
