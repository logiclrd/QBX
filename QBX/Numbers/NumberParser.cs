using System;
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

		if (!char.IsAsciiHexDigit(lastChar) && (lastChar != '.'))
		{
			if (lastChar != '%')
				return false;

			chars = chars.Slice(0, chars.Length - 1);
		}

		int parsed;

		int sign = 1;

		while (chars[0] == '-')
		{
			sign = -sign;
			chars = chars.Slice(1);
		}

		if (chars.StartsWith("&H", StringComparison.OrdinalIgnoreCase))
		{
			if (!int.TryParse(chars.Slice(2), NumberStyles.HexNumber, default, out parsed))
				return false;

			if (parsed >= 32768)
				parsed -= 65536;
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

		if (!char.IsAsciiHexDigit(lastChar) && (lastChar != '.'))
		{
			if (lastChar != '&')
				return false;

			chars = chars.Slice(0, chars.Length - 1);
		}

		bool negate = false;

		while (chars[0] == '-')
		{
			negate = !negate;
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

			for (int i = 2; i < chars.Length; i++)
			{
				if (!char.IsAsciiDigit(chars[i]))
					return false;

				long parsedLong = (long)(value * 8) + (chars[i] - '0');

				if (parsedLong > int.MaxValue)
					return false;

				value = (int)parsedLong;
			}
		}
		else
		{
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

		float sign = +1f;

		while (chars[0] == '-')
		{
			sign = -sign;
			chars = chars.Slice(1);
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

		if (!float.TryParse(chars, out value))
			return false;

		value *= sign;

		return true;
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

		double sign = +1d;

		while (chars[0] == '-')
		{
			sign = -sign;
			chars = chars.Slice(1);
		}

		if (chars.Contains("d", StringComparison.OrdinalIgnoreCase))
		{
			// BASIC allows 'D' for the exponent in DOUBLE values.
			Span<char> translated = stackalloc char[chars.Length];

			for (int i = 0; i < chars.Length; i++)
			{
				if ((chars[i] == 'd') || (chars[i] == 'D'))
					translated[i] = 'e';
				else
					translated[i] = chars[i];
			}

			if (!double.TryParse(translated, out value))
				return false;
		}
		else
		{
			if (!double.TryParse(chars, out value))
				return false;
		}

		value *= sign;

		return true;
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

		bool negate = false;

		while (chars[0] == '-')
		{
			negate = !negate;
			chars = chars.Slice(1);
		}

		if (decimal.TryParse(chars, out value))
		{
			if (value.IsInCurrencyRange())
			{
				if (strictScale && value.IsTooPrecise())
					return false;

				value = value.Fix();

				if (negate)
					value = decimal.Negate(value);

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
