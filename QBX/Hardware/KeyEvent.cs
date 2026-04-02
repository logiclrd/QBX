using System.Collections;
using System.IO;

using QBX.Firmware;
using QBX.Platform.Linux;

using SDL3;

namespace QBX.Hardware;

public class KeyEvent
{
	public char TextCharacter;
	public ScanCode ScanCode;
	public KeyModifiers Modifiers;
	public bool IsRight; // true for Ctrl, Alt, Shift when it is the right-hand key that is pressed
	public bool IsRelease;
	public bool IsKeyPad;
	public bool IsEphemeral; // true when INKEY$ doesn't return anything for this key

	public SDL.Scancode SDLScanCode;

	public bool IsEmpty => (TextCharacter == default) && (ScanCode == 0);
	public bool IsNormalText => (TextCharacter >= 32);
	public bool HasTextCharacter => (TextCharacter > 0);

	public bool IsBreak => Modifiers.CtrlKey && (SDLScanCode == SDL.Scancode.Pause);

	public bool IsModifierKey
	{
		get
		{
			switch (ScanCode)
			{
				case ScanCode.Control:
				case ScanCode.Alt:
				case ScanCode.LeftShift:
				case ScanCode.RightShift:
				case ScanCode.CapsLock:
				case ScanCode.NumLock:
					return true;
				default:
					return false;
			}
		}
	}

	private KeyEvent(KeyModifiers modifiers) { Modifiers = modifiers; }

	public KeyEvent Clone()
	{
		return
			new KeyEvent(Modifiers)
			{
				TextCharacter = TextCharacter,
				ScanCode = ScanCode,
				IsRight = IsRight,
				IsRelease = IsRelease,
				IsKeyPad = IsKeyPad,
				IsEphemeral = IsEphemeral,
			};
	}

	public string ToInKeyString()
	{
		if (IsEphemeral)
			return "";
		else if (TextCharacter != 0)
			return TextCharacter.ToString();
		else if (ScanCode != 0)
			return string.Concat((char)0, (char)ScanCode);
		else
			return "";
	}

	public KeyEvent(KeyEventData data)
	{
		var modifiers = data.Modifiers;

		TextCharacter = data.TextCharacter;
		SDLScanCode = data.RawKeyEventData.RawScanCode;
		ScanCode = data.ScanCode;
		IsRight = data.IsRight;
		Modifiers = modifiers;
		IsRelease = data.RawKeyEventData.IsRelease;
		IsEphemeral = data.IsEphemeral || IsRelease;
	}

	public KeyEvent NormalizeModifierCombinationKey()
	{
		bool ctrl = Modifiers.CtrlKey;
		bool alt = Modifiers.AltKey;
		bool shift = Modifiers.ShiftKey;

		ScanCode scanCode = ScanCode;

		switch (ScanCode)
		{
			case ScanCode.ShiftF1: shift = true; scanCode = ScanCode.F1; break;
			case ScanCode.ShiftF2: shift = true; scanCode = ScanCode.F2; break;
			case ScanCode.ShiftF3: shift = true; scanCode = ScanCode.F3; break;
			case ScanCode.ShiftF4: shift = true; scanCode = ScanCode.F4; break;
			case ScanCode.ShiftF5: shift = true; scanCode = ScanCode.F5; break;
			case ScanCode.ShiftF6: shift = true; scanCode = ScanCode.F6; break;
			case ScanCode.ShiftF7: shift = true; scanCode = ScanCode.F7; break;
			case ScanCode.ShiftF8: shift = true; scanCode = ScanCode.F8; break;
			case ScanCode.ShiftF9: shift = true; scanCode = ScanCode.F9; break;
			case ScanCode.ShiftF10: shift = true; scanCode = ScanCode.F10; break;
			case ScanCode.CtrlF1: ctrl = true; scanCode = ScanCode.F1; break;
			case ScanCode.CtrlF2: ctrl = true; scanCode = ScanCode.F2; break;
			case ScanCode.CtrlF3: ctrl = true; scanCode = ScanCode.F3; break;
			case ScanCode.CtrlF4: ctrl = true; scanCode = ScanCode.F4; break;
			case ScanCode.CtrlF5: ctrl = true; scanCode = ScanCode.F5; break;
			case ScanCode.CtrlF6: ctrl = true; scanCode = ScanCode.F6; break;
			case ScanCode.CtrlF7: ctrl = true; scanCode = ScanCode.F7; break;
			case ScanCode.CtrlF8: ctrl = true; scanCode = ScanCode.F8; break;
			case ScanCode.CtrlF9: ctrl = true; scanCode = ScanCode.F9; break;
			case ScanCode.CtrlF10: ctrl = true; scanCode = ScanCode.F10; break;
			case ScanCode.AltF1: alt = true; scanCode = ScanCode.F1; break;
			case ScanCode.AltF2: alt = true; scanCode = ScanCode.F2; break;
			case ScanCode.AltF3: alt = true; scanCode = ScanCode.F3; break;
			case ScanCode.AltF4: alt = true; scanCode = ScanCode.F4; break;
			case ScanCode.AltF5: alt = true; scanCode = ScanCode.F5; break;
			case ScanCode.AltF6: alt = true; scanCode = ScanCode.F6; break;
			case ScanCode.AltF7: alt = true; scanCode = ScanCode.F7; break;
			case ScanCode.AltF8: alt = true; scanCode = ScanCode.F8; break;
			case ScanCode.AltF9: alt = true; scanCode = ScanCode.F9; break;
			case ScanCode.AltF10: alt = true; scanCode = ScanCode.F10; break;
			case ScanCode.CtrlLeft: ctrl = true; scanCode = ScanCode.Left; break;
			case ScanCode.CtrlRight: ctrl = true; scanCode = ScanCode.Right; break;
			case ScanCode.CtrlEnd: ctrl = true; scanCode = ScanCode.End; break;
			case ScanCode.CtrlPageDown: ctrl = true; scanCode = ScanCode.PageDown; break;
			case ScanCode.CtrlHome: ctrl = true; scanCode = ScanCode.Home; break;
			case ScanCode.Alt1: alt = true; scanCode = ScanCode._1; break;
			case ScanCode.Alt2: alt = true; scanCode = ScanCode._2; break;
			case ScanCode.Alt3: alt = true; scanCode = ScanCode._3; break;
			case ScanCode.Alt4: alt = true; scanCode = ScanCode._4; break;
			case ScanCode.Alt5: alt = true; scanCode = ScanCode._5; break;
			case ScanCode.Alt6: alt = true; scanCode = ScanCode._6; break;
			case ScanCode.Alt7: alt = true; scanCode = ScanCode._7; break;
			case ScanCode.Alt8: alt = true; scanCode = ScanCode._8; break;
			case ScanCode.Alt9: alt = true; scanCode = ScanCode._9; break;
			case ScanCode.Alt0: alt = true; scanCode = ScanCode._0; break;
			case ScanCode.AltMinus: alt = true; scanCode = ScanCode.Minus; break;
			case ScanCode.AltEquals: alt = true; scanCode = ScanCode.Equals; break;
			case ScanCode.CtrlPageUp: ctrl = true; scanCode = ScanCode.PageUp; break;
			case ScanCode.ShiftF11: shift = true; scanCode = ScanCode.F11; break;
			case ScanCode.ShiftF12: shift = true; scanCode = ScanCode.F12; break;
			case ScanCode.CtrlF11: ctrl = true; scanCode = ScanCode.F11; break;
			case ScanCode.CtrlF12: ctrl = true; scanCode = ScanCode.F12; break;
			case ScanCode.AltF11: alt = true; scanCode = ScanCode.F11; break;
			case ScanCode.AltF12: alt = true; scanCode = ScanCode.F12; break;
			case ScanCode.CtrlUp: ctrl = true; scanCode = ScanCode.Up; break;
			case ScanCode.CtrlKpMinus: ctrl = true; scanCode = ScanCode.KpMinus; break;
			case ScanCode.CtrlKp5: ctrl = true; scanCode = ScanCode.Kp5; break;
			case ScanCode.CtrlKpPlus: ctrl = true; scanCode = ScanCode.KpPlus; break;
			case ScanCode.CtrlDown: ctrl = true; scanCode = ScanCode.Down; break;
			case ScanCode.CtrlInsert: ctrl = true; scanCode = ScanCode.Insert; break;
			case ScanCode.CtrlDelete: ctrl = true; scanCode = ScanCode.Delete; break;
			case ScanCode.CtrlTab: ctrl = true; scanCode = ScanCode.Tab; break;
			case ScanCode.AltHome: alt = true; scanCode = ScanCode.Home; break;
			case ScanCode.AltUp: alt = true; scanCode = ScanCode.Up; break;
			case ScanCode.AltPageUp: alt = true; scanCode = ScanCode.PageUp; break;
			case ScanCode.AltLeft: alt = true; scanCode = ScanCode.Left; break;
			case ScanCode.AltRight: alt = true; scanCode = ScanCode.Right; break;
			case ScanCode.AltEnd: alt = true; scanCode = ScanCode.End; break;
			case ScanCode.AltDown: alt = true; scanCode = ScanCode.Down; break;
			case ScanCode.AltPageDown: alt = true; scanCode = ScanCode.PageDown; break;
			case ScanCode.AltInsert: alt = true; scanCode = ScanCode.Insert; break;
			case ScanCode.AltDelete: alt = true; scanCode = ScanCode.Delete; break;
		}

		if (scanCode == ScanCode)
			return this;
		else
		{
			var modifiers = new KeyModifiers(ctrl, alt, shift, Modifiers.CapsLock, Modifiers.NumLock);

			return
				new KeyEvent(modifiers)
				{
					ScanCode = scanCode,

					TextCharacter = TextCharacter,
					IsRight = IsRight,
					IsRelease = IsRelease,
					IsKeyPad = IsKeyPad,
					IsEphemeral = IsEphemeral,
					SDLScanCode = SDLScanCode,
				};
		}
	}

	public void Serialize(BinaryWriter writer)
	{
		writer.Write((short)TextCharacter);
		writer.Write((byte)ScanCode);
		writer.Write(Modifiers.Pack());
		writer.Write(IsRight);
		writer.Write(IsRelease);
		writer.Write(IsKeyPad);
		writer.Write(IsEphemeral);
		writer.Write((int)SDLScanCode);
	}

	public static KeyEvent Deserialize(BinaryReader reader)
	{
		char textCharacter = (char)reader.ReadInt16();
		byte scanCode = reader.ReadByte();
		byte packedModifiers = reader.ReadByte();
		bool isRight = reader.ReadBoolean();
		bool isRelease = reader.ReadBoolean();
		bool isKeyPad = reader.ReadBoolean();
		bool isEphemeral = reader.ReadBoolean();
		int sdlScanCode = reader.ReadInt32();

		var ret = new KeyEvent(KeyModifiers.Unpack(packedModifiers));

		ret.TextCharacter = textCharacter;
		ret.ScanCode = (ScanCode)scanCode;
		ret.IsRight = isRight;
		ret.IsRelease = isRelease;
		ret.IsKeyPad = isKeyPad;
		ret.IsEphemeral = isEphemeral;
		ret.SDLScanCode = (SDL.Scancode)sdlScanCode;

		return ret;
	}
}
