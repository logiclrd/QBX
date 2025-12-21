using QBX.LexicalAnalysis;
using System.Globalization;

namespace QBX.CodeModel.Expressions;

public class LiteralExpression : Expression
{
	public LiteralExpression(Token token)
	{
		Token = token;
	}

	public bool TryAsInteger(out short value)
	{
		value = default;

		string? str = Token?.Value;

		if (str == null)
			return false;

		if (char.IsSymbol(str.Last()))
			return (str.Last() == '%');

		int parsed;

		if (str.StartsWith("&H", StringComparison.OrdinalIgnoreCase))
		{
			if (!int.TryParse(str.Substring(2), NumberStyles.HexNumber, default, out parsed))
				return false;
		}
		else if (str.StartsWith("&O", StringComparison.OrdinalIgnoreCase))
		{
			parsed = 0;

			for (int i = 2; i < str.Length; i++)
			{
				if (!char.IsAsciiDigit(str[i]))
					return false;

				parsed = (parsed * 8) + (str[i] - '0');

				if (parsed > short.MaxValue)
					return false;
			}
		}
		else
		{
			if (!int.TryParse(str, out parsed))
				return false;
		}

		if ((parsed >= short.MinValue) && (parsed <= short.MaxValue))
		{
			value = (short)parsed;
			return true;
		}

		return false;
	}

	public bool TryAsLong(out int value)
	{
		value = default;

		string? str = Token?.Value;

		if (str == null)
			return false;

		if (char.IsSymbol(str.Last()))
		{
			if (str.Last() != '%')
				return false;

			str = str.Remove(str.Length - 1);
		}

		if (str.StartsWith("&H", StringComparison.OrdinalIgnoreCase))
		{
			if (!int.TryParse(str.Substring(2), NumberStyles.HexNumber, default, out value))
				return false;
		}
		else if (str.StartsWith("&O", StringComparison.OrdinalIgnoreCase))
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
			if (!int.TryParse(str, out value))
				return false;
		}

		return true;
	}

	public override void Render(TextWriter writer)
	{
		// TODO
	}
}
