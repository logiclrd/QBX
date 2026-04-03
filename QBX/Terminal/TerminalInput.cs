using System;
using System.IO;

using QBX.Firmware.Fonts;
using QBX.Hardware;

namespace QBX.Terminal;

/*
public enum TerminalInputEncoding
{
	VT,
	WIN32IM,
	Kitty,
}
*/

public class TerminalInput(Stream pipe, Machine machine) : InputInjector
{
	public TerminalInputMode Mode = TerminalInputMode.Normal;
	//public TerminalInputEncoding Encoding;
	public bool BackarrowMode;

	[ThreadStatic]
	static byte[]? s_buffer;

	struct Encoding
	{
		public bool UseAltPrefix;
		public byte Datum;
		public int CharacterCode;
		public char ANSICommand;
	}

	public override void Inject(KeyEvent evt)
	{
		s_buffer ??= new byte[10];

		var enc = new Encoding();

		bool isModified = evt.Modifiers.Pack() != 0;

		switch (evt.ScanCode)
		{
			case ScanCode.Backspace:
				enc.UseAltPrefix = true;
				if (evt.Modifiers.CtrlKey != BackarrowMode)
					enc.Datum = 0x7F;
				else
					enc.Datum = 8;
				break;
			case ScanCode.Tab:
				enc.UseAltPrefix = true;
				if (evt.Modifiers.ShiftKey)
					enc.Datum = 9;
				else
					enc.ANSICommand = 'Z';
				break;
			case ScanCode.Return:
				enc.UseAltPrefix = true;
				if (!evt.Modifiers.CtrlKey)
					enc.Datum = 13;
				else
					enc.Datum = 10;
				break;
			case ScanCode.F1: enc.ANSICommand = 'P'; break;
			case ScanCode.F2: enc.ANSICommand = 'Q'; break;
			case ScanCode.F3: enc.ANSICommand = 'R'; break;
			case ScanCode.F4: enc.ANSICommand = 'S'; break;
			case ScanCode.F5: enc.CharacterCode = 15; enc.ANSICommand = '~'; break;
			case ScanCode.F6: enc.CharacterCode = 17; enc.ANSICommand = '~'; break;
			case ScanCode.F7: enc.CharacterCode = 18; enc.ANSICommand = '~'; break;
			case ScanCode.F8: enc.CharacterCode = 19; enc.ANSICommand = '~'; break;
			case ScanCode.F9: enc.CharacterCode = 20; enc.ANSICommand = '~'; break;
			case ScanCode.F10: enc.CharacterCode = 21; enc.ANSICommand = '~'; break;
			case ScanCode.F11: enc.CharacterCode = 23; enc.ANSICommand = '~'; break;
			case ScanCode.F12: enc.CharacterCode = 24; enc.ANSICommand = '~'; break;
			case ScanCode.Up: enc.ANSICommand = 'A'; break;
			case ScanCode.Down: enc.ANSICommand = 'B'; break;
			case ScanCode.Right: enc.ANSICommand = 'C'; break;
			case ScanCode.Left: enc.ANSICommand = 'D'; break;
			case ScanCode.End: enc.ANSICommand = 'F'; break;
			case ScanCode.Home: enc.ANSICommand = 'H'; break;
			case ScanCode.Insert: enc.CharacterCode = 2; enc.ANSICommand = '~'; break;
			case ScanCode.Delete: enc.CharacterCode = 3; enc.ANSICommand = '~'; break;
			case ScanCode.PageUp: enc.CharacterCode = 5; enc.ANSICommand = '~'; break;
			case ScanCode.PageDown: enc.CharacterCode = 6; enc.ANSICommand = '~'; break;

			default:
				if (evt.HasTextCharacter)
				{
					Span<byte> buf = stackalloc byte[1];

					buf[0] = CP437Encoding.GetByteSemantic(evt.TextCharacter);

					InjectInput(buf);
				}

				return;
		}

		if (enc.ANSICommand == 0)
		{
			Span<byte> buf = stackalloc byte[1];

			buf[0] = enc.Datum;

			InjectInput(buf);
		}
		else
		{
			Span<byte> buf = stackalloc byte[15];

			int i = 9;

			buf[i--] = CP437Encoding.GetByteSemantic(enc.ANSICommand);

			int n = enc.CharacterCode;

			do
			{
				buf[i--] = (byte)(48 + (n % 10)); // '0' through '9'
				n /= 10;
			} while (n > 0);

			buf[i--] = (byte)'[';
			buf[i] = 0x1B;

			InjectInput(buf.Slice(i));
		}
	}

	public void InjectInput(ReadOnlySpan<byte> bytes)
	{
		pipe.Write(bytes);
	}

	public void SetNumLockState(bool numLockOn)
	{
		machine.SystemMemory.KeyboardStatus.Byte0.NumLock = numLockOn;
		machine.SystemMemory.KeyboardStatus.Byte2.NumLockIndicator = numLockOn;
	}
}
