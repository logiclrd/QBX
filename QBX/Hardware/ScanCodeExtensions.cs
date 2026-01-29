namespace QBX.Hardware;

public static class ScanCodeExtensions
{
	public static char ToCharacter(this ScanCode scanCode)
	{
		switch (scanCode)
		{
			case ScanCode._1: return '1';
			case ScanCode._2: return '2';
			case ScanCode._3: return '3';
			case ScanCode._4: return '4';
			case ScanCode._5: return '5';
			case ScanCode._6: return '6';
			case ScanCode._7: return '7';
			case ScanCode._8: return '8';
			case ScanCode._9: return '9';
			case ScanCode._0: return '0';
			case ScanCode.Minus: return '-';
			case ScanCode.Equals: return '=';
			case ScanCode.Q: return 'Q';
			case ScanCode.W: return 'W';
			case ScanCode.E: return 'E';
			case ScanCode.R: return 'R';
			case ScanCode.T: return 'T';
			case ScanCode.Y: return 'Y';
			case ScanCode.U: return 'U';
			case ScanCode.I: return 'I';
			case ScanCode.O: return 'O';
			case ScanCode.P: return 'P';
			case ScanCode.LeftBracket: return '[';
			case ScanCode.RightBracket: return ']';
			case ScanCode.A: return 'A';
			case ScanCode.S: return 'S';
			case ScanCode.D: return 'D';
			case ScanCode.F: return 'F';
			case ScanCode.G: return 'G';
			case ScanCode.H: return 'H';
			case ScanCode.J: return 'J';
			case ScanCode.K: return 'K';
			case ScanCode.L: return 'L';
			case ScanCode.Grave: return '`';
			case ScanCode.Semicolon: return ';';
			case ScanCode.Apostrophe: return '\'';
			case ScanCode.Backslash: return '\\';
			case ScanCode.Z: return 'Z';
			case ScanCode.X: return 'X';
			case ScanCode.C: return 'C';
			case ScanCode.V: return 'V';
			case ScanCode.B: return 'B';
			case ScanCode.N: return 'N';
			case ScanCode.M: return 'M';
			case ScanCode.Comma: return ',';
			case ScanCode.Period: return '.';
			case ScanCode.Slash: return '/';
			case ScanCode.KpMultiply: return '*';
			case ScanCode.KpMinus: return '-';
			case ScanCode.Kp5: return '5';
			case ScanCode.KpPlus: return '+';
			case ScanCode.Alt1: return '1';
			case ScanCode.Alt2: return '2';
			case ScanCode.Alt3: return '3';
			case ScanCode.Alt4: return '4';
			case ScanCode.Alt5: return '5';
			case ScanCode.Alt6: return '6';
			case ScanCode.Alt7: return '7';
			case ScanCode.Alt8: return '8';
			case ScanCode.Alt9: return '9';
			case ScanCode.Alt0: return '0';
			case ScanCode.AltMinus: return '-';
			case ScanCode.AltEquals: return '=';
			case ScanCode.CtrlKpMinus: return '-';
			case ScanCode.CtrlKp5: return '5';
			case ScanCode.CtrlKpPlus: return '+';
			case ScanCode.CtrlKpDivide: return '/';
			case ScanCode.CtrlKpMultiply: return '*';
			case ScanCode.AltKpDivide: return '/';

			default: return default;
		}
	}

	public static string ToCharacterString(this ScanCode scanCode)
	{
		switch (scanCode)
		{
			case ScanCode._1: return "1";
			case ScanCode._2: return "2";
			case ScanCode._3: return "3";
			case ScanCode._4: return "4";
			case ScanCode._5: return "5";
			case ScanCode._6: return "6";
			case ScanCode._7: return "7";
			case ScanCode._8: return "8";
			case ScanCode._9: return "9";
			case ScanCode._0: return "0";
			case ScanCode.Minus: return "-";
			case ScanCode.Equals: return "=";
			case ScanCode.Q: return "Q";
			case ScanCode.W: return "W";
			case ScanCode.E: return "E";
			case ScanCode.R: return "R";
			case ScanCode.T: return "T";
			case ScanCode.Y: return "Y";
			case ScanCode.U: return "U";
			case ScanCode.I: return "I";
			case ScanCode.O: return "O";
			case ScanCode.P: return "P";
			case ScanCode.LeftBracket: return "[";
			case ScanCode.RightBracket: return "]";
			case ScanCode.A: return "A";
			case ScanCode.S: return "S";
			case ScanCode.D: return "D";
			case ScanCode.F: return "F";
			case ScanCode.G: return "G";
			case ScanCode.H: return "H";
			case ScanCode.J: return "J";
			case ScanCode.K: return "K";
			case ScanCode.L: return "L";
			case ScanCode.Grave: return "`";
			case ScanCode.Semicolon: return ";";
			case ScanCode.Apostrophe: return "'";
			case ScanCode.Backslash: return "\\";
			case ScanCode.Z: return "Z";
			case ScanCode.X: return "X";
			case ScanCode.C: return "C";
			case ScanCode.V: return "V";
			case ScanCode.B: return "B";
			case ScanCode.N: return "N";
			case ScanCode.M: return "M";
			case ScanCode.Comma: return ",";
			case ScanCode.Period: return ".";
			case ScanCode.Slash: return "/";
			case ScanCode.KpMultiply: return "*";
			case ScanCode.KpMinus: return "-";
			case ScanCode.Kp5: return "5";
			case ScanCode.KpPlus: return "+";
			case ScanCode.Alt1: return "1";
			case ScanCode.Alt2: return "2";
			case ScanCode.Alt3: return "3";
			case ScanCode.Alt4: return "4";
			case ScanCode.Alt5: return "5";
			case ScanCode.Alt6: return "6";
			case ScanCode.Alt7: return "7";
			case ScanCode.Alt8: return "8";
			case ScanCode.Alt9: return "9";
			case ScanCode.Alt0: return "0";
			case ScanCode.AltMinus: return "-";
			case ScanCode.AltEquals: return "=";
			case ScanCode.CtrlKpMinus: return "-";
			case ScanCode.CtrlKp5: return "5";
			case ScanCode.CtrlKpPlus: return "+";
			case ScanCode.CtrlKpDivide: return "/";
			case ScanCode.CtrlKpMultiply: return "*";
			case ScanCode.AltKpDivide: return "/";

			default: return "";
		}
	}

	public static ScanCode GetScanCode(this char ch, out bool shifted)
	{
		shifted = char.IsAsciiLetterUpper(ch);

		switch (char.ToUpperInvariant(ch))
		{
			case '1': return ScanCode._1;
			case '!': shifted = true; goto case '1';
			case '2': return ScanCode._2;
			case '@': shifted = true; goto case '2';
			case '3': return ScanCode._3;
			case '#': shifted = true; goto case '3';
			case '4': return ScanCode._4;
			case '$': shifted = true; goto case '4';
			case '5': return ScanCode._5;
			case '%': shifted = true; goto case '5';
			case '6': return ScanCode._6;
			case '^': shifted = true; goto case '6';
			case '7': return ScanCode._7;
			case '&': shifted = true; goto case '7';
			case '8': return ScanCode._8;
			case '*': shifted = true; goto case '8';
			case '9': return ScanCode._9;
			case '(': shifted = true; goto case '9';
			case '0': return ScanCode._0;
			case ')': shifted = true; goto case '0';
			case '-': return ScanCode.Minus;
			case '_': shifted = true; goto case '-';
			case '=': return ScanCode.Equals;
			case '+': shifted = true; goto case '=';
			case 'Q': return ScanCode.Q;
			case 'W': return ScanCode.W;
			case 'E': return ScanCode.E;
			case 'R': return ScanCode.R;
			case 'T': return ScanCode.T;
			case 'Y': return ScanCode.Y;
			case 'U': return ScanCode.U;
			case 'I': return ScanCode.I;
			case 'O': return ScanCode.O;
			case 'P': return ScanCode.P;
			case '[': return ScanCode.LeftBracket;
			case '{': shifted = true; goto case '[';
			case ']': return ScanCode.RightBracket;
			case '}': shifted = true; goto case ']';
			case 'A': return ScanCode.A;
			case 'S': return ScanCode.S;
			case 'D': return ScanCode.D;
			case 'F': return ScanCode.F;
			case 'G': return ScanCode.G;
			case 'H': return ScanCode.H;
			case 'J': return ScanCode.J;
			case 'K': return ScanCode.K;
			case 'L': return ScanCode.L;
			case '`': return ScanCode.Grave;
			case '~': shifted = true; goto case '`';
			case ';': return ScanCode.Semicolon;
			case ':': shifted = true; goto case ';';
			case '\'': return ScanCode.Apostrophe;
			case '"': shifted = true; goto case '\'';
			case '\\': return ScanCode.Backslash;
			case '|': shifted = true; goto case '\\';
			case 'Z': return ScanCode.Z;
			case 'X': return ScanCode.X;
			case 'C': return ScanCode.C;
			case 'V': return ScanCode.V;
			case 'B': return ScanCode.B;
			case 'N': return ScanCode.N;
			case 'M': return ScanCode.M;
			case ',': return ScanCode.Comma;
			case '<': shifted = true; goto case ',';
			case '.': return ScanCode.Period;
			case '>': shifted = true; goto case '.';
			case '/': return ScanCode.Slash;
			case '?': shifted = true; goto case '/';

			default: return ScanCode.None;
		}
	}
}
