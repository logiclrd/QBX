using QBX.Firmware.Fonts;

namespace QBX.Utility;

public static class CharExtensions
{
	public static bool IsWordCharacter(this char ch)
	{
		switch (ch)
		{
			case '!':
			case '#':
			case '$':
			case '%':
			case '&':
			case '.':
			case '/':
			case '@':
				return true;
			default:
				byte v = CP437Encoding.GetByteSemantic(ch);

				return CP437Encoding.IsLetterOrDigit(v);
		}
	}
}
