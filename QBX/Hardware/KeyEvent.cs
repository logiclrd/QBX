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
		if (IsEphemeral || IsRelease)
			return "";
		else if (TextCharacter != 0)
			return TextCharacter.ToString();
		else if (ScanCode != 0)
			return string.Concat((char)0, (char)ScanCode);
		else
			return "";
	}

	public KeyEvent(SDL.Scancode sdlScanCode, KeyModifiers modifiers, bool isRelease)
	{
		bool upperCase = modifiers.ShiftKey ^ modifiers.CapsLock;

		SDLScanCode = sdlScanCode;
		Modifiers = modifiers;
		IsRelease = isRelease;

		switch (sdlScanCode)
		{
			case SDL.Scancode.LCtrl:
			case SDL.Scancode.RCtrl:
			case SDL.Scancode.LAlt:
			case SDL.Scancode.RAlt:
			case SDL.Scancode.LShift:
			case SDL.Scancode.RShift:
			case SDL.Scancode.Capslock:
			case SDL.Scancode.NumLockClear:
			case SDL.Scancode.Scrolllock:
			{
				switch (sdlScanCode)
				{
					case SDL.Scancode.LCtrl: ScanCode = ScanCode.Control; break;
					case SDL.Scancode.RCtrl: ScanCode = ScanCode.Control; IsRight = true; break;
					case SDL.Scancode.LAlt: ScanCode = ScanCode.Alt; break;
					case SDL.Scancode.RAlt: ScanCode = ScanCode.Alt; IsRight = true; break;
					case SDL.Scancode.LShift: ScanCode = ScanCode.LeftShift; break;
					case SDL.Scancode.RShift: ScanCode = ScanCode.RightShift; IsRight = true; break;
					case SDL.Scancode.Capslock: ScanCode = ScanCode.CapsLock; break;
					case SDL.Scancode.NumLockClear: ScanCode = ScanCode.NumLock; break;
					case SDL.Scancode.Scrolllock: ScanCode = ScanCode.ScrollLock; break;
				}

				IsEphemeral = true;

				return;
			}

			case SDL.Scancode.KpDivide:
			case SDL.Scancode.KpMultiply:
			case SDL.Scancode.KpMinus:
			case SDL.Scancode.KpPlus:
			case SDL.Scancode.KpEnter:
			case SDL.Scancode.KpPeriod:
			case SDL.Scancode.Kp0:
			case SDL.Scancode.Kp1:
			case SDL.Scancode.Kp2:
			case SDL.Scancode.Kp3:
			case SDL.Scancode.Kp4:
			case SDL.Scancode.Kp5:
			case SDL.Scancode.Kp6:
			case SDL.Scancode.Kp7:
			case SDL.Scancode.Kp8:
			case SDL.Scancode.Kp9:
				IsKeyPad = true;
				break;
		}

		// Default translation, may be overridden
		ScanCode = TranslateScanCode(sdlScanCode);

		if (modifiers.AltKey)
		{
			switch (sdlScanCode)
			{
				case SDL.Scancode.Escape: ScanCode = ScanCode.Escape; break;
				case SDL.Scancode.F1: ScanCode = ScanCode.AltF1; break;
				case SDL.Scancode.F2: ScanCode = ScanCode.AltF2; break;
				case SDL.Scancode.F3: ScanCode = ScanCode.AltF3; break;
				case SDL.Scancode.F4: ScanCode = ScanCode.AltF4; break;
				case SDL.Scancode.F5: ScanCode = ScanCode.AltF5; break;
				case SDL.Scancode.F6: ScanCode = ScanCode.AltF6; break;
				case SDL.Scancode.F7: ScanCode = ScanCode.AltF7; break;
				case SDL.Scancode.F8: ScanCode = ScanCode.AltF8; break;
				case SDL.Scancode.F9: ScanCode = ScanCode.AltF9; break;
				case SDL.Scancode.F10: ScanCode = ScanCode.AltF10; break;
				case SDL.Scancode.F11: ScanCode = ScanCode.AltF11; break;
				case SDL.Scancode.F12: ScanCode = ScanCode.AltF12; break;
				case SDL.Scancode.Grave: ScanCode = ScanCode.Grave; break;
				case SDL.Scancode.Alpha1: ScanCode = ScanCode.Alt1; break;
				case SDL.Scancode.Alpha2: ScanCode = ScanCode.Alt2; break;
				case SDL.Scancode.Alpha3: ScanCode = ScanCode.Alt3; break;
				case SDL.Scancode.Alpha4: ScanCode = ScanCode.Alt4; break;
				case SDL.Scancode.Alpha5: ScanCode = ScanCode.Alt5; break;
				case SDL.Scancode.Alpha6: ScanCode = ScanCode.Alt6; break;
				case SDL.Scancode.Alpha7: ScanCode = ScanCode.Alt7; break;
				case SDL.Scancode.Alpha8: ScanCode = ScanCode.Alt8; break;
				case SDL.Scancode.Alpha9: ScanCode = ScanCode.Alt9; break;
				case SDL.Scancode.Alpha0: ScanCode = ScanCode.Alt0; break;
				case SDL.Scancode.Minus: ScanCode = ScanCode.AltMinus; break;
				case SDL.Scancode.Equals: ScanCode = ScanCode.AltEquals; break;
				case SDL.Scancode.Backspace: ScanCode = ScanCode.Backspace; break;
				case SDL.Scancode.Q: ScanCode = ScanCode.Q; break;
				case SDL.Scancode.W: ScanCode = ScanCode.W; break;
				case SDL.Scancode.E: ScanCode = ScanCode.E; break;
				case SDL.Scancode.R: ScanCode = ScanCode.R; break;
				case SDL.Scancode.T: ScanCode = ScanCode.T; break;
				case SDL.Scancode.Y: ScanCode = ScanCode.Y; break;
				case SDL.Scancode.U: ScanCode = ScanCode.U; break;
				case SDL.Scancode.I: ScanCode = ScanCode.I; break;
				case SDL.Scancode.O: ScanCode = ScanCode.O; break;
				case SDL.Scancode.P: ScanCode = ScanCode.P; break;
				case SDL.Scancode.Leftbracket: ScanCode = ScanCode.LeftBracket; break;
				case SDL.Scancode.Rightbracket: ScanCode = ScanCode = ScanCode.RightBracket; break;
				case SDL.Scancode.Backslash: ScanCode = ScanCode.Backslash; break;
				case SDL.Scancode.A: ScanCode = ScanCode.A; break;
				case SDL.Scancode.S: ScanCode = ScanCode.S; break;
				case SDL.Scancode.D: ScanCode = ScanCode.D; break;
				case SDL.Scancode.F: ScanCode = ScanCode.F; break;
				case SDL.Scancode.G: ScanCode = ScanCode.G; break;
				case SDL.Scancode.H: ScanCode = ScanCode.H; break;
				case SDL.Scancode.J: ScanCode = ScanCode.J; break;
				case SDL.Scancode.K: ScanCode = ScanCode.K; break;
				case SDL.Scancode.L: ScanCode = ScanCode.L; break;
				case SDL.Scancode.Semicolon: ScanCode = ScanCode.Semicolon; break;
				case SDL.Scancode.Apostrophe: ScanCode = ScanCode.Apostrophe; break;
				case SDL.Scancode.Return: ScanCode = ScanCode.Return; break;
				case SDL.Scancode.Z: ScanCode = ScanCode.Z; break;
				case SDL.Scancode.X: ScanCode = ScanCode.X; break;
				case SDL.Scancode.C: ScanCode = ScanCode.C; break;
				case SDL.Scancode.V: ScanCode = ScanCode.V; break;
				case SDL.Scancode.B: ScanCode = ScanCode.B; break;
				case SDL.Scancode.N: ScanCode = ScanCode.N; break;
				case SDL.Scancode.M: ScanCode = ScanCode.M; break;
				case SDL.Scancode.Comma: ScanCode = ScanCode.Comma; break;
				case SDL.Scancode.Period: ScanCode = ScanCode.Period; break;
				case SDL.Scancode.Slash: ScanCode = ScanCode.Slash; break;
				case SDL.Scancode.Space: ScanCode = ScanCode.Space; TextCharacter = ' '; break;
				case SDL.Scancode.Insert: ScanCode = ScanCode.AltInsert; break;
				case SDL.Scancode.Delete: ScanCode = ScanCode.AltDelete; break;
				case SDL.Scancode.Home: ScanCode = ScanCode.AltHome; break;
				case SDL.Scancode.End: ScanCode = ScanCode.AltEnd; break;
				case SDL.Scancode.Pageup: ScanCode = ScanCode.AltPageUp; break;
				case SDL.Scancode.Pagedown: ScanCode = ScanCode.AltPageDown; break;
				case SDL.Scancode.KpDivide: ScanCode = ScanCode.AltKpDivide; break;
				case SDL.Scancode.KpMultiply: ScanCode = ScanCode.KpMultiply; break;
				case SDL.Scancode.KpMinus: ScanCode = ScanCode.KpMinus; break;
				case SDL.Scancode.KpPlus: ScanCode = ScanCode.KpPlus; break;
				case SDL.Scancode.KpEnter: ScanCode = ScanCode.AltKpEnter; break;
				case SDL.Scancode.Up: ScanCode = ScanCode.AltUp; break;
				case SDL.Scancode.Left: ScanCode = ScanCode.AltLeft; break;
				case SDL.Scancode.Down: ScanCode = ScanCode.AltDown; break;
				case SDL.Scancode.Right: ScanCode = ScanCode.AltRight; break;
			}
		}
		else if (modifiers.CtrlKey)
		{
			switch (sdlScanCode)
			{
				case SDL.Scancode.Escape: ScanCode = ScanCode.Escape; TextCharacter = (char)27; break;
				case SDL.Scancode.F1: ScanCode = ScanCode.CtrlF1; break;
				case SDL.Scancode.F2: ScanCode = ScanCode.CtrlF2; break;
				case SDL.Scancode.F3: ScanCode = ScanCode.CtrlF3; break;
				case SDL.Scancode.F4: ScanCode = ScanCode.CtrlF4; break;
				case SDL.Scancode.F5: ScanCode = ScanCode.CtrlF5; break;
				case SDL.Scancode.F6: ScanCode = ScanCode.CtrlF6; break;
				case SDL.Scancode.F7: ScanCode = ScanCode.CtrlF7; break;
				case SDL.Scancode.F8: ScanCode = ScanCode.CtrlF8; break;
				case SDL.Scancode.F9: ScanCode = ScanCode.CtrlF9; break;
				case SDL.Scancode.F10: ScanCode = ScanCode.CtrlF10; break;
				case SDL.Scancode.F11: ScanCode = ScanCode.CtrlF11; break;
				case SDL.Scancode.F12: ScanCode = ScanCode.CtrlF12; break;
				case SDL.Scancode.Alpha2: ScanCode = ScanCode._2; break;
				case SDL.Scancode.Alpha6: ScanCode = ScanCode._6; TextCharacter = (char)30; break;
				case SDL.Scancode.Minus: ScanCode = ScanCode.Minus; TextCharacter = (char)31; break;
				case SDL.Scancode.Backspace: ScanCode = ScanCode.Backspace; TextCharacter = (char)127; break;
				case SDL.Scancode.Tab: ScanCode = ScanCode.CtrlTab; break;
				case SDL.Scancode.Q: ScanCode = ScanCode.Q; TextCharacter = (char)17; break;
				case SDL.Scancode.W: ScanCode = ScanCode.W; TextCharacter = (char)23; break;
				case SDL.Scancode.E: ScanCode = ScanCode.E; TextCharacter = (char)5; break;
				case SDL.Scancode.R: ScanCode = ScanCode.R; TextCharacter = (char)18; break;
				case SDL.Scancode.T: ScanCode = ScanCode.T; TextCharacter = (char)20; break;
				case SDL.Scancode.Y: ScanCode = ScanCode.Y; TextCharacter = (char)25; break;
				case SDL.Scancode.U: ScanCode = ScanCode.U; TextCharacter = (char)21; break;
				case SDL.Scancode.I: ScanCode = ScanCode.I; TextCharacter = (char)9; break;
				case SDL.Scancode.O: ScanCode = ScanCode.O; TextCharacter = (char)15; break;
				case SDL.Scancode.P: ScanCode = ScanCode.P; TextCharacter = (char)16; break;
				case SDL.Scancode.Leftbracket: ScanCode = ScanCode.LeftBracket; TextCharacter = (char)27; break;
				case SDL.Scancode.Rightbracket: ScanCode = ScanCode.RightBracket; TextCharacter = (char)29; break;
				case SDL.Scancode.Backslash: ScanCode = ScanCode.Backslash; TextCharacter = (char)28; break;
				case SDL.Scancode.A: ScanCode = ScanCode.A; TextCharacter = (char)1; break;
				case SDL.Scancode.S: ScanCode = ScanCode.S; TextCharacter = (char)19; break;
				case SDL.Scancode.D: ScanCode = ScanCode.D; TextCharacter = (char)4; break;
				case SDL.Scancode.F: ScanCode = ScanCode.F; TextCharacter = (char)6; break;
				case SDL.Scancode.G: ScanCode = ScanCode.G; TextCharacter = (char)7; break;
				case SDL.Scancode.H: ScanCode = ScanCode.H; TextCharacter = (char)8; break;
				case SDL.Scancode.J: ScanCode = ScanCode.J; TextCharacter = (char)10; break;
				case SDL.Scancode.K: ScanCode = ScanCode.K; TextCharacter = (char)11; break;
				case SDL.Scancode.L: ScanCode = ScanCode.L; TextCharacter = (char)12; break;
				case SDL.Scancode.Return: ScanCode = ScanCode.Return; TextCharacter = (char)10; break;
				case SDL.Scancode.Z: ScanCode = ScanCode.Z; TextCharacter = (char)26; break;
				case SDL.Scancode.X: ScanCode = ScanCode.X; TextCharacter = (char)24; break;
				case SDL.Scancode.C: ScanCode = ScanCode.C; TextCharacter = (char)3; break;
				case SDL.Scancode.V: ScanCode = ScanCode.V; TextCharacter = (char)22; break;
				case SDL.Scancode.B: ScanCode = ScanCode.B; TextCharacter = (char)2; break;
				case SDL.Scancode.N: ScanCode = ScanCode.N; TextCharacter = (char)14; break;
				case SDL.Scancode.M: ScanCode = ScanCode.M; TextCharacter = (char)13; break;
				case SDL.Scancode.Insert: ScanCode = ScanCode.CtrlInsert; break;
				case SDL.Scancode.Delete: ScanCode = ScanCode.CtrlDelete; break;
				case SDL.Scancode.Home: ScanCode = ScanCode.CtrlHome; break;
				case SDL.Scancode.End: ScanCode = ScanCode.CtrlEnd; break;
				case SDL.Scancode.Pageup: ScanCode = ScanCode.CtrlPageUp; break;
				case SDL.Scancode.Pagedown: ScanCode = ScanCode.CtrlPageDown; break;
				case SDL.Scancode.KpDivide: ScanCode = ScanCode.CtrlKpDivide; break;
				case SDL.Scancode.KpMultiply: ScanCode = ScanCode.CtrlKpMultiply; break;
				case SDL.Scancode.KpMinus: ScanCode = ScanCode.CtrlKpMinus; break;
				case SDL.Scancode.Kp7: ScanCode = ScanCode.CtrlHome; break;
				case SDL.Scancode.Kp8: ScanCode = ScanCode.CtrlUp; break;
				case SDL.Scancode.Kp9: ScanCode = ScanCode.CtrlPageUp; break;
				case SDL.Scancode.KpPlus: ScanCode = ScanCode.CtrlKpPlus; break;
				case SDL.Scancode.Kp4: ScanCode = ScanCode.CtrlLeft; break;
				case SDL.Scancode.Kp5: ScanCode = ScanCode.CtrlKp5; break;
				case SDL.Scancode.Kp6: ScanCode = ScanCode.CtrlRight; break;
				case SDL.Scancode.Kp1: ScanCode = ScanCode.CtrlEnd; break;
				case SDL.Scancode.Kp2: ScanCode = ScanCode.CtrlDown; break;
				case SDL.Scancode.Kp3: ScanCode = ScanCode.CtrlPageDown; break;
				case SDL.Scancode.KpEnter: ScanCode = ScanCode.Return; TextCharacter = (char)10; break;
				case SDL.Scancode.Kp0: ScanCode = ScanCode.CtrlInsert; break;
				case SDL.Scancode.KpPeriod: ScanCode = ScanCode.CtrlDelete; break;
				case SDL.Scancode.Up: ScanCode = ScanCode.CtrlUp; break;
				case SDL.Scancode.Left: ScanCode = ScanCode.CtrlLeft; break;
				case SDL.Scancode.Down: ScanCode = ScanCode.CtrlDown; break;
				case SDL.Scancode.Right: ScanCode = ScanCode.CtrlRight; break;
			}
		}
		else if (modifiers.ShiftKey)
		{
			switch (sdlScanCode)
			{
				case SDL.Scancode.Escape: ScanCode = ScanCode.Escape; TextCharacter = (char)27; break;
				case SDL.Scancode.F1: ScanCode = ScanCode.ShiftF1; break;
				case SDL.Scancode.F2: ScanCode = ScanCode.ShiftF2; break;
				case SDL.Scancode.F3: ScanCode = ScanCode.ShiftF3; break;
				case SDL.Scancode.F4: ScanCode = ScanCode.ShiftF4; break;
				case SDL.Scancode.F5: ScanCode = ScanCode.ShiftF5; break;
				case SDL.Scancode.F6: ScanCode = ScanCode.ShiftF6; break;
				case SDL.Scancode.F7: ScanCode = ScanCode.ShiftF7; break;
				case SDL.Scancode.F8: ScanCode = ScanCode.ShiftF8; break;
				case SDL.Scancode.F9: ScanCode = ScanCode.ShiftF9; break;
				case SDL.Scancode.F10: ScanCode = ScanCode.ShiftF10; break;
				case SDL.Scancode.F11: ScanCode = ScanCode.ShiftF11; break;
				case SDL.Scancode.F12: ScanCode = ScanCode.ShiftF12; break;
				case SDL.Scancode.Grave: ScanCode = ScanCode.Grave; TextCharacter = '~'; break;
				case SDL.Scancode.Alpha1: ScanCode = ScanCode._1; TextCharacter = '!'; break;
				case SDL.Scancode.Alpha2: ScanCode = ScanCode._2; TextCharacter = '@'; break;
				case SDL.Scancode.Alpha3: ScanCode = ScanCode._3; TextCharacter = '#'; break;
				case SDL.Scancode.Alpha4: ScanCode = ScanCode._4; TextCharacter = '$'; break;
				case SDL.Scancode.Alpha5: ScanCode = ScanCode._5; TextCharacter = '%'; break;
				case SDL.Scancode.Alpha6: ScanCode = ScanCode._6; TextCharacter = '^'; break;
				case SDL.Scancode.Alpha7: ScanCode = ScanCode._7; TextCharacter = '&'; break;
				case SDL.Scancode.Alpha8: ScanCode = ScanCode._8; TextCharacter = '*'; break;
				case SDL.Scancode.Alpha9: ScanCode = ScanCode._9; TextCharacter = '('; break;
				case SDL.Scancode.Alpha0: ScanCode = ScanCode._0; TextCharacter = ')'; break;
				case SDL.Scancode.Minus: ScanCode = ScanCode.Minus; TextCharacter = '_'; break;
				case SDL.Scancode.Equals: ScanCode = ScanCode.Equals; TextCharacter = '+'; break;
				case SDL.Scancode.Backspace: ScanCode = ScanCode.Backspace; TextCharacter = (char)8; break;
				case SDL.Scancode.Tab: ScanCode = ScanCode.Tab; break;
				case SDL.Scancode.Q: ScanCode = ScanCode.Q; TextCharacter = upperCase ? 'Q' : 'q'; break;
				case SDL.Scancode.W: ScanCode = ScanCode.W; TextCharacter = upperCase ? 'W' : 'w'; break;
				case SDL.Scancode.E: ScanCode = ScanCode.E; TextCharacter = upperCase ? 'E' : 'e'; break;
				case SDL.Scancode.R: ScanCode = ScanCode.R; TextCharacter = upperCase ? 'R' : 'r'; break;
				case SDL.Scancode.T: ScanCode = ScanCode.T; TextCharacter = upperCase ? 'T' : 't'; break;
				case SDL.Scancode.Y: ScanCode = ScanCode.Y; TextCharacter = upperCase ? 'Y' : 'y'; break;
				case SDL.Scancode.U: ScanCode = ScanCode.U; TextCharacter = upperCase ? 'U' : 'u'; break;
				case SDL.Scancode.I: ScanCode = ScanCode.I; TextCharacter = upperCase ? 'I' : 'i'; break;
				case SDL.Scancode.O: ScanCode = ScanCode.O; TextCharacter = upperCase ? 'O' : 'o'; break;
				case SDL.Scancode.P: ScanCode = ScanCode.P; TextCharacter = upperCase ? 'P' : 'p'; break;
				case SDL.Scancode.Leftbracket: ScanCode = ScanCode.LeftBracket; TextCharacter = '{'; break;
				case SDL.Scancode.Rightbracket: ScanCode = ScanCode.RightBracket; TextCharacter = '}'; break;
				case SDL.Scancode.Backslash: ScanCode = ScanCode.Backslash; TextCharacter = '|'; break;
				case SDL.Scancode.A: ScanCode = ScanCode.A; TextCharacter = upperCase ? 'A' : 'a'; break;
				case SDL.Scancode.S: ScanCode = ScanCode.S; TextCharacter = upperCase ? 'S' : 's'; break;
				case SDL.Scancode.D: ScanCode = ScanCode.D; TextCharacter = upperCase ? 'D' : 'd'; break;
				case SDL.Scancode.F: ScanCode = ScanCode.F; TextCharacter = upperCase ? 'F' : 'f'; break;
				case SDL.Scancode.G: ScanCode = ScanCode.G; TextCharacter = upperCase ? 'G' : 'g'; break;
				case SDL.Scancode.H: ScanCode = ScanCode.H; TextCharacter = upperCase ? 'H' : 'h'; break;
				case SDL.Scancode.J: ScanCode = ScanCode.J; TextCharacter = upperCase ? 'J' : 'j'; break;
				case SDL.Scancode.K: ScanCode = ScanCode.K; TextCharacter = upperCase ? 'K' : 'k'; break;
				case SDL.Scancode.L: ScanCode = ScanCode.L; TextCharacter = upperCase ? 'L' : 'l'; break;
				case SDL.Scancode.Semicolon: ScanCode = ScanCode.Semicolon; TextCharacter = ':'; break;
				case SDL.Scancode.Apostrophe: ScanCode = ScanCode.Apostrophe; TextCharacter = '"'; break;
				case SDL.Scancode.Return: ScanCode = ScanCode.Return; TextCharacter = (char)13; break;
				case SDL.Scancode.Z: ScanCode = ScanCode.Z; TextCharacter = upperCase ? 'Z' : 'z'; break;
				case SDL.Scancode.X: ScanCode = ScanCode.X; TextCharacter = upperCase ? 'X' : 'x'; break;
				case SDL.Scancode.C: ScanCode = ScanCode.C; TextCharacter = upperCase ? 'C' : 'c'; break;
				case SDL.Scancode.V: ScanCode = ScanCode.V; TextCharacter = upperCase ? 'V' : 'v'; break;
				case SDL.Scancode.B: ScanCode = ScanCode.B; TextCharacter = upperCase ? 'B' : 'b'; break;
				case SDL.Scancode.N: ScanCode = ScanCode.N; TextCharacter = upperCase ? 'N' : 'n'; break;
				case SDL.Scancode.M: ScanCode = ScanCode.M; TextCharacter = upperCase ? 'M' : 'm'; break;
				case SDL.Scancode.Comma: ScanCode = ScanCode.Comma; TextCharacter = '<'; break;
				case SDL.Scancode.Period: ScanCode = ScanCode.Period; TextCharacter = '>'; break;
				case SDL.Scancode.Slash: ScanCode = ScanCode.Slash; TextCharacter = '?'; break;
				case SDL.Scancode.Space: ScanCode = ScanCode.Space; TextCharacter = ' '; break;
				case SDL.Scancode.Insert: ScanCode = ScanCode.Insert; break;
				case SDL.Scancode.Delete: ScanCode = ScanCode.Delete; break;
				case SDL.Scancode.Home: ScanCode = ScanCode.Home; break;
				case SDL.Scancode.End: ScanCode = ScanCode.End; break;
				case SDL.Scancode.Pageup: ScanCode = ScanCode.PageUp; break;
				case SDL.Scancode.Pagedown: ScanCode = ScanCode.PageDown; break;
				case SDL.Scancode.KpDivide: TextCharacter = '/'; break;
				case SDL.Scancode.KpMultiply: ScanCode = ScanCode.KpMultiply; TextCharacter = '*'; break;
				case SDL.Scancode.KpMinus: ScanCode = ScanCode.KpMinus; TextCharacter = '-'; break;
				case SDL.Scancode.Kp7: ScanCode = ScanCode.Home; if (modifiers.NumLock) TextCharacter = '7'; break;
				case SDL.Scancode.Kp8: ScanCode = ScanCode.Up; if (modifiers.NumLock) TextCharacter = '8'; break;
				case SDL.Scancode.Kp9: ScanCode = ScanCode.PageUp; if (modifiers.NumLock) TextCharacter = '9'; break;
				case SDL.Scancode.KpPlus: ScanCode = ScanCode.KpPlus; TextCharacter = '+'; break;
				case SDL.Scancode.Kp4: ScanCode = ScanCode.Left; if (modifiers.NumLock) TextCharacter = '4'; break;
				case SDL.Scancode.Kp5: ScanCode = ScanCode.Kp5; if (modifiers.NumLock) TextCharacter = '5'; break;
				case SDL.Scancode.Kp6: ScanCode = ScanCode.Right; if (modifiers.NumLock) TextCharacter = '6'; break;
				case SDL.Scancode.Kp1: ScanCode = ScanCode.End; if (modifiers.NumLock) TextCharacter = '1'; break;
				case SDL.Scancode.Kp2: ScanCode = ScanCode.Down; if (modifiers.NumLock) TextCharacter = '2'; break;
				case SDL.Scancode.Kp3: ScanCode = ScanCode.PageDown; if (modifiers.NumLock) TextCharacter = '3'; break;
				case SDL.Scancode.KpEnter: ScanCode = ScanCode.Return; TextCharacter = (char)13; break;
				case SDL.Scancode.Kp0: ScanCode = ScanCode.Insert; if (modifiers.NumLock) TextCharacter = '0'; break;
				case SDL.Scancode.KpPeriod: ScanCode = ScanCode.Delete; if (modifiers.NumLock) TextCharacter = '.'; break;
				case SDL.Scancode.Up: ScanCode = ScanCode.Up; break;
				case SDL.Scancode.Left: ScanCode = ScanCode.Left; break;
				case SDL.Scancode.Down: ScanCode = ScanCode.Down; break;
				case SDL.Scancode.Right: ScanCode = ScanCode.Right; break;
			}
		}
		else
		{
			switch (sdlScanCode)
			{
				case SDL.Scancode.Escape: TextCharacter = (char)27; break;
				case SDL.Scancode.F1: ScanCode = ScanCode.F1; break;
				case SDL.Scancode.F2: ScanCode = ScanCode.F2; break;
				case SDL.Scancode.F3: ScanCode = ScanCode.F3; break;
				case SDL.Scancode.F4: ScanCode = ScanCode.F4; break;
				case SDL.Scancode.F5: ScanCode = ScanCode.F5; break;
				case SDL.Scancode.F6: ScanCode = ScanCode.F6; break;
				case SDL.Scancode.F7: ScanCode = ScanCode.F7; break;
				case SDL.Scancode.F8: ScanCode = ScanCode.F8; break;
				case SDL.Scancode.F9: ScanCode = ScanCode.F9; break;
				case SDL.Scancode.F10: ScanCode = ScanCode.F10; break;
				case SDL.Scancode.F11: ScanCode = ScanCode.F11; break;
				case SDL.Scancode.F12: ScanCode = ScanCode.F12; break;
				case SDL.Scancode.Grave: ScanCode = ScanCode.Grave; TextCharacter = '`'; break;
				case SDL.Scancode.Alpha1: ScanCode = ScanCode._1; TextCharacter = '1'; break;
				case SDL.Scancode.Alpha2: ScanCode = ScanCode._2; TextCharacter = '2'; break;
				case SDL.Scancode.Alpha3: ScanCode = ScanCode._3; TextCharacter = '3'; break;
				case SDL.Scancode.Alpha4: ScanCode = ScanCode._4; TextCharacter = '4'; break;
				case SDL.Scancode.Alpha5: ScanCode = ScanCode._5; TextCharacter = '5'; break;
				case SDL.Scancode.Alpha6: ScanCode = ScanCode._6; TextCharacter = '6'; break;
				case SDL.Scancode.Alpha7: ScanCode = ScanCode._7; TextCharacter = '7'; break;
				case SDL.Scancode.Alpha8: ScanCode = ScanCode._8; TextCharacter = '8'; break;
				case SDL.Scancode.Alpha9: ScanCode = ScanCode._9; TextCharacter = '9'; break;
				case SDL.Scancode.Alpha0: ScanCode = ScanCode._0; TextCharacter = '0'; break;
				case SDL.Scancode.Minus: ScanCode = ScanCode.Minus; TextCharacter = '-'; break;
				case SDL.Scancode.Equals: ScanCode = ScanCode.Equals; TextCharacter = '='; break;
				case SDL.Scancode.Backspace: ScanCode = ScanCode.Backspace; TextCharacter = (char)8; break;
				case SDL.Scancode.Tab: ScanCode = ScanCode.Tab; TextCharacter = '\t'; break;
				case SDL.Scancode.Q: ScanCode = ScanCode.Q; TextCharacter = upperCase ? 'Q' : 'q'; break;
				case SDL.Scancode.W: ScanCode = ScanCode.W; TextCharacter = upperCase ? 'W' : 'w'; break;
				case SDL.Scancode.E: ScanCode = ScanCode.E; TextCharacter = upperCase ? 'E' : 'e'; break;
				case SDL.Scancode.R: ScanCode = ScanCode.R; TextCharacter = upperCase ? 'R' : 'r'; break;
				case SDL.Scancode.T: ScanCode = ScanCode.T; TextCharacter = upperCase ? 'T' : 't'; break;
				case SDL.Scancode.Y: ScanCode = ScanCode.Y; TextCharacter = upperCase ? 'Y' : 'y'; break;
				case SDL.Scancode.U: ScanCode = ScanCode.U; TextCharacter = upperCase ? 'U' : 'u'; break;
				case SDL.Scancode.I: ScanCode = ScanCode.I; TextCharacter = upperCase ? 'I' : 'i'; break;
				case SDL.Scancode.O: ScanCode = ScanCode.O; TextCharacter = upperCase ? 'O' : 'o'; break;
				case SDL.Scancode.P: ScanCode = ScanCode.P; TextCharacter = upperCase ? 'P' : 'p'; break;
				case SDL.Scancode.Leftbracket: ScanCode = ScanCode.LeftBracket; TextCharacter = '['; break;
				case SDL.Scancode.Rightbracket: ScanCode = ScanCode.RightBracket; TextCharacter = ']'; break;
				case SDL.Scancode.Backslash: ScanCode = ScanCode.Backslash; TextCharacter = '\\'; break;
				case SDL.Scancode.A: ScanCode = ScanCode.A; TextCharacter = upperCase ? 'A' : 'a'; break;
				case SDL.Scancode.S: ScanCode = ScanCode.S; TextCharacter = upperCase ? 'S' : 's'; break;
				case SDL.Scancode.D: ScanCode = ScanCode.D; TextCharacter = upperCase ? 'D' : 'd'; break;
				case SDL.Scancode.F: ScanCode = ScanCode.F; TextCharacter = upperCase ? 'F' : 'f'; break;
				case SDL.Scancode.G: ScanCode = ScanCode.G; TextCharacter = upperCase ? 'G' : 'g'; break;
				case SDL.Scancode.H: ScanCode = ScanCode.H; TextCharacter = upperCase ? 'H' : 'h'; break;
				case SDL.Scancode.J: ScanCode = ScanCode.J; TextCharacter = upperCase ? 'J' : 'j'; break;
				case SDL.Scancode.K: ScanCode = ScanCode.K; TextCharacter = upperCase ? 'K' : 'k'; break;
				case SDL.Scancode.L: ScanCode = ScanCode.L; TextCharacter = upperCase ? 'L' : 'l'; break;
				case SDL.Scancode.Semicolon: ScanCode = ScanCode.Semicolon; TextCharacter = ';'; break;
				case SDL.Scancode.Apostrophe: ScanCode = ScanCode.Apostrophe; TextCharacter = '\''; break;
				case SDL.Scancode.Return: ScanCode = ScanCode.Return; TextCharacter = (char)13; break;
				case SDL.Scancode.Z: ScanCode = ScanCode.Z; TextCharacter = upperCase ? 'Z' : 'z'; break;
				case SDL.Scancode.X: ScanCode = ScanCode.X; TextCharacter = upperCase ? 'X' : 'x'; break;
				case SDL.Scancode.C: ScanCode = ScanCode.C; TextCharacter = upperCase ? 'C' : 'c'; break;
				case SDL.Scancode.V: ScanCode = ScanCode.V; TextCharacter = upperCase ? 'V' : 'v'; break;
				case SDL.Scancode.B: ScanCode = ScanCode.B; TextCharacter = upperCase ? 'B' : 'b'; break;
				case SDL.Scancode.N: ScanCode = ScanCode.N; TextCharacter = upperCase ? 'N' : 'n'; break;
				case SDL.Scancode.M: ScanCode = ScanCode.M; TextCharacter = upperCase ? 'M' : 'm'; break;
				case SDL.Scancode.Comma: ScanCode = ScanCode.Comma; TextCharacter = ','; break;
				case SDL.Scancode.Period: ScanCode = ScanCode.Period; TextCharacter = '.'; break;
				case SDL.Scancode.Slash: ScanCode = ScanCode.Slash; TextCharacter = '/'; break;
				case SDL.Scancode.Space: ScanCode = ScanCode.Space; TextCharacter = ' '; break;
				case SDL.Scancode.Insert: ScanCode = ScanCode.Insert; break;
				case SDL.Scancode.Delete: ScanCode = ScanCode.Delete; break;
				case SDL.Scancode.Home: ScanCode = ScanCode.Home; break;
				case SDL.Scancode.End: ScanCode = ScanCode.End; break;
				case SDL.Scancode.Pageup: ScanCode = ScanCode.PageUp; break;
				case SDL.Scancode.Pagedown: ScanCode = ScanCode.PageDown; break;
				case SDL.Scancode.KpDivide: TextCharacter = '/'; break;
				case SDL.Scancode.KpMultiply: TextCharacter = '*'; break;
				case SDL.Scancode.KpMinus: TextCharacter = '-'; break;
				case SDL.Scancode.Kp7: ScanCode = ScanCode.Home; if (modifiers.NumLock) TextCharacter = '7'; break;
				case SDL.Scancode.Kp8: ScanCode = ScanCode.Up; if (modifiers.NumLock) TextCharacter = '8'; break;
				case SDL.Scancode.Kp9: ScanCode = ScanCode.PageUp; if (modifiers.NumLock) TextCharacter = '9'; break;
				case SDL.Scancode.KpPlus: ScanCode = ScanCode.KpPlus; TextCharacter = '+'; break;
				case SDL.Scancode.Kp4: ScanCode = ScanCode.Left; if (modifiers.NumLock) TextCharacter = '4'; break;
				case SDL.Scancode.Kp5: ScanCode = ScanCode.Kp5; if (modifiers.NumLock) TextCharacter = '5'; break;
				case SDL.Scancode.Kp6: ScanCode = ScanCode.Right; if (modifiers.NumLock) TextCharacter = '6'; break;
				case SDL.Scancode.Kp1: ScanCode = ScanCode.End; if (modifiers.NumLock) TextCharacter = '1'; break;
				case SDL.Scancode.Kp2: ScanCode = ScanCode.Down; if (modifiers.NumLock) TextCharacter = '2'; break;
				case SDL.Scancode.Kp3: ScanCode = ScanCode.PageDown; if (modifiers.NumLock) TextCharacter = '3'; break;
				case SDL.Scancode.KpEnter: ScanCode = ScanCode.Return; TextCharacter = (char)13; break;
				case SDL.Scancode.Kp0: ScanCode = ScanCode.Insert; if (modifiers.NumLock) TextCharacter = '0'; break;
				case SDL.Scancode.KpPeriod: ScanCode = ScanCode.Delete; if (modifiers.NumLock) TextCharacter = '.'; break;
				case SDL.Scancode.Up: ScanCode = ScanCode.Up; break;
				case SDL.Scancode.Left: ScanCode = ScanCode.Left; break;
				case SDL.Scancode.Down: ScanCode = ScanCode.Down; break;
				case SDL.Scancode.Right: ScanCode = ScanCode.Right; break;
			}
		}
	}

	private ScanCode TranslateScanCode(SDL.Scancode sdlScanCode)
	{
		switch (sdlScanCode)
		{
			case SDL.Scancode.Escape: return ScanCode.Escape;
			case SDL.Scancode.Alpha1: return ScanCode._1;
			case SDL.Scancode.Alpha2: return ScanCode._2;
			case SDL.Scancode.Alpha3: return ScanCode._3;
			case SDL.Scancode.Alpha4: return ScanCode._4;
			case SDL.Scancode.Alpha5: return ScanCode._5;
			case SDL.Scancode.Alpha6: return ScanCode._6;
			case SDL.Scancode.Alpha7: return ScanCode._7;
			case SDL.Scancode.Alpha8: return ScanCode._8;
			case SDL.Scancode.Alpha9: return ScanCode._9;
			case SDL.Scancode.Alpha0: return ScanCode._0;
			case SDL.Scancode.Minus: return ScanCode.Minus;
			case SDL.Scancode.Equals: return ScanCode.Equals;
			case SDL.Scancode.Backspace: return ScanCode.Backspace;
			case SDL.Scancode.Tab: return ScanCode.Tab;
			case SDL.Scancode.Q: return ScanCode.Q;
			case SDL.Scancode.W: return ScanCode.W;
			case SDL.Scancode.E: return ScanCode.E;
			case SDL.Scancode.R: return ScanCode.R;
			case SDL.Scancode.T: return ScanCode.T;
			case SDL.Scancode.Y: return ScanCode.Y;
			case SDL.Scancode.U: return ScanCode.U;
			case SDL.Scancode.I: return ScanCode.I;
			case SDL.Scancode.O: return ScanCode.O;
			case SDL.Scancode.P: return ScanCode.P;
			case SDL.Scancode.Leftbracket: return ScanCode.LeftBracket;
			case SDL.Scancode.Rightbracket: return ScanCode.RightBracket;
			case SDL.Scancode.LCtrl: return ScanCode.Control;
			case SDL.Scancode.RCtrl: return ScanCode.Control;
			case SDL.Scancode.A: return ScanCode.A;
			case SDL.Scancode.S: return ScanCode.S;
			case SDL.Scancode.D: return ScanCode.D;
			case SDL.Scancode.F: return ScanCode.F;
			case SDL.Scancode.G: return ScanCode.G;
			case SDL.Scancode.H: return ScanCode.H;
			case SDL.Scancode.J: return ScanCode.J;
			case SDL.Scancode.K: return ScanCode.K;
			case SDL.Scancode.L: return ScanCode.L;
			case SDL.Scancode.Grave: return ScanCode.Grave;
			case SDL.Scancode.Semicolon: return ScanCode.Semicolon;
			case SDL.Scancode.Apostrophe: return ScanCode.Apostrophe;
			case SDL.Scancode.Return: return ScanCode.Return;
			case SDL.Scancode.LShift: return ScanCode.LeftShift;
			case SDL.Scancode.Backslash: return ScanCode.Backslash;
			case SDL.Scancode.Z: return ScanCode.Z;
			case SDL.Scancode.X: return ScanCode.X;
			case SDL.Scancode.C: return ScanCode.C;
			case SDL.Scancode.V: return ScanCode.V;
			case SDL.Scancode.B: return ScanCode.B;
			case SDL.Scancode.N: return ScanCode.N;
			case SDL.Scancode.M: return ScanCode.M;
			case SDL.Scancode.Comma: return ScanCode.Comma;
			case SDL.Scancode.Period: return ScanCode.Period;
			case SDL.Scancode.Slash: return ScanCode.Slash;
			case SDL.Scancode.RShift: return ScanCode.RightShift;
			case SDL.Scancode.KpMultiply: return ScanCode.KpMultiply;
			case SDL.Scancode.LAlt: return ScanCode.Alt;
			case SDL.Scancode.RAlt: return ScanCode.Alt;
			case SDL.Scancode.Space: return ScanCode.Space;
			case SDL.Scancode.Capslock: return ScanCode.CapsLock;
			case SDL.Scancode.F1: return ScanCode.F1;
			case SDL.Scancode.F2: return ScanCode.F2;
			case SDL.Scancode.F3: return ScanCode.F3;
			case SDL.Scancode.F4: return ScanCode.F4;
			case SDL.Scancode.F5: return ScanCode.F5;
			case SDL.Scancode.F6: return ScanCode.F6;
			case SDL.Scancode.F7: return ScanCode.F7;
			case SDL.Scancode.F8: return ScanCode.F8;
			case SDL.Scancode.F9: return ScanCode.F9;
			case SDL.Scancode.F10: return ScanCode.F10;
			case SDL.Scancode.NumLockClear: return ScanCode.NumLock;
			case SDL.Scancode.Scrolllock: return ScanCode.ScrollLock;
			case SDL.Scancode.Home: return ScanCode.Home;
			case SDL.Scancode.Up: return ScanCode.Up;
			case SDL.Scancode.Pageup: return ScanCode.PageUp;
			case SDL.Scancode.KpMinus: return ScanCode.KpMinus;
			case SDL.Scancode.Left: return ScanCode.Left;
			case SDL.Scancode.Kp5: return ScanCode.Kp5;
			case SDL.Scancode.Right: return ScanCode.Right;
			case SDL.Scancode.KpPlus: return ScanCode.KpPlus;
			case SDL.Scancode.End: return ScanCode.End;
			case SDL.Scancode.Down: return ScanCode.Down;
			case SDL.Scancode.Pagedown: return ScanCode.PageDown;
			case SDL.Scancode.Insert: return ScanCode.Insert;
			case SDL.Scancode.Delete: return ScanCode.Delete;
			case SDL.Scancode.F11: return ScanCode.F11;
			case SDL.Scancode.F12: return ScanCode.F12;

			default: return ScanCode.None;
		}
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
}
