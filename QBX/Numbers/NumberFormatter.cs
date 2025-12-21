using System.Text;

namespace QBX.Numbers;

public class NumberFormatter
{
	public static string Format(short value) => value.ToString();

	public static string FormatHex(short value) => "&H" + value.ToString("X");

	public static string FormatOctal(short value) => FormatOctal((int)value, forceLong: false);

	public static string Format(int value)
	{
		if ((value >= short.MinValue) && (value <= short.MaxValue))
			return value + "&";
		else
			return value.ToString();
	}

	public static string FormatHex(int value)
	{
		if ((value >= short.MinValue) && (value <= short.MaxValue))
			return value.ToString("X") + '&';
		else
			return value.ToString("X");
	}

	public static string FormatOctal(int value) => FormatOctal(value, forceLong: true);

	static string FormatOctal(int value, bool forceLong)
	{
		forceLong &= ((value >= short.MinValue) && (value <= short.MaxValue));

		var builder = new StringBuilder();

		builder.Append("&O");

		int radix = 1;

		while (value > radix)
			radix <<= 3;

		radix >>= 3;

		while (value > 0)
		{
			int digit = 0;

			while (value >= radix)
			{
				digit++;
				value -= radix;
			}

			builder.Append('0' + digit);

			radix >>= 3;
		}

		if (builder.Length == 2)
			builder.Append('0');

		if (forceLong)
			builder.Append('&');

		return builder.ToString();
	}

	public static string Format(float value)
	{
		if (value > 9999999f)
		{
			int exponent = 0;

			while (value >= 10)
			{
				value *= 0.1f;
				exponent++;
			}

			return value.ToString("#.#") + "E+" + exponent.ToString("00");
		}

		if (value < -9999999f)
		{
			int exponent = 0;

			while (value <= -10)
			{
				value *= 0.1f;
				exponent++;
			}

			return value.ToString("#.#") + "E+" + exponent.ToString("00");
		}

		if ((value > 0) && (value < 0.0000001))
		{
			int exponent = 0;

			while (value < 1)
			{
				value *= 10f;
				exponent--;
			}

			return value.ToString("#.#") + "E-" + exponent.ToString("00");
		}

		if ((value < 0) && (value > -0.0000001))
		{
			int exponent = 0;

			while (value > -1)
			{
				value *= 10f;
				exponent--;
			}

			return value.ToString("#.#") + "E-" + exponent.ToString("00");
		}

		string str = value.ToString("#.#");

		if (int.TryParse(str, out _))
			str += '!';

		return str;
	}

	public static string Format(double value)
	{
		if (value > 999999999999999d)
		{
			int exponent = 0;

			while (value >= 10)
			{
				value *= 0.1d;
				exponent++;
			}

			return value.ToString("#.#") + "D+" + exponent.ToString("00");
		}

		if (value < -999999999999999d)
		{
			int exponent = 0;

			while (value <= -10)
			{
				value *= 0.1f;
				exponent++;
			}

			return value.ToString("#.#") + "D+" + exponent.ToString("00");
		}

		if ((value > 0) && (value < 0.000000000000001))
		{
			int exponent = 0;

			while (value < 1)
			{
				value *= 10d;
				exponent--;
			}

			return value.ToString("#.#") + "D-" + exponent.ToString("00");
		}

		if ((value < 0) && (value > -0.000000000000001))
		{
			int exponent = 0;

			while (value > -1)
			{
				value *= 10d;
				exponent--;
			}

			return value.ToString("#.#") + "D-" + exponent.ToString("00");
		}

		string str = value.ToString("#.#");

		if (((float)value).ToString() == str)
			str += '#';

		return str;
	}

	public static string Format(decimal currencyValue)
	{
		currencyValue = currencyValue.Fix();

		return currencyValue.ToString("#.#") + '@';
	}
}
