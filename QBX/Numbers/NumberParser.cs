using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

using QBX.Utility;

namespace QBX.Numbers;

public static class NumberParser
{
	public static bool TryAsInteger(ReadOnlySpan<char> chars, out short value)
	{
		value = default;

		if (chars.Length == 0)
			return true;

		char lastChar = chars[chars.Length - 1];

		if (!char.IsAsciiDigit(lastChar) && (lastChar != '.'))
		{
			if (lastChar != '%')
				return false;

			chars = chars.Slice(0, chars.Length - 1);
		}

		int parsed;

		int sign = 1;

		var noNegativeChars = chars;

		if (chars[0] == '-')
		{
			sign = -1;
			noNegativeChars = chars.Slice(1);
		}

		if (noNegativeChars.StartsWith("&H", StringComparison.OrdinalIgnoreCase))
		{
			if (!int.TryParse(noNegativeChars.Slice(2), NumberStyles.HexNumber, default, out parsed))
				return false;
		}
		else if (noNegativeChars.StartsWith("&O", StringComparison.OrdinalIgnoreCase))
		{
			parsed = 0;

			for (int i = 2; i < noNegativeChars.Length; i++)
			{
				if (!char.IsAsciiDigit(noNegativeChars[i]))
					return false;

				parsed = (parsed * 8) + (noNegativeChars[i] - '0');

				if (parsed > short.MaxValue)
					return false;
			}
		}
		else
		{
			sign = 1;
			if (!int.TryParse(chars, out parsed))
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

	public static bool TryAsLong(ReadOnlySpan<char> chars, out int value)
	{
		value = default;

		if (chars.Length == 0)
			return true;

		char lastChar = chars[chars.Length - 1];

		if (!char.IsAsciiDigit(lastChar) && (lastChar != '.'))
		{
			if (lastChar != '&')
				return false;

			chars = chars.Slice(0, chars.Length - 1);
		}

		bool negate = false;

		var noNegativeChars = chars;

		if (chars[0] == '-')
		{
			negate = true;
			noNegativeChars = chars.Slice(1);
		}

		if (noNegativeChars.StartsWith("&H", StringComparison.OrdinalIgnoreCase))
		{
			if (!int.TryParse(noNegativeChars.Slice(2), NumberStyles.HexNumber, default, out value))
				return false;
		}
		else if (noNegativeChars.StartsWith("&O", StringComparison.OrdinalIgnoreCase))
		{
			value = 0;

			for (int i = 2; i < noNegativeChars.Length; i++)
			{
				if (!char.IsAsciiDigit(noNegativeChars[i]))
					return false;

				long parsedLong = (long)(value * 8) + (noNegativeChars[i] - '0');

				if (parsedLong > int.MaxValue)
					return false;

				value = (int)parsedLong;
			}
		}
		else
		{
			negate = false;
			if (!int.TryParse(chars, out value))
				return false;
		}

		if (negate)
			value = unchecked(1 + ~value);

		return true;
	}

	public static bool TryAsSingle(ReadOnlySpan<char> chars, out float value)
	{
		value = default;

		if (chars.Length == 0)
			return true;

		char lastChar = chars[chars.Length - 1];
		bool coerce = false;

		if (!char.IsAsciiDigit(lastChar) && (lastChar != '.'))
		{
			if (lastChar != '!')
				return false;

			coerce = true;
			chars = chars.Slice(0, chars.Length - 1);
		}

		if (!coerce)
		{
			var trimmed = new StringBuilder(new string(chars));

			int e = trimmed.IndexOf('e', caseSensitive: false);

			if (e > 0)
				trimmed.Remove(e, trimmed.Length - e);

			int dot = trimmed.IndexOf('.');

			if (dot < 0)
				trimmed.Append('.');

			while (trimmed[trimmed.Length - 1] == '0')
				trimmed.Length--;
			while (trimmed[0] == '-')
				trimmed.Remove(0, 1);
			while (trimmed[0] == '0')
				trimmed.Remove(0, 1);

			int significantFigures = trimmed.Length - 1; // always contains a dot, even if it's the last character

			if (significantFigures > 7)
				return false;
		}

		return float.TryParse(chars, out value);
	}

	public static bool TryAsDouble(ReadOnlySpan<char> chars, out double value)
	{
		value = default;

		if (chars.Length == 0)
			return true;

		char lastChar = chars[chars.Length - 1];

		if (!char.IsAsciiDigit(lastChar) && (lastChar != '.'))
		{
			if (lastChar != '#')
				return false;

			chars = chars.Slice(0, chars.Length - 1);
		}

		return double.TryParse(chars, out value);
	}

	public static bool TryAsCurrency(ReadOnlySpan<char> chars, out decimal value)
	{
		value = default;

		if (chars.Length == 0)
			return true;

		char lastChar = chars[chars.Length - 1];

		bool strictScale = false;

		if (!char.IsAsciiDigit(lastChar) && (lastChar != '.'))
		{
			if (lastChar != '@')
				return false;

			strictScale = true;
			chars = chars.Slice(0, chars.Length - 1);
		}

		if (decimal.TryParse(chars, out value))
		{
			if (value.IsInCurrencyRange())
			{
				if (strictScale && value.IsTooPrecise())
					return false;

				value = value.Fix();
				return true;
			}
		}

		return false;
	}

	public static bool TryParse(ReadOnlySpan<char> valueString, [NotNullWhen(true)] out object? value)
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
