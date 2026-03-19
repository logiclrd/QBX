using System;
using System.Text;

using QBX.Firmware.Fonts;

namespace QBX.OperatingSystem;

public class ShortPath
{
	public const char VolumeSeparatorChar = ':';

	public static bool IsDriveLetter(string path)
	{
		return (path.Length == 2) && char.IsAsciiLetter(path[0]) && (path[1] == VolumeSeparatorChar);
	}

	public static bool HasDriveLetter(string path)
	{
		return TryGetDriveLetter(path, out _);
	}

	public static char GetDriveLetter(string path)
	{
		TryGetDriveLetter(path, out var driveLetter);

		return driveLetter;
	}

	public static bool TryGetDriveLetter(ReadOnlySpan<char> path, out char driveLetter)
	{
		if ((path.Length >= 2)
		 && char.IsAsciiLetter(path[0])
		 && (path[1] == VolumeSeparatorChar))
		{
			driveLetter = char.ToUpperInvariant(path[0]);
			return true;
		}
		else
		{
			driveLetter = 'C'; // "C:/" synthetic drive on platforms with no drive letters
			return false;
		}
	}

	public static bool TryGetDriveLetter(ReadOnlySpan<byte> path, out byte driveLetter)
	{
		if ((path.Length >= 2)
		 && CP437Encoding.IsAsciiLetter(path[0])
		 && (path[1] == VolumeSeparatorChar))
		{
			driveLetter = CP437Encoding.ToUpper(path[0]);
			return true;
		}
		else
		{
			driveLetter = (byte)'C'; // "C:/" synthetic drive on platforms with no drive letters
			return false;
		}
	}

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

	public static readonly char[] DirectorySeparators = ['\\', '/'];

	public static bool IsDirectorySeparator(char ch)
		=> (ch == '\\') || (ch == '/');

	public static bool IsDirectorySeparator(byte ch)
		=> (ch == (byte)'\\') || (ch == (byte)'/');

	public static string? GetDirectoryName(ReadOnlySpan<char> path)
	{
		int lastSeparator = path.LastIndexOfAny(DirectorySeparators);

		while ((lastSeparator > 0) && DirectorySeparators.Contains(path[lastSeparator - 1]))
			lastSeparator--;

		if (lastSeparator < 0)
			return null;
		else
		{
			if ((lastSeparator == 2) && (path[1] == VolumeSeparatorChar))
				lastSeparator++;

			return new string(path.Slice(0, lastSeparator));
		}
	}

	public static string GetFileName(ReadOnlySpan<char> path)
	{
		int lastSeparator = path.LastIndexOfAny(DirectorySeparators);

		return new string(path.Slice(lastSeparator + 1));
	}

	public static string Join(string? left, string? right)
	{
		if ((left == null) && (right == null))
			return "";
		else if (string.IsNullOrWhiteSpace(left))
			return right!;
		else if (string.IsNullOrWhiteSpace(right))
			return left;
		else
			return left + DirectorySeparators[0] + right;
	}

	public static string Join(ReadOnlySpan<string?> components)
	{
		var builder = new StringBuilder();

		for (int i=0; i < components.Length; i++)
		{
			if (!string.IsNullOrWhiteSpace(components[i]))
			{
				if (builder.Length > 0)
					builder.Append(DirectorySeparators[0]);

				builder.Append(components[i]);
			}
		}

		return builder.ToString();
	}

	public static bool EqualsCaseInsensitive(string left, string right)
	{
		if (left.Length != right.Length)
			return false;

		for (int i=0; i < left.Length; i++)
		{
			char leftCh = left[i];
			char rightCh = right[i];

			if (leftCh != rightCh)
			{
				if (IsDirectorySeparator(leftCh) && IsDirectorySeparator(rightCh))
					continue;

				byte leftByte = CP437Encoding.GetByteSemantic(leftCh);
				byte rightByte = CP437Encoding.GetByteSemantic(rightCh);

				if (ToUpper(leftByte) == ToUpper(rightByte))
					continue;

				return false;
			}
		}

		return true;
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
