using System.ComponentModel;
using System.Text;

namespace QBX.Numbers;

public class NumberFormatter
{
	public static string Format(short value) => value.ToString();

	public static string FormatHex(short value, bool includePrefix)
	{
		if (includePrefix)
			return "&H" + value.ToString("X");
		else
			return value.ToString("X");
	}

	public static string FormatOctal(short value, bool includePrefix) => FormatOctal(value, includePrefix, bits: 0xFFFF, forceLong: false);

	public static string Format(int value, bool qualify = true)
	{
		if (qualify && (value >= short.MinValue) && (value <= short.MaxValue))
			return value + "&";
		else
			return value.ToString();
	}

	public static string FormatHex(int value, bool includePrefix, bool qualify = true)
	{
		qualify &= ((value >= 0) && (value <= short.MaxValue));

		if (qualify)
		{
			if (includePrefix)
				return "&H" + value.ToString("X") + '&';
			else
				return value.ToString("X") + '&';
		}
		else
		{
			if (includePrefix)
				return "&H" + value.ToString("X");
			else
				return value.ToString("X");
		}
	}

	public static string FormatOctal(int value, bool includePrefix, bool qualify = true) => FormatOctal(value, includePrefix, bits: ~0, forceLong: qualify);

	static string FormatOctal(int value, bool includePrefix, int bits, bool forceLong)
	{
		forceLong &= ((value >= 0) && (value <= short.MaxValue));

		var builder = new StringBuilder();

		if (includePrefix)
			builder.Append("&O");

		int placeScale;
		int digitBits;

		int bitsForLargerPlaces = -1 << 3;

		placeScale = 0;

		value &= bits;

		while ((value & bitsForLargerPlaces) != 0)
		{
			bitsForLargerPlaces <<= 3;
			placeScale += 3;
		}

		digitBits = unchecked((int)((uint)bits >> placeScale)) & 0b111;

		while (placeScale >= 0)
		{
			int digit = (value >> placeScale) & digitBits;

			builder.Append(unchecked((char)('0' + digit)));

			placeScale -= 3;
			digitBits = 0b111;
		}

		if (forceLong)
			builder.Append('&');

		return builder.ToString();
	}

	public static string Format(float value, bool qualify = true)
	{
		string str = value.ToString();

		if (qualify)
		{
			if (int.TryParse(str, out _))
				str += '!';
		}

		return str;
	}

	public static string Format(double value, bool qualify = true)
	{
		string FormatBaseDigits(double adjustedValue)
		{
			string ret = adjustedValue.ToString("#.###############");

			if (ret.Length > 0)
				return ret;
			else
				return "0";
		}

		if (value > 999999999999999d)
		{
			int exponent = 0;

			while (value >= 10)
			{
				value *= 0.1d;
				exponent++;
			}

			return FormatBaseDigits(value) + "D+" + exponent.ToString("00");
		}

		if (value < -999999999999999d)
		{
			int exponent = 0;

			while (value <= -10)
			{
				value *= 0.1f;
				exponent++;
			}

			return FormatBaseDigits(value) + "D+" + exponent.ToString("00");
		}

		if ((value > 0) && (value < 0.000000000000001))
		{
			int exponent = 0;

			while (value < 1)
			{
				value *= 10d;
				exponent--;
			}

			return FormatBaseDigits(value) + "D-" + exponent.ToString("00");
		}

		if ((value < 0) && (value > -0.000000000000001))
		{
			int exponent = 0;

			while (value > -1)
			{
				value *= 10d;
				exponent--;
			}

			return FormatBaseDigits(value) + "D-" + exponent.ToString("00");
		}

		string str = FormatBaseDigits(value);

		if (qualify)
		{
			if (((float)value).ToString() == str)
				str += '#';
		}

		return str;
	}

	public static string Format(decimal currencyValue, bool qualify = true)
	{
		currencyValue = currencyValue.Fix();

		if (qualify)
			return currencyValue.ToString("#.#") + '@';
		else
			return currencyValue.ToString("#.#");
	}
}
