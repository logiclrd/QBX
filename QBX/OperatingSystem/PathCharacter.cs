using System;
using System.Text;

namespace QBX.OperatingSystem;

public class PathCharacter
{
	public static bool IsSpace(byte b) => (b == ' ') || (b == '\0');

	static readonly byte[] InvalidCharacters = Encoding.ASCII.GetBytes("\"*+,./:;<=>?[\\]|");

	public static bool IsValid(byte b) => (b >= 32) && (Array.IndexOf(InvalidCharacters, b) < 0);

	public static bool IsValidOrWildcard(byte b) => (b == '?') || IsValid(b);

	public static bool IsLetter(byte b) =>
		((b >= 'A') && (b <= 'Z')) ||
		((b >= 'a') && (b <= 'z'));

	public static bool IsFileNameSeparator(byte b)
	{
		switch (b)
		{
			case (byte)':':
			//case (byte)'.': // documentation error: dot is never a filename separator, because ".TXT" is a valid filename
			case (byte)';':
			case (byte)',':
			case (byte)'=':
			case (byte)'+':
				return true;
		}

		return false;
	}

	public static byte ToUpper(byte b)
	{
		if ((b >= 'a') && (b <= 'z'))
		{
			b ^= 32;
			return b;
		}
		else
		{
			switch (b)
			{
				case 131: // â
				case 133: // à
				case 160: // á
					return 65; // A
				case 132: // ä
					return 142; // Ä
				case 134: // å
					return 143; // Å
				case 130: // é
				case 136: // ê
				case 137: // ë
				case 138: // è
					return 69; // E
				case 139: // ï
				case 140: // î
				case 141: // ì
				case 161: // í
					return 73; // I
				case 147: // ô
				case 149: // ò
				case 162: // ó
					return 79; // O
				case 148: // ö
					return 153; // Ö
				case 150: // û
				case 151: // ù
				case 163: // ú
					return 85; // U
				case 129: // ü
					return 154; // Ü
				case 152: // ÿ
					return 89; // Y
				case 164: // ñ
					return 165; // Ñ
				case 135: // ç
					return 128; // Ç
				case 145: // æ
					return 146; // Æ

				default:
					return b;
			}
		}
	}
}
