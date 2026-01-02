namespace QBX.Hardware;

public static class ScanCodeExtensions
{
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
}
