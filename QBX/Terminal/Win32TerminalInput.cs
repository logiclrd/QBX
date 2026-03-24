using System;
using System.IO;

using QBX.Firmware.Fonts;
using QBX.Hardware;
using QBX.Platform.Windows;
using QBX.Terminal.Platform.Windows;

namespace QBX.Terminal;

public class Win32TerminalInput(Stream pipe, Machine machine) : TerminalInput(pipe, machine)
{
	public override void Inject(KeyEvent evt)
	{
		static void Append(ref Span<byte> buf, byte b)
		{
			buf[0] = b;
			buf = buf.Slice(1);
		}

		static void AppendNumber(ref Span<byte> buf, int number)
		{
			int radix = 1;

			while (radix < number)
				radix *= 10;

			radix /= 10;

			while (radix > 0)
			{
				int digit = number / radix;

				Append(ref buf, (byte)(digit + '0'));

				number -= digit * radix;
				radix /= 10;
			}
		}

		static void AppendChar(ref Span<byte> buf, char ch)
		{
			Append(ref buf, CP437Encoding.GetByteSemantic(ch));
		}

		const byte ESC = 27;

		Span<byte> buffer = stackalloc byte[38];

		var bufPtr = buffer;

		TranslateKeyEvent(
			evt,
			out ushort vkey,
			out ushort scanCode,
			out ushort unicodeChar,
			out ushort keyDown,
			out ushort controlKeyState,
			out ushort repeatCount);

		Append(ref bufPtr, ESC);
		AppendChar(ref bufPtr, '[');
		AppendNumber(ref bufPtr, vkey);
		AppendChar(ref bufPtr, ';');
		AppendNumber(ref bufPtr, scanCode);
		AppendChar(ref bufPtr, ';');
		AppendNumber(ref bufPtr, unicodeChar);
		AppendChar(ref bufPtr, ';');
		AppendNumber(ref bufPtr, keyDown);
		AppendChar(ref bufPtr, ';');
		AppendNumber(ref bufPtr, controlKeyState);
		AppendChar(ref bufPtr, ';');
		AppendNumber(ref bufPtr, repeatCount);
		AppendChar(ref bufPtr, '_');

		int numBytes = buffer.Length - bufPtr.Length;

		InjectInput(buffer.Slice(0, numBytes));
	}

	public static void TranslateKeyEvent(KeyEvent evt, out KEY_INPUT_RECORD keyEvent)
	{
		TranslateKeyEvent(
			evt,
			out keyEvent.wVirtualKeyCode,
			out keyEvent.wVirtualScanCode,
			out keyEvent.UnicodeChar,
			out keyEvent.bKeyDown,
			out keyEvent.dwControlKeyState,
			out keyEvent.wRepeatCount);
	}

	public static void TranslateKeyEvent(
		KeyEvent evt,
		out short vkey,
		out short scanCode,
		out char unicodeChar,
		out bool keyDown,
		out KeyInputRecordModifiers controlKeyState,
		out short repeatCount)
	{
		vkey = (short)evt.SDLScanCode.ToVirtualKeyCode();
		scanCode = (short)evt.SDLScanCode;
		unicodeChar = evt.TextCharacter;
		keyDown = !evt.IsRelease;
		controlKeyState = evt.Modifiers.ToKeyInputRecordModifier();
		repeatCount = 1;
	}

	public static void TranslateKeyEvent(
		KeyEvent evt,
		out ushort vkey,
		out ushort scanCode,
		out ushort unicodeChar,
		out ushort keyDown,
		out ushort controlKeyState,
		out ushort repeatCount)
	{
		vkey = (ushort)evt.SDLScanCode.ToVirtualKeyCode();
		scanCode = (ushort)evt.SDLScanCode;
		unicodeChar = evt.TextCharacter;
		keyDown = (ushort)(evt.IsRelease ? 0 : 1);
		controlKeyState = (ushort)evt.Modifiers.ToKeyInputRecordModifier();
		repeatCount = 1;
	}
}
