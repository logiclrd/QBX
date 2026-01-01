using SDL3;

namespace QBX.Hardware;

public class KeyEvent
{
	public char TextCharacter;
	public int ScanCode;

	public bool IsEmpty => (TextCharacter == default) && (ScanCode == 0);
	public bool IsAlphanumeric => (TextCharacter >= 32);

	public string ToInKeyString()
	{
		if (ScanCode != 0)
			return string.Concat((char)0, (char)ScanCode);
		else if (TextCharacter != 0)
			return TextCharacter.ToString();
		else
			return "";
	}

	public KeyEvent(SDL.Scancode sdlScanCode, bool ctrlKey, bool altKey, bool shiftKey, bool capsLock, bool numLock)
	{
		bool upperCase = shiftKey ^ capsLock;

		if (altKey)
		{
			switch (sdlScanCode)
			{
				case SDL.Scancode.Escape: ScanCode = 1; break;
				case SDL.Scancode.F1: ScanCode = 104; break;
				case SDL.Scancode.F2: ScanCode = 105; break;
				case SDL.Scancode.F3: ScanCode = 106; break;
				case SDL.Scancode.F4: ScanCode = 107; break;
				case SDL.Scancode.F5: ScanCode = 108; break;
				case SDL.Scancode.F6: ScanCode = 109; break;
				case SDL.Scancode.F7: ScanCode = 110; break;
				case SDL.Scancode.F8: ScanCode = 111; break;
				case SDL.Scancode.F9: ScanCode = 112; break;
				case SDL.Scancode.F10: ScanCode = 113; break;
				case SDL.Scancode.F11: ScanCode = 139; break;
				case SDL.Scancode.F12: ScanCode = 140; break;
				case SDL.Scancode.Grave: ScanCode = 41; break;
				case SDL.Scancode.Alpha1: ScanCode = 120; break;
				case SDL.Scancode.Alpha2: ScanCode = 121; break;
				case SDL.Scancode.Alpha3: ScanCode = 122; break;
				case SDL.Scancode.Alpha4: ScanCode = 123; break;
				case SDL.Scancode.Alpha5: ScanCode = 124; break;
				case SDL.Scancode.Alpha6: ScanCode = 125; break;
				case SDL.Scancode.Alpha7: ScanCode = 126; break;
				case SDL.Scancode.Alpha8: ScanCode = 127; break;
				case SDL.Scancode.Alpha9: ScanCode = 128; break;
				case SDL.Scancode.Alpha0: ScanCode = 129; break;
				case SDL.Scancode.Minus: ScanCode = 130; break;
				case SDL.Scancode.Equals: ScanCode = 131; break;
				case SDL.Scancode.Backspace: ScanCode = 14; break;
				case SDL.Scancode.Q: ScanCode = 16; break;
				case SDL.Scancode.W: ScanCode = 17; break;
				case SDL.Scancode.E: ScanCode = 18; break;
				case SDL.Scancode.R: ScanCode = 19; break;
				case SDL.Scancode.T: ScanCode = 20; break;
				case SDL.Scancode.Y: ScanCode = 21; break;
				case SDL.Scancode.U: ScanCode = 22; break;
				case SDL.Scancode.I: ScanCode = 23; break;
				case SDL.Scancode.O: ScanCode = 24; break;
				case SDL.Scancode.P: ScanCode = 25; break;
				case SDL.Scancode.Leftbracket: ScanCode = 26; break;
				case SDL.Scancode.Rightbracket: ScanCode = 27; break;
				case SDL.Scancode.Backslash: ScanCode = 43; break;
				case SDL.Scancode.A: ScanCode = 30; break;
				case SDL.Scancode.S: ScanCode = 31; break;
				case SDL.Scancode.D: ScanCode = 32; break;
				case SDL.Scancode.F: ScanCode = 33; break;
				case SDL.Scancode.G: ScanCode = 34; break;
				case SDL.Scancode.H: ScanCode = 35; break;
				case SDL.Scancode.J: ScanCode = 36; break;
				case SDL.Scancode.K: ScanCode = 37; break;
				case SDL.Scancode.L: ScanCode = 38; break;
				case SDL.Scancode.Semicolon: ScanCode = 39; break;
				case SDL.Scancode.Apostrophe: ScanCode = 40; break;
				case SDL.Scancode.Return: ScanCode = 41; break;
				case SDL.Scancode.Z: ScanCode = 44; break;
				case SDL.Scancode.X: ScanCode = 45; break;
				case SDL.Scancode.C: ScanCode = 46; break;
				case SDL.Scancode.V: ScanCode = 47; break;
				case SDL.Scancode.B: ScanCode = 48; break;
				case SDL.Scancode.N: ScanCode = 49; break;
				case SDL.Scancode.M: ScanCode = 50; break;
				case SDL.Scancode.Comma: ScanCode = 51; break;
				case SDL.Scancode.Period: ScanCode = 52; break;
				case SDL.Scancode.Slash: ScanCode = 53; break;
				case SDL.Scancode.Space: TextCharacter = ' '; break;
				case SDL.Scancode.Insert: ScanCode = 162; break;
				case SDL.Scancode.Delete: ScanCode = 163; break;
				case SDL.Scancode.Home: ScanCode = 151; break;
				case SDL.Scancode.End: ScanCode = 159; break;
				case SDL.Scancode.Pageup: ScanCode = 153; break;
				case SDL.Scancode.Pagedown: ScanCode = 161; break;
				case SDL.Scancode.KpDivide: ScanCode = 164; break;
				case SDL.Scancode.KpMultiply: ScanCode = 55; break;
				case SDL.Scancode.KpMinus: ScanCode = 74; break;
				case SDL.Scancode.KpPlus: ScanCode = 78; break;
				case SDL.Scancode.KpEnter: ScanCode = 166; break;
			}
		}
		else if (ctrlKey)
		{
			switch (sdlScanCode)
			{
				case SDL.Scancode.Escape: TextCharacter = (char)27; break;
				case SDL.Scancode.F1: ScanCode = 94; break;
				case SDL.Scancode.F2: ScanCode = 95; break;
				case SDL.Scancode.F3: ScanCode = 96; break;
				case SDL.Scancode.F4: ScanCode = 97; break;
				case SDL.Scancode.F5: ScanCode = 98; break;
				case SDL.Scancode.F6: ScanCode = 99; break;
				case SDL.Scancode.F7: ScanCode = 100; break;
				case SDL.Scancode.F8: ScanCode = 101; break;
				case SDL.Scancode.F9: ScanCode = 102; break;
				case SDL.Scancode.F10: ScanCode = 103; break;
				case SDL.Scancode.F11: ScanCode = 137; break;
				case SDL.Scancode.F12: ScanCode = 138; break;
				case SDL.Scancode.Alpha2: ScanCode = 3; break;
				case SDL.Scancode.Alpha6: TextCharacter = (char)30; break;
				case SDL.Scancode.Minus: TextCharacter = (char)31; break;
				case SDL.Scancode.Backspace: TextCharacter = (char)127; break;
				case SDL.Scancode.Tab: TextCharacter = (char)148; break;
				case SDL.Scancode.Q: TextCharacter = (char)17; break;
				case SDL.Scancode.W: TextCharacter = (char)23; break;
				case SDL.Scancode.E: TextCharacter = (char)5; break;
				case SDL.Scancode.R: TextCharacter = (char)18; break;
				case SDL.Scancode.T: TextCharacter = (char)20; break;
				case SDL.Scancode.Y: TextCharacter = (char)25; break;
				case SDL.Scancode.U: TextCharacter = (char)21; break;
				case SDL.Scancode.I: TextCharacter = (char)9; break;
				case SDL.Scancode.O: TextCharacter = (char)15; break;
				case SDL.Scancode.P: TextCharacter = (char)16; break;
				case SDL.Scancode.Leftbracket: TextCharacter = (char)27; break;
				case SDL.Scancode.Rightbracket:TextCharacter = (char)29; break;
				case SDL.Scancode.Backslash: TextCharacter = (char)28; break;
				case SDL.Scancode.A: TextCharacter = (char)1; break;
				case SDL.Scancode.S: TextCharacter = (char)19; break;
				case SDL.Scancode.D: TextCharacter = (char)4; break;
				case SDL.Scancode.F: TextCharacter = (char)6; break;
				case SDL.Scancode.G: TextCharacter = (char)7; break;
				case SDL.Scancode.H: TextCharacter = (char)8; break;
				case SDL.Scancode.J: TextCharacter = (char)10; break;
				case SDL.Scancode.K: TextCharacter = (char)11; break;
				case SDL.Scancode.L: TextCharacter = (char)12; break;
				case SDL.Scancode.Return: TextCharacter = (char)10; break;
				case SDL.Scancode.Z: TextCharacter = (char)26; break;
				case SDL.Scancode.X: TextCharacter = (char)24; break;
				case SDL.Scancode.C: TextCharacter = (char)3; break;
				case SDL.Scancode.V: TextCharacter = (char)22; break;
				case SDL.Scancode.B: TextCharacter = (char)2; break;
				case SDL.Scancode.N: TextCharacter = (char)14; break;
				case SDL.Scancode.M: TextCharacter = (char)13; break;
				case SDL.Scancode.Insert: ScanCode = 146; break;
				case SDL.Scancode.Delete: ScanCode = 147; break;
				case SDL.Scancode.Home: ScanCode = 119; break;
				case SDL.Scancode.End: ScanCode = 117; break;
				case SDL.Scancode.Pageup: ScanCode = 132; break;
				case SDL.Scancode.Pagedown: ScanCode = 118; break;
				case SDL.Scancode.KpDivide: ScanCode = 149; break;
				case SDL.Scancode.KpMultiply: ScanCode = 150; break;
				case SDL.Scancode.KpMinus: ScanCode = 142; break;
				case SDL.Scancode.Kp7: ScanCode = 119; break;
				case SDL.Scancode.Kp8: ScanCode = 141; break;
				case SDL.Scancode.Kp9: ScanCode = 132; break;
				case SDL.Scancode.KpPlus: ScanCode = 144; break;
				case SDL.Scancode.Kp4: ScanCode = 115; break;
				case SDL.Scancode.Kp5: ScanCode = 143; break;
				case SDL.Scancode.Kp6: ScanCode = 116; break;
				case SDL.Scancode.Kp1: ScanCode = 117; break;
				case SDL.Scancode.Kp2: ScanCode = 145; break;
				case SDL.Scancode.Kp3: ScanCode = 118; break;
				case SDL.Scancode.KpEnter: TextCharacter = (char)10; break;
				case SDL.Scancode.Kp0: ScanCode = 146; break;
				case SDL.Scancode.KpPeriod: ScanCode = 147; break;
			}
		}
		else if (shiftKey)
		{
			switch (sdlScanCode)
			{
				case SDL.Scancode.Escape: TextCharacter = (char)27; break;
				case SDL.Scancode.F1: ScanCode = 84; break;
				case SDL.Scancode.F2: ScanCode = 85; break;
				case SDL.Scancode.F3: ScanCode = 86; break;
				case SDL.Scancode.F4: ScanCode = 87; break;
				case SDL.Scancode.F5: ScanCode = 88; break;
				case SDL.Scancode.F6: ScanCode = 89; break;
				case SDL.Scancode.F7: ScanCode = 90; break;
				case SDL.Scancode.F8: ScanCode = 91; break;
				case SDL.Scancode.F9: ScanCode = 92; break;
				case SDL.Scancode.F10: ScanCode = 93; break;
				case SDL.Scancode.F11: ScanCode = 135; break;
				case SDL.Scancode.F12: ScanCode = 136; break;
				case SDL.Scancode.Grave: TextCharacter = '~'; break;
				case SDL.Scancode.Alpha1: TextCharacter = '!'; break;
				case SDL.Scancode.Alpha2: TextCharacter = '@'; break;
				case SDL.Scancode.Alpha3: TextCharacter = '#'; break;
				case SDL.Scancode.Alpha4: TextCharacter = '$'; break;
				case SDL.Scancode.Alpha5: TextCharacter = '%'; break;
				case SDL.Scancode.Alpha6: TextCharacter = '^'; break;
				case SDL.Scancode.Alpha7: TextCharacter = '&'; break;
				case SDL.Scancode.Alpha8: TextCharacter = '*'; break;
				case SDL.Scancode.Alpha9: TextCharacter = '('; break;
				case SDL.Scancode.Alpha0: TextCharacter = ')'; break;
				case SDL.Scancode.Minus: TextCharacter = '_'; break;
				case SDL.Scancode.Equals: TextCharacter = '+'; break;
				case SDL.Scancode.Backspace: TextCharacter = (char)8; break;
				case SDL.Scancode.Tab: ScanCode = 15; break;
				case SDL.Scancode.Q: TextCharacter = upperCase ? 'Q' : 'q'; break;
				case SDL.Scancode.W: TextCharacter = upperCase ? 'W' : 'w'; break;
				case SDL.Scancode.E: TextCharacter = upperCase ? 'E' : 'e'; break;
				case SDL.Scancode.R: TextCharacter = upperCase ? 'R' : 'r'; break;
				case SDL.Scancode.T: TextCharacter = upperCase ? 'T' : 't'; break;
				case SDL.Scancode.Y: TextCharacter = upperCase ? 'Y' : 'y'; break;
				case SDL.Scancode.U: TextCharacter = upperCase ? 'U' : 'u'; break;
				case SDL.Scancode.I: TextCharacter = upperCase ? 'I' : 'i'; break;
				case SDL.Scancode.O: TextCharacter = upperCase ? 'O' : 'o'; break;
				case SDL.Scancode.P: TextCharacter = upperCase ? 'P' : 'p'; break;
				case SDL.Scancode.Leftbracket: TextCharacter = '{'; break;
				case SDL.Scancode.Rightbracket: TextCharacter = '}'; break;
				case SDL.Scancode.Backslash: TextCharacter = '|'; break;
				case SDL.Scancode.A: TextCharacter = upperCase ? 'A' : 'a'; break;
				case SDL.Scancode.S: TextCharacter = upperCase ? 'S' : 's'; break;
				case SDL.Scancode.D: TextCharacter = upperCase ? 'D' : 'd'; break;
				case SDL.Scancode.F: TextCharacter = upperCase ? 'F' : 'f'; break;
				case SDL.Scancode.G: TextCharacter = upperCase ? 'G' : 'g'; break;
				case SDL.Scancode.H: TextCharacter = upperCase ? 'H' : 'h'; break;
				case SDL.Scancode.J: TextCharacter = upperCase ? 'J' : 'j'; break;
				case SDL.Scancode.K: TextCharacter = upperCase ? 'K' : 'k'; break;
				case SDL.Scancode.L: TextCharacter = upperCase ? 'L' : 'l'; break;
				case SDL.Scancode.Semicolon: TextCharacter = ':'; break;
				case SDL.Scancode.Apostrophe: TextCharacter = '"'; break;
				case SDL.Scancode.Return: TextCharacter = (char)13; break;
				case SDL.Scancode.Z: TextCharacter = upperCase ? 'Z' : 'z'; break;
				case SDL.Scancode.X: TextCharacter = upperCase ? 'X' : 'x'; break;
				case SDL.Scancode.C: TextCharacter = upperCase ? 'C' : 'c'; break;
				case SDL.Scancode.V: TextCharacter = upperCase ? 'V' : 'v'; break;
				case SDL.Scancode.B: TextCharacter = upperCase ? 'B' : 'b'; break;
				case SDL.Scancode.N: TextCharacter = upperCase ? 'N' : 'n'; break;
				case SDL.Scancode.M: TextCharacter = upperCase ? 'M' : 'm'; break;
				case SDL.Scancode.Comma: TextCharacter = '<'; break;
				case SDL.Scancode.Period: TextCharacter = '>'; break;
				case SDL.Scancode.Slash: TextCharacter = '?'; break;
				case SDL.Scancode.Space: TextCharacter = ' '; break;
				case SDL.Scancode.Insert: ScanCode = 82; break;
				case SDL.Scancode.Delete: ScanCode = 83; break;
				case SDL.Scancode.Home: ScanCode = 71; break;
				case SDL.Scancode.End: ScanCode = 79; break;
				case SDL.Scancode.Pageup: ScanCode = 73; break;
				case SDL.Scancode.Pagedown: ScanCode = 81; break;
				case SDL.Scancode.KpDivide: TextCharacter = '/'; break;
				case SDL.Scancode.KpMultiply: TextCharacter = '*'; break;
				case SDL.Scancode.KpMinus: TextCharacter = '-'; break;
				case SDL.Scancode.Kp7: TextCharacter = '7'; break;
				case SDL.Scancode.Kp8: TextCharacter = '8'; break;
				case SDL.Scancode.Kp9: TextCharacter = '9'; break;
				case SDL.Scancode.KpPlus: TextCharacter = '+'; break;
				case SDL.Scancode.Kp4: TextCharacter = '4'; break;
				case SDL.Scancode.Kp5: TextCharacter = '5'; break;
				case SDL.Scancode.Kp6: TextCharacter = '6'; break;
				case SDL.Scancode.Kp1: TextCharacter = '1'; break;
				case SDL.Scancode.Kp2: TextCharacter = '2'; break;
				case SDL.Scancode.Kp3: TextCharacter = '3'; break;
				case SDL.Scancode.KpEnter: TextCharacter = (char)13; break;
				case SDL.Scancode.Kp0: TextCharacter = '0'; break;
				case SDL.Scancode.KpPeriod: TextCharacter = '.'; break;
			}
		}
		else
		{
			switch (sdlScanCode)
			{
				case SDL.Scancode.Escape: TextCharacter = (char)27; break;
				case SDL.Scancode.F1: ScanCode = 59; break;
				case SDL.Scancode.F2: ScanCode = 60; break;
				case SDL.Scancode.F3: ScanCode = 61; break;
				case SDL.Scancode.F4: ScanCode = 62; break;
				case SDL.Scancode.F5: ScanCode = 63; break;
				case SDL.Scancode.F6: ScanCode = 64; break;
				case SDL.Scancode.F7: ScanCode = 65; break;
				case SDL.Scancode.F8: ScanCode = 66; break;
				case SDL.Scancode.F9: ScanCode = 67; break;
				case SDL.Scancode.F10: ScanCode = 68; break;
				case SDL.Scancode.F11: ScanCode = 133; break;
				case SDL.Scancode.F12: ScanCode = 134; break;
				case SDL.Scancode.Grave: TextCharacter = '`'; break;
				case SDL.Scancode.Alpha1: TextCharacter = '1'; break;
				case SDL.Scancode.Alpha2: TextCharacter = '2'; break;
				case SDL.Scancode.Alpha3: TextCharacter = '3'; break;
				case SDL.Scancode.Alpha4: TextCharacter = '4'; break;
				case SDL.Scancode.Alpha5: TextCharacter = '5'; break;
				case SDL.Scancode.Alpha6: TextCharacter = '6'; break;
				case SDL.Scancode.Alpha7: TextCharacter = '7'; break;
				case SDL.Scancode.Alpha8: TextCharacter = '8'; break;
				case SDL.Scancode.Alpha9: TextCharacter = '9'; break;
				case SDL.Scancode.Alpha0: TextCharacter = '0'; break;
				case SDL.Scancode.Minus: TextCharacter = '-'; break;
				case SDL.Scancode.Equals: TextCharacter = '='; break;
				case SDL.Scancode.Backspace: TextCharacter = (char)8; break;
				case SDL.Scancode.Tab: TextCharacter = '\t'; break;
				case SDL.Scancode.Q: TextCharacter = upperCase ? 'Q' : 'q'; break;
				case SDL.Scancode.W: TextCharacter = upperCase ? 'W' : 'w'; break;
				case SDL.Scancode.E: TextCharacter = upperCase ? 'E' : 'e'; break;
				case SDL.Scancode.R: TextCharacter = upperCase ? 'R' : 'r'; break;
				case SDL.Scancode.T: TextCharacter = upperCase ? 'T' : 't'; break;
				case SDL.Scancode.Y: TextCharacter = upperCase ? 'Y' : 'y'; break;
				case SDL.Scancode.U: TextCharacter = upperCase ? 'U' : 'u'; break;
				case SDL.Scancode.I: TextCharacter = upperCase ? 'I' : 'i'; break;
				case SDL.Scancode.O: TextCharacter = upperCase ? 'O' : 'o'; break;
				case SDL.Scancode.P: TextCharacter = upperCase ? 'P' : 'p'; break;
				case SDL.Scancode.Leftbracket: TextCharacter = '['; break;
				case SDL.Scancode.Rightbracket: TextCharacter = ']'; break;
				case SDL.Scancode.Backslash: TextCharacter = '\\'; break;
				case SDL.Scancode.A: TextCharacter = upperCase ? 'A' : 'a'; break;
				case SDL.Scancode.S: TextCharacter = upperCase ? 'S' : 's'; break;
				case SDL.Scancode.D: TextCharacter = upperCase ? 'D' : 'd'; break;
				case SDL.Scancode.F: TextCharacter = upperCase ? 'F' : 'f'; break;
				case SDL.Scancode.G: TextCharacter = upperCase ? 'G' : 'g'; break;
				case SDL.Scancode.H: TextCharacter = upperCase ? 'H' : 'h'; break;
				case SDL.Scancode.J: TextCharacter = upperCase ? 'J' : 'j'; break;
				case SDL.Scancode.K: TextCharacter = upperCase ? 'K' : 'k'; break;
				case SDL.Scancode.L: TextCharacter = upperCase ? 'L' : 'l'; break;
				case SDL.Scancode.Semicolon: TextCharacter = ';'; break;
				case SDL.Scancode.Apostrophe: TextCharacter = '\''; break;
				case SDL.Scancode.Return: TextCharacter = (char)13; break;
				case SDL.Scancode.Z: TextCharacter = upperCase ? 'Z' : 'z'; break;
				case SDL.Scancode.X: TextCharacter = upperCase ? 'X' : 'x'; break;
				case SDL.Scancode.C: TextCharacter = upperCase ? 'C' : 'c'; break;
				case SDL.Scancode.V: TextCharacter = upperCase ? 'V' : 'v'; break;
				case SDL.Scancode.B: TextCharacter = upperCase ? 'B' : 'b'; break;
				case SDL.Scancode.N: TextCharacter = upperCase ? 'N' : 'n'; break;
				case SDL.Scancode.M: TextCharacter = upperCase ? 'M' : 'm'; break;
				case SDL.Scancode.Comma: TextCharacter = ','; break;
				case SDL.Scancode.Period: TextCharacter = '.'; break;
				case SDL.Scancode.Slash: TextCharacter = '/'; break;
				case SDL.Scancode.Space: TextCharacter = ' '; break;
				case SDL.Scancode.Insert: ScanCode = 82; break;
				case SDL.Scancode.Delete: ScanCode = 83; break;
				case SDL.Scancode.Home: ScanCode = 71; break;
				case SDL.Scancode.End: ScanCode = 79; break;
				case SDL.Scancode.Pageup: ScanCode = 73; break;
				case SDL.Scancode.Pagedown: ScanCode = 81; break;
				case SDL.Scancode.KpDivide: TextCharacter = '/'; break;
				case SDL.Scancode.KpMultiply: TextCharacter = '*'; break;
				case SDL.Scancode.KpMinus: TextCharacter = '-'; break;
				case SDL.Scancode.Kp7: if (numLock) TextCharacter = '7'; else ScanCode = 71; break;
				case SDL.Scancode.Kp8: if (numLock) TextCharacter = '8'; else ScanCode = 72; break;
				case SDL.Scancode.Kp9: if (numLock) TextCharacter = '9'; else ScanCode = 73; break;
				case SDL.Scancode.KpPlus: TextCharacter = '+'; break;
				case SDL.Scancode.Kp4: if (numLock) TextCharacter = '4'; else ScanCode = 75; break;
				case SDL.Scancode.Kp5: if (numLock) TextCharacter = '5'; else ScanCode = 76; break;
				case SDL.Scancode.Kp6: if (numLock) TextCharacter = '6'; else ScanCode = 77; break;
				case SDL.Scancode.Kp1: if (numLock) TextCharacter = '1'; else ScanCode = 79; break;
				case SDL.Scancode.Kp2: if (numLock) TextCharacter = '2'; else ScanCode = 80; break;
				case SDL.Scancode.Kp3: if (numLock) TextCharacter = '3'; else ScanCode = 81; break;
				case SDL.Scancode.KpEnter: TextCharacter = (char)13; break;
				case SDL.Scancode.Kp0: if (numLock) TextCharacter = '0'; else ScanCode = 82; break;
				case SDL.Scancode.KpPeriod: if (numLock) TextCharacter = '.'; else ScanCode = 83; break;
			}
		}
	}
}
