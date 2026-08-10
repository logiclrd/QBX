using QBX.Firmware.Fonts;

namespace QBX.Utility;

public static class ByteExtensions
{
	public static bool IsWordCharacter(this byte ch)
	{
		switch (ch)
		{
			case (byte)'!':
			case (byte)'#':
			case (byte)'$':
			case (byte)'%':
			case (byte)'&':
			case (byte)'.':
			case (byte)'/':
			case (byte)'@':
				return true;
			default:
				return CP437Encoding.IsLetterOrDigit(ch);
		}
	}
}
