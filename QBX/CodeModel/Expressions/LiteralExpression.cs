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

		if (str == null)
			return false;

		return NumberParser.TryAsInteger(str, out value);
	}

	public bool TryAsLong(out int value)
	{
		value = default;

		string? str = Token?.Value;

		if (str == null)
			return false;

		return NumberParser.TryAsLong(str, out value);
	}

	public bool TryAsSingle(out float value)
	{
		value = default;

		string? str = Token?.Value;

		if (str == null)
			return false;

		return NumberParser.TryAsSingle(str, out value);
	}

	public bool TryAsDouble(out double value)
	{
		value = default;

		string? str = Token?.Value;

		if (str == null)
			return false;

		return NumberParser.TryAsDouble(str, out value);
	}

	public bool TryAsCurrency(out decimal value)
	{
		value = default;

		string? str = Token?.Value;

		if (str == null)
			return false;

		return NumberParser.TryAsCurrency(str, out value);
	}

	public bool IsStringLiteral => (Token != null) && (Token.Type == TokenType.String);
	public string StringLiteralValue => Token!.StringLiteralValue;

	public override void Render(TextWriter writer)
	{
		string? str = Token?.Value;

		if (str == null)
			return;

		if (str.StartsWith("\""))
		{
			writer.Write(str);
			if (!str.EndsWith("\""))
				writer.Write('"');
		}
		else if (str.StartsWith("&H", StringComparison.OrdinalIgnoreCase))
		{
			if (NumberParser.TryAsInteger(str, out var integerValue))
				writer.Write(NumberFormatter.FormatHex(integerValue));
			else if (NumberParser.TryAsLong(str, out var longValue))
				writer.Write(NumberFormatter.FormatHex(longValue));
			else
			{
				writer.Write(str);
				Debugger.Break();
			}
		}
		else if (str.StartsWith("&O", StringComparison.OrdinalIgnoreCase))
		{
			if (NumberParser.TryAsInteger(str, out var integerValue))
				writer.Write(NumberFormatter.FormatOctal(integerValue));
			else if (NumberParser.TryAsLong(str, out var longValue))
				writer.Write(NumberFormatter.FormatOctal(longValue));
			else
			{
				writer.Write(str);
				Debugger.Break();
			}
		}
		else
		{
			if (NumberParser.TryAsInteger(str, out var integerValue))
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
