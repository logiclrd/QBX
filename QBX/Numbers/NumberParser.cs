using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;

namespace QBX.Numbers;

public static class NumberParser
{
	public static bool TryAsInteger(string str, out short value)
	{
		value = default;

		if (char.IsSymbol(str.Last()))
			return (str.Last() == '%');

		int parsed;

		var chars = str.AsSpan();

		int sign = 1;

		if (chars[0] == '-')
		{
			sign = -1;
			chars = chars.Slice(1);
		}

		if (chars.StartsWith("&H", StringComparison.OrdinalIgnoreCase))
		{
			if (!int.TryParse(chars.Slice(2), NumberStyles.HexNumber, default, out parsed))
				return false;
		}
		else if (chars.StartsWith("&O", StringComparison.OrdinalIgnoreCase))
		{
			parsed = 0;

			for (int i = 2; i < chars.Length; i++)
			{
				if (!char.IsAsciiDigit(chars[i]))
					return false;

				parsed = (parsed * 8) + (chars[i] - '0');

				if (parsed > short.MaxValue)
					return false;
			}
		}
		else
		{
			sign = 1;
			if (!int.TryParse(str, out parsed))
				return false;
		}

		parsed *= sign;

		if ((parsed >= short.MinValue) && (parsed <= short.MaxValue))
		{
			value = (short)parsed;
			return true;
		}

		return false;
	}

	public static bool TryAsLong(string str, out int value)
	{
		value = default;

		if (char.IsSymbol(str.Last()))
		{
			if (str.Last() != '&')
				return false;

			str = str.Remove(str.Length - 1);
		}

		var chars = str.AsSpan();

		bool negate = false;

		if (chars[0] == '-')
		{
			negate = true;
			chars = chars.Slice(1);
		}

		if (chars.StartsWith("&H", StringComparison.OrdinalIgnoreCase))
		{
			if (!int.TryParse(chars.Slice(2), NumberStyles.HexNumber, default, out value))
				return false;
		}
		else if (chars.StartsWith("&O", StringComparison.OrdinalIgnoreCase))
		{
			value = 0;

			for (int i = 2; i < str.Length; i++)
			{
				if (!char.IsAsciiDigit(str[i]))
					return false;

				long parsedLong = (long)(value * 8) + (str[i] - '0');

				if (parsedLong > int.MaxValue)
					return false;

				value = (int)parsedLong;
			}
		}
		else
		{
			negate = false;
			if (!int.TryParse(str, out value))
				return false;
		}

		if (negate)
			value = unchecked(1 + ~value);

		return true;
	}

	public static bool TryAsSingle(string str, out float value)
	{
		value = default;

		if (char.IsSymbol(str.Last()))
		{
			if (str.Last() != '!')
				return false;

			str = str.Remove(str.Length - 1);
		}

		var trimmed = str.Replace(".", "").Trim('0');

		int significantFigures = trimmed.Length;

		if (significantFigures > 7)
			return false;

		return float.TryParse(str, out value);
	}

	public static bool TryAsDouble(string str, out double value)
	{
		value = default;

		if (char.IsSymbol(str.Last()))
		{
			if (str.Last() != '#')
				return false;

			str = str.Remove(str.Length - 1);
		}

		return double.TryParse(str, out value);
	}

	public static bool TryAsCurrency(string str, out decimal value)
	{
		value = default;

		if (char.IsSymbol(str.Last()))
		{
			if (str.Last() != '@')
				return false;

			str = str.Remove(str.Length - 1);
		}

		if (decimal.TryParse(str, out value))
		{
			if (value.IsInCurrencyRange())
			{
				value = value.Fix();
				return true;
			}
		}

		return false;
	}

	public static bool TryParse(string valueString, [NotNullWhen(true)] out object? value)
	{
		if (TryAsInteger(valueString, out var integerValue))
		{
			value = integerValue;
			return true;
		}

		if (TryAsLong(valueString, out var longValue))
		{
			value = longValue;
			return true;
		}

		if (TryAsSingle(valueString, out var singleValue))
		{
			value = singleValue;
			return true;
		}

		if (TryAsDouble(valueString, out var doubleValue))
		{
			value = doubleValue;
			return true;
		}

		if (TryAsCurrency(valueString, out var currencyValue))
		{
			value = currencyValue;
			return true;
		}

		value = default;
		return false;
	}
}
