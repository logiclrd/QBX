using System;
using System.IO;

using QBX.LexicalAnalysis;
using QBX.Numbers;

using System.Diagnostics;

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

		if (string.IsNullOrEmpty(str))
			return false;

		if (TypeCharacter.TryParse(str[str.Length - 1], out var typeCharacter))
		{
			if (typeCharacter.Character != '%')
				return false;

			str = str.Remove(str.Length - 1);
		}

		return NumberParser.TryAsInteger(str, out value);
	}

	public bool TryAsLong(out int value)
	{
		value = default;

		string? str = Token?.Value;

		if (string.IsNullOrEmpty(str))
			return false;

		if (TypeCharacter.TryParse(str[str.Length - 1], out var typeCharacter))
		{
			if (typeCharacter.Character != '&')
				return false;

			str = str.Remove(str.Length - 1);
		}

		return NumberParser.TryAsLong(str, out value);
	}

	public bool TryAsSingle(out float value)
	{
		value = default;

		string? str = Token?.Value;

		if (string.IsNullOrEmpty(str))
			return false;

		if (TypeCharacter.TryParse(str[str.Length - 1], out var typeCharacter))
		{
			if (typeCharacter.Character != '!')
				return false;

			str = str.Remove(str.Length - 1);
		}

		return NumberParser.TryAsSingle(str, out value);
	}

	public bool TryAsDouble(out double value)
	{
		value = default;

		string? str = Token?.Value;

		if (string.IsNullOrEmpty(str))
			return false;

		if (TypeCharacter.TryParse(str[str.Length - 1], out var typeCharacter))
		{
			if (typeCharacter.Character != '#')
				return false;

			str = str.Remove(str.Length - 1);
		}

		return NumberParser.TryAsDouble(str, out value);
	}

	public bool TryAsCurrency(out decimal value)
	{
		value = default;

		string? str = Token?.Value;

		if (string.IsNullOrEmpty(str))
			return false;

		if (TypeCharacter.TryParse(str[str.Length - 1], out var typeCharacter))
		{
			if (typeCharacter.Character != '$')
				return false;

			str = str.Remove(str.Length - 1);
		}

		return NumberParser.TryAsCurrency(str, out value);
	}

	public bool IsStringLiteral => (Token != null) && (Token.Type == TokenType.String);
	public string StringLiteralValue => Token!.StringLiteralValue;

	public override void Render(TextWriter writer)
	{
		string? str = Token?.Value;

		if (string.IsNullOrEmpty(str))
			return;

		var chars = str.AsSpan();

		if (chars[0] == '-')
		{
			writer.Write('-');
			chars = chars.Slice(1);
		}

		if (str.StartsWith("\""))
		{
			writer.Write(str);
			if (!str.EndsWith("\""))
				writer.Write('"');
		}
		else if (str.StartsWith("&H", StringComparison.OrdinalIgnoreCase))
		{
			if (NumberParser.TryAsInteger(str, out var integerValue))
				writer.Write(NumberFormatter.FormatHex(integerValue, includePrefix: true));
			else if (NumberParser.TryAsLong(str, out var longValue))
				writer.Write(NumberFormatter.FormatHex(longValue, includePrefix: true));
			else
			{
				writer.Write(str);
				Debugger.Break();
			}
		}
		else if (str.StartsWith("&O", StringComparison.OrdinalIgnoreCase) || str.StartsWith("&O", StringComparison.OrdinalIgnoreCase))
		{
			if (NumberParser.TryAsInteger(str, out var integerValue))
				writer.Write(NumberFormatter.FormatOctal(integerValue, includePrefix: true));
			else if (NumberParser.TryAsLong(str, out var longValue))
				writer.Write(NumberFormatter.FormatOctal(longValue, includePrefix: true));
			else
			{
				writer.Write(str);
				Debugger.Break();
			}
		}
		else
		{
			if (TypeCharacter.TryParse(str[str.Length - 1], out var typeCharacter))
			{
				string unqualified = str.Remove(str.Length - 1);

				switch (typeCharacter.Character)
				{
					case '%':
						if (NumberParser.TryAsInteger(unqualified, out var integerValue))
							writer.Write(NumberFormatter.Format(integerValue));
						break;
					case '&':
						if (NumberParser.TryAsLong(unqualified, out var longValue))
							writer.Write(NumberFormatter.Format(longValue));
						break;
					case '!':
						if (NumberParser.TryAsSingle(unqualified, out var singleValue))
							writer.Write(NumberFormatter.Format(singleValue));
						break;
					case '#':
						if (NumberParser.TryAsDouble(unqualified, out var doubleValue))
							writer.Write(NumberFormatter.Format(doubleValue));
						break;
					case '@':
						if (NumberParser.TryAsCurrency(unqualified, out var currencyValue))
							writer.Write(NumberFormatter.Format(currencyValue));
						break;
				}
			}
			else if (NumberParser.TryAsInteger(str, out var integerValue))
				writer.Write(NumberFormatter.Format(integerValue));
			else if (NumberParser.TryAsLong(str, out var longValue))
				writer.Write(NumberFormatter.Format(longValue));
			else if (NumberParser.TryAsSingle(str, out var singleValue))
				writer.Write(NumberFormatter.Format(singleValue));
			else if (NumberParser.TryAsDouble(str, out var doubleValue))
				writer.Write(NumberFormatter.Format(doubleValue));
			else if (NumberParser.TryAsCurrency(str, out var currencyValue))
				writer.Write(NumberFormatter.Format(currencyValue));
			else
			{
				writer.Write(str);
				Debugger.Break();
			}
		}
	}
}
