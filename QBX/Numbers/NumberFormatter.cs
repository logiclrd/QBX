using System;
using System.Text;

using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.LexicalAnalysis;

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
		if (value == 0)
			return qualify ? "0!" : "0";

		string str;

		string lead = "";

		if (value < 0)
		{
			lead = "-";
			value = -value;
		}

		if (value < 1)
		{
			int exponent = 0;

			for (int i = 0; i <= 38 - 7; i++)
			{
				float t = MathF.Pow(10, -i);

				if (value >= t)
				{
					value /= t;
					exponent = i;
					break;
				}
			}

			string s = value.ToString("0.#######");

			if (s.Length == 17)
				s = s.Remove(s.Length - 1);

			if (s.Length + exponent <= 8)
			{
				if (s.Length > 1)
					s = s.Remove(1, 1);
				str = lead + "." + new string('0', exponent - 1) + s;
			}
			else
				str = lead + s + "E-" + exponent.ToString("d2");
		}
		else if (value > 9999999)
		{
			int exponent = -1;

			for (int i = 7; i < 38; i++)
			{
				float t = MathF.Pow(10, i);

				if (value < t)
				{
					t = MathF.Pow(10, i - 1);
					value /= t;
					exponent = i - 1;
					break;
				}
			}

			string s = value.ToString("0.#######");

			if (s.Length == 9)
				s = s.Remove(s.Length - 1);

			if (exponent < 0)
				str = lead + s;
			else
				str = lead + s + "E+" + exponent.ToString("d2");
		}
		else
			str = lead + value.ToString("#######.#######");

		if (qualify)
		{
			if (long.TryParse(str, out _))
				str += '!';
		}

		return str;
	}

	public static string Format(double value, bool qualify = true)
	{
		if (value == 0)
			return qualify ? "0#" : "0";

		string str;

		string lead = "";

		if (value < 0)
		{
			lead = "-";
			value = -value;
		}

		if (value < 1)
		{
			int exponent = 0;

			for (int i = 0; i <= 308 - 15; i++)
			{
				double t = Math.Pow(10, -i);

				if (value >= t)
				{
					value /= t;
					exponent = i;
					break;
				}
			}

			string s = value.ToString("0.###############");

			if (s.Length == 17)
				s = s.Remove(s.Length - 1);

			if (s.Length + exponent <= 17)
			{
				if (s.Length > 1)
					s = s.Remove(1, 1);
				str = lead + "." + new string('0', exponent - 1) + s;
			}
			else
			{
				str = lead + s + "D-" + exponent.ToString("d2");
				qualify = false;
			}
		}
		else if (value > 999999999999999)
		{
			int exponent = -1;

			for (int i = 15; i < 308; i++)
			{
				double t = Math.Pow(10, i);

				if (value < t)
				{
					t = Math.Pow(10, i - 1);
					value /= t;
					exponent = i - 1;
					break;
				}
			}

			string s = value.ToString("0.###############");

			if (s.Length == 17)
				s = s.Remove(s.Length - 1);

			if (exponent < 0)
				str = lead + s;
			else
			{
				str = lead + s + "D+" + exponent.ToString("d2");
				qualify = false;
			}
		}
		else
			str = lead + value.ToString("###############.###############");

		if (qualify)
		{
			int digits = str.Length;

			if (str.IndexOf('.') >= 0)
				digits--;

			if (digits <= 7)
				str += '#';
		}

		return str;
	}

	public static string Format(decimal currencyValue, bool qualify = true)
	{
		currencyValue = currencyValue.Fix();

		string formatString;

		if (currencyValue == 0)
			formatString = "0";
		else
			formatString = "#.####";

		if (qualify)
			return currencyValue.ToString(formatString) + '@';
		else
			return currencyValue.ToString(formatString);
	}

	public static string Format(Variable value, bool qualify = true, CodeModel.Expressions.Expression? expression = null)
	{
		switch (value)
		{
			case IntegerVariable integerValue: return Format(integerValue.Value);
			case LongVariable longValue: return Format(longValue.Value, qualify);
			case SingleVariable singleValue: return Format(singleValue.Value, qualify);
			case DoubleVariable doubleValue: return Format(doubleValue.Value, qualify);
			case CurrencyVariable currencyValue: return Format(currencyValue.Value, qualify);

			default:
				throw RuntimeException.TypeMismatch(expression);
		}
	}
}
