using System;
using System.Collections.Generic;
using System.Text;

namespace QBX.Terminal;

public class DECGraphicsEncoding : Encoding
{
	static char[] s_byteToChar;
	static Dictionary<char, byte> s_charToByte;

	static DECGraphicsEncoding()
	{
		s_byteToChar = new char[256];

		s_byteToChar.AsSpan().Slice(1).Fill('\uFFFD'); // "Replacement Character"

		for (int i = 0; i < 95; i++)
			s_byteToChar[i] = (char)i;

		s_byteToChar[ 95] = ' ';
		s_byteToChar[ 96] = '♦';
		s_byteToChar[ 97] = '▒';
		s_byteToChar[ 98] = '␉';
		s_byteToChar[ 99] = '␌';
		s_byteToChar[100] = '␍';
		s_byteToChar[101] = '␊';
		s_byteToChar[102] = '°';
		s_byteToChar[103] = '±';
		s_byteToChar[104] = '␤';
		s_byteToChar[105] = '␋';
		s_byteToChar[106] = '┘';
		s_byteToChar[107] = '┐';
		s_byteToChar[108] = '┌';
		s_byteToChar[109] = '└';
		s_byteToChar[110] = '┼';
		s_byteToChar[111] = '⎺';
		s_byteToChar[112] = '⎻';
		s_byteToChar[113] = '─';
		s_byteToChar[114] = '⎼';
		s_byteToChar[115] = '⎽';
		s_byteToChar[116] = '├';
		s_byteToChar[117] = '┤';
		s_byteToChar[118] = '┴';
		s_byteToChar[119] = '┬';
		s_byteToChar[120] = '│';
		s_byteToChar[121] = '≤';
		s_byteToChar[122] = '≥';
		s_byteToChar[123] = 'π';
		s_byteToChar[124] = '≠';
		s_byteToChar[125] = '£';
		s_byteToChar[126] = '·';

		s_charToByte = new Dictionary<char, byte>();

		for (int i = 0; i < 256; i++)
			s_charToByte[s_byteToChar[i]] = unchecked((byte)i);
	}

	public override int GetByteCount(char[] chars, int index, int count) => count;

	public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		=> GetBytes(chars.AsSpan().Slice(charIndex, charCount), bytes.AsSpan().Slice(byteIndex));

	public override int GetBytes(ReadOnlySpan<char> chars, Span<byte> bytes)
	{
		if (chars.Length > bytes.Length)
			throw new ArgumentException("Insufficient space", nameof(bytes));

		for (int i = 0; i < chars.Length; i++)
			if (!s_charToByte.TryGetValue(chars[i], out bytes[i]))
				bytes[i] = (byte)'?';

		return chars.Length;
	}

	public override int GetCharCount(byte[] bytes, int index, int count) => count;

	public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		=> GetChars(bytes.AsSpan().Slice(byteIndex, byteCount), chars.AsSpan().Slice(charIndex));

	public override int GetChars(ReadOnlySpan<byte> bytes, Span<char> chars)
	{
		if (chars.Length > bytes.Length)
			throw new ArgumentException("Insufficient space", nameof(bytes));

		for (int i = 0; i < chars.Length; i++)
			chars[i] = s_byteToChar[bytes[i]];

		return bytes.Length;
	}

	public override int GetMaxByteCount(int charCount) => charCount;
	public override int GetMaxCharCount(int byteCount) => byteCount;
}
