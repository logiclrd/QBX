using System;
using System.ComponentModel;
using System.IO;
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
	public ConsoleInputInjector(Stream ptyStdinPipe)
	{
		_hStdIn = GetStdHandle(StandardHandles.STD_INPUT_HANDLE);

		if (_hStdIn == INVALID_HANDLE_VALUE)
			throw new Win32Exception();

		_ptyStdinPipe = ptyStdinPipe;
	}

	IntPtr _hStdIn;
	Stream _ptyStdinPipe;

	public override void Inject(KeyEvent evt)
	{
		bool isCtrlKey = evt.Modifiers.CtrlKey && !evt.Modifiers.AltKey;

		if (isCtrlKey && (evt.ScanCode == ScanCode.C))
			_ptyStdinPipe.WriteByte(3);
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
