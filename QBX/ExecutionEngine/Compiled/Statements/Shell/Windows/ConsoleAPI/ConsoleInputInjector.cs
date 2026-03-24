using System;
using System.ComponentModel;
using System.Runtime.Versioning;

using QBX.Hardware;
using QBX.Platform.Windows;
using QBX.Terminal;
using QBX.Utility;

using static QBX.Platform.Windows.NativeMethods;

namespace QBX.ExecutionEngine.Compiled.Statements.Shell.Windows.ConsoleAPI;

[SupportedOSPlatform(PlatformNames.Windows)]
class ConsoleInputInjector : InputInjector
{
	public ConsoleInputInjector(int processID)
	{
		_processID = processID;

		_hStdIn = GetStdHandle(StandardHandles.STD_INPUT_HANDLE);

		if (_hStdIn == INVALID_HANDLE_VALUE)
			throw new Win32Exception();
	}

	IntPtr _hStdIn;
	int _processID;

	public override void Inject(KeyEvent evt)
	{
		if ((evt.Modifiers.CtrlKey) && (evt.ScanCode != ScanCode.Control))
			System.Diagnostics.Debugger.Break();

		bool isCtrlKey = evt.Modifiers.CtrlKey && !evt.Modifiers.AltKey;

		if (isCtrlKey && (evt.ScanCode == ScanCode.C))
			GenerateConsoleCtrlEvent(ConsoleCtrlEvent.CTRL_C_EVENT, 0);
		else if (isCtrlKey && (evt.SDLScanCode == SDL3.SDL.Scancode.Pause))
			GenerateConsoleCtrlEvent(ConsoleCtrlEvent.CTRL_BREAK_EVENT, 0);
		else
		{
			var record = new INPUT_RECORD__KEY();

			Win32TerminalInput.TranslateKeyEvent(
				evt,
				out record.KeyEvent);

			bool success = WriteConsoleInputW(_hStdIn, ref record, 1, out _);

			if (!success)
				throw new Win32Exception();
		}
	}

	public override void Dispose()
	{
		FreeConsole();
	}
}
