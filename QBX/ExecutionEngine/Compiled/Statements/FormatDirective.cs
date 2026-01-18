using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using QBX.CodeModel.Statements;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class FormatDirective
{
	public virtual bool UsesArgument => false;

	public static void SplitString(string format, Func<FormatDirective, bool> yield)
		=> SplitString(format, null, yield);

	public static void SplitString(string format, CodeModel.Statements.Statement? statement, Func<FormatDirective, bool> yield)
	{
		// Number formats:
		//   leading? pattern exponent? trailingminus?
		//
		//   leading:
		//     (nothing)
		//     $$   (leading $)
		//     **   (left fill with '*')
		//     **$  ($$ and ** combined)
		//
		//   pattern:
		//     ####
		//     ###.
		//     ##.#
		//     .###
		//     or also can be omitted if leading is present
		//
		//   exponent:
		//     '^^^^' ("E+01")
		//     '^^^^^' ("E+001")
		//     if present forces the number to exponential representation
		//     'D' is used in place of 'E' if the number is DOUBLE
		//
		//   trailingminus: '-', if present moves any negative sign to be trailing
		//
		// If leading is specified, the characters are treated like '#' characters as
		// part of pattern in addition to their special meaning.
		//
		// If exponent is present, then a leading '.' on pattern is treated as
		// literal (not part of the format directive).
		//
		// If exponent is present, then the number is scaled to fit the available format
		// string first. The "E+01" (or D or -) exponent value isn't considered when
		// checking the formatted length.
		//
		// If the formatted length exceeds the number of characters in pattern,
		// then an extra '%' is emitted before the formatted text.
		//
		// If trailingminus is specified, a character is always emitted. It is a space
		// for positive numbers. If the number is negative, the minus sign does not count
		// toward the length check.
		//
		// -----------------
		//
		// String formats:
		//    &      Prints an entire string argument
		//    !      Prints only the first character, or a space if the string is empty
		//    \  \   Delineates a field graphically. Prints the as much of the string as
		//           will fit into the field (example here is 4 chars), padding with
		//           spaces as needed.
		//
		// -----------------
		//
		// Literals:
		//
		//    _      Escapes the following character to always be a literal.
		//    Anything not matched by other rules is a literal.

		var formatChars = format.AsSpan();

		while (formatChars.Length > 0)
		{
			switch (formatChars[0])
			{
				case '$':
				{
					if ((formatChars.Length == 1) || (formatChars[1] != '$'))
					{
						if (!yield(new LiteralFormatDirective("$")))
							return;

						formatChars = formatChars.Slice(1);
					}
					else
					{
						formatChars = formatChars.Slice(2);
						if (!yield(ExtractNumberFormatDirective(ref formatChars, "00", leadingDollarSign: true)))
							return;
					}

					break;
				}
				case '*':
				{
					if ((formatChars.Length == 1) || (formatChars[1] != '*'))
					{
						if (!yield(new LiteralFormatDirective("*")))
							return;

						formatChars = formatChars.Slice(1);
					}
					else
					{
						formatChars = formatChars.Slice(2);

						if ((formatChars.Length > 0) && (formatChars[0] == '$'))
						{
							formatChars = formatChars.Slice(1);
							if (!yield(ExtractNumberFormatDirective(ref formatChars, "000", leftPadChar: '*', leadingDollarSign: true)))
								return;
						}
						else
						{
							if (!yield(ExtractNumberFormatDirective(ref formatChars, "00", leftPadChar: '*')))
								return;
						}
					}

					break;
				}
				case '#':
				case '.':
				{
					string patternSoFar = formatChars[0].ToString();
					bool haveDecimal = formatChars[0] == '.';

					formatChars = formatChars.Slice(1);

					if (!yield(ExtractNumberFormatDirective(ref formatChars, patternSoFar, haveDecimal: haveDecimal)))
						return;

					break;
				}
				case ',':
				{
					formatChars = formatChars.Slice(1);

					if (!yield(ExtractNumberFormatDirective(ref formatChars, "", separateThousands: true)))
						return;

					break;
				}
				case '&':
				{
					formatChars = formatChars.Slice(1);
					if (!yield(new StringFormatDirective()))
						return;

					break;
				}
				case '!':
				{
					formatChars = formatChars.Slice(1);
					if (!yield(new StringFormatDirective(fieldWidth: 1)))
						return;

					break;
				}
				case '\\':
				{
					int fieldWidth = 1;

					while (formatChars[fieldWidth] != '\\')
					{
						if (formatChars[fieldWidth] != ' ')
							throw RuntimeException.IllegalFunctionCall(statement);

						fieldWidth++;
					}

					fieldWidth++;

					formatChars = formatChars.Slice(fieldWidth);

					if (!yield(new StringFormatDirective(fieldWidth)))
						return;

					break;
				}
				case '_':
				{
					formatChars = formatChars.Slice(1);

					char literalCharacter = '_';

					if (formatChars.Length > 0)
					{
						literalCharacter = formatChars[0];
						formatChars = formatChars.Slice(1);
					}

					if (!yield(new LiteralFormatDirective(literalCharacter.ToString())))
						return;

					break;
				}

				default:
				{
					var substring = new StringBuilder();

					substring.Append(formatChars[0]);
					formatChars = formatChars.Slice(1);

					while (formatChars.Length > 0)
					{
						bool isSpecial =
							formatChars[0] switch
							{
								'$' or '*' or '#' or '.' => true, // numeric
								'&' or '!' or '\\' => true, // string
								'_' => true, // escape

								_ => false,
							};

						if (isSpecial)
							break;

						substring.Append(formatChars[0]);
						formatChars = formatChars.Slice(1);
					}

					if (!yield(new LiteralFormatDirective(substring.ToString())))
						return;

					break;
				}
			}
		}
	}

	static FormatDirective ExtractNumberFormatDirective(ref ReadOnlySpan<char> formatChars, string patternSoFar, char leftPadChar = ' ', bool leadingDollarSign = false, bool separateThousands = false, bool haveDecimal = false)
	{
		// Pattern
		var pattern = new StringBuilder(patternSoFar);

		while (formatChars.Length > 0)
		{
			if (formatChars[0] == '.')
			{
				if (haveDecimal)
					return new NumericFormatDirective(pattern.ToString(), leftPadChar, leadingDollarSign, separateThousands, exponentCharacters: 0, trailingMinusSign: false);

				pattern.Append('.');
				haveDecimal = true;
			}
			else if (formatChars[0] == ',')
			{
				pattern.Append('0');
				separateThousands = true;
			}
			else if (formatChars[0] == '#')
				pattern.Append('0');
			else
				break;

			formatChars = formatChars.Slice(1);
		}

		// Exponential Notation: ^^^^ or ^^^^^

		int exponentCharacters = 0;

		if ((formatChars.Length >= 5) && formatChars.Slice(0, 5).SequenceEqual("^^^^^"))
		{
			formatChars = formatChars.Slice(5);
			exponentCharacters = 5;
		}
		else if ((formatChars.Length >= 4) && formatChars.Slice(0, 4).SequenceEqual("^^^^"))
		{
			formatChars = formatChars.Slice(4);
			exponentCharacters = 4;
		}

		// Trailing Minus

		bool trailingMinusSign = false;

		if ((formatChars.Length > 0) && (formatChars[0] == '-'))
		{
			formatChars = formatChars.Slice(1);
			trailingMinusSign = true;
		}

		return new NumericFormatDirective(pattern.ToString(), leftPadChar, leadingDollarSign, separateThousands, exponentCharacters, trailingMinusSign);
	}

	public abstract void Emit(Variable argument, ExecutionContext context, CodeModel.Statements.PrintStatement? statement);
}

public class LiteralFormatDirective(string literalText) : FormatDirective
{
	public override void Emit(Variable argument, ExecutionContext context, CodeModel.Statements.PrintStatement? statement)
	{
		context.VisualLibrary.WriteText(literalText);
	}
}

public class NumericFormatDirective(string pattern, char leftPadChar, bool leadingDollarSign, bool separateThousands, int exponentCharacters, bool trailingMinusSign) : FormatDirective
{
	public override bool UsesArgument => true;

	public override void Emit(Variable argument, ExecutionContext context, PrintStatement? statement)
	{
		switch (argument)
		{
			case IntegerVariable integerValue: Emit((decimal)integerValue.Value, context.VisualLibrary); break;
			case LongVariable longValue: Emit((decimal)longValue.Value, context.VisualLibrary); break;
			case SingleVariable singleValue: Emit(singleValue.Value, 'E', context.VisualLibrary); break;
			case DoubleVariable doubleValue: Emit(doubleValue.Value, 'D', context.VisualLibrary); break;
			case CurrencyVariable currencyValue: Emit(currencyValue.Value, context.VisualLibrary); break;

			default: throw RuntimeException.TypeMismatch(statement);
		}
	}

	[ThreadStatic]
	static Dictionary<string, string>? s_patternsWithThousandsSeparators;

	internal void Emit(double value, char exponentLetter, VisualLibrary visual)
	{
		bool isNegative = (value < 0);

		if (trailingMinusSign)
			value = Math.Abs(value);

		int exponentValue = 0;

		if (exponentCharacters > 0)
		{
			double targetRangeHigh = 1d;
			double targetRangeLow = 0.1d;

			int leftOfDecimal = pattern.IndexOf('.');

			if (leftOfDecimal < 0)
			{
				targetRangeHigh *= 10d;
				targetRangeLow *= 10d;

				leftOfDecimal = pattern.Length - 1;
			}

			int rightOfDecimal = pattern.Length - leftOfDecimal - 1;

			for (int i = trailingMinusSign ? 1 : 2; i <= leftOfDecimal; i++)
			{
				targetRangeHigh *= 10d;
				targetRangeLow *= 10d;
			}

			while ((value >= targetRangeHigh) || (value <= -targetRangeHigh))
			{
				exponentValue++;
				value *= 0.1d;
			}

			while (((value > 0) && (value < targetRangeLow))
			    || ((value > -targetRangeLow) && (value < 0)))
			{
				exponentValue--;
				value *= 10d;
			}

			string checkForRounding = value.ToString(pattern).TrimStart('0');

			int maxDigits = pattern.IndexOf('.');

			if (maxDigits < 0)
				maxDigits = pattern.Length;

			if (!trailingMinusSign)
				maxDigits--;

			if (checkForRounding.IndexOf('.') > maxDigits)
			{
				exponentValue++;
				value *= 0.1d;
			}
		}

		if (separateThousands && (Math.Abs(value) >= 1000))
		{
			s_patternsWithThousandsSeparators ??= new Dictionary<string, string>();

			if (!s_patternsWithThousandsSeparators.TryGetValue(pattern, out var alteredPattern))
			{
				int decimalOffset = pattern.IndexOf('.');

				if (decimalOffset < 0)
					decimalOffset = pattern.Length - 1;

				if (decimalOffset <= 3)
					alteredPattern = "0,0";
				else
				{
					char[] patternCharacters = pattern.ToCharArray();

					for (int commaOffset = decimalOffset - 3; commaOffset > 0; commaOffset -= 3)
						patternCharacters[commaOffset] = ',';

					alteredPattern = new string(patternCharacters);

					s_patternsWithThousandsSeparators[pattern] = alteredPattern;
				}
			}

			pattern = alteredPattern;
		}

		string formatted = value.ToString(pattern);

		char[] formattedChars = formatted.ToCharArray();

		int numberStart = formatted.Length - 1;

		for (int i = 0; i + 1 < formatted.Length; i++)
		{
			if ((formatted[i] >= '1') && (formatted[i] <= '9'))
			{
				numberStart = i;
				break;
			}

			if (formatted[i] == '.')
			{
				numberStart = i;

				if (numberStart > 0)
				{
					numberStart--;
					formattedChars[numberStart] = '0';
				}

				break;
			}

			formattedChars[i] = leftPadChar;
		}

		if (leadingDollarSign)
		{
			if (numberStart == 0)
				visual.WriteText("%$");
			else
				formattedChars[numberStart - 1] = '$';
		}
		else if (formatted.Length > pattern.Length)
			visual.WriteText('%');

		for (int i = formattedChars.Length; i < pattern.Length; i++)
			visual.WriteText(leftPadChar);

		visual.WriteText(formattedChars);

		if (exponentCharacters > 0)
		{
			visual.WriteText(exponentLetter);

			if (exponentValue >= 0)
				visual.WriteText('+');
			else
				visual.WriteText('-');

			exponentValue = Math.Abs(exponentValue);

			if ((exponentCharacters == 4) && (exponentValue >= 100))
			{
				visual.WriteText('%');
				visual.WriteText((exponentValue %= 10).ToString());
			}
			else
			{
				visual.WriteText(exponentValue.ToString(
					exponentCharacters == 4 ? "d2" : "d3"));
			}
		}

		if (trailingMinusSign)
			visual.WriteText(isNegative ? '-' : ' ');
	}

	internal void Emit(decimal value, VisualLibrary visual)
	{
		bool isNegative = (value < 0);

		value = Math.Abs(value);

		int exponentValue = 0;

		var rawDigits = decimal.Round(value * 10000M).ToString().ToList();

		if (value == 0)
			rawDigits.AddRange("000");

		int decimalPosition = rawDigits.Count - 4;

		if (exponentCharacters > 0)
		{
			decimal targetRangeHigh = 1M;
			decimal targetRangeLow = 0.1M;

			int leftOfDecimal = pattern.IndexOf('.');

			if (leftOfDecimal < 0)
			{
				targetRangeHigh *= 10M;
				targetRangeLow *= 10M;

				leftOfDecimal = pattern.Length - 1;
			}

			for (int i = trailingMinusSign ? 1 : 2; i <= leftOfDecimal; i++)
			{
				targetRangeHigh *= 10M;
				targetRangeLow *= 10M;
			}

			while ((value >= targetRangeHigh) || (value <= -targetRangeHigh))
			{
				exponentValue++;
				decimalPosition--;
				value *= 0.1M;
			}

			while (((value > 0) && (value < targetRangeLow))
			    || ((value > -targetRangeLow) && (value < 0)))
			{
				exponentValue--;
				decimalPosition++;
				value *= 10M;
			}
		}

		int patternDecimalPosition = pattern.IndexOf('.');

		if (patternDecimalPosition < 0)
			patternDecimalPosition = pattern.Length;

		if (isNegative && !trailingMinusSign)
		{
			rawDigits.Insert(0, '-');
			decimalPosition++;
		}

		if (separateThousands)
		{
			for (int commaPosition = decimalPosition - 3; commaPosition > 0; commaPosition -= 3)
			{
				rawDigits.Insert(commaPosition, ',');
				decimalPosition++;
			}
		}

		var rawDigitsSpan = CollectionsMarshal.AsSpan(rawDigits);

		var firstTruncatedDigitPosition = (patternDecimalPosition < pattern.Length)
			? decimalPosition + (pattern.Length - patternDecimalPosition) - 1
			: decimalPosition + 1;

		if ((firstTruncatedDigitPosition < rawDigits.Count)
		 && (rawDigits[firstTruncatedDigitPosition] >= '5'))
		{
			int i = firstTruncatedDigitPosition - 1;

			while ((i >= 0) && (rawDigits[i] == '9'))
			{
				rawDigits[i] = '0';
				i--;

				if ((i >= 0) && (rawDigits[i] == '.'))
					i--;
			}

			if (i >= 0)
				rawDigits[i]++;
			else
			{
				rawDigits.Insert(0, '1');
				if (exponentCharacters > 0)
					exponentValue++;
				else
					decimalPosition++;
			}
		}

		if (leadingDollarSign)
		{
			rawDigits.Insert(0, '$');
			decimalPosition++;
		}

		if (decimalPosition > patternDecimalPosition)
			visual.WriteText('%');

		if (decimalPosition == 0)
		{
			for (int i = 1; i < patternDecimalPosition; i++)
				visual.WriteText(leftPadChar);

			visual.WriteText('0');
		}
		else
		{
			for (int i = decimalPosition; i < patternDecimalPosition; i++)
				visual.WriteText(leftPadChar);

			visual.WriteText(rawDigitsSpan.Slice(0, decimalPosition));
		}

		if (patternDecimalPosition < pattern.Length)
		{
			visual.WriteText('.');

			for (int i = patternDecimalPosition + 1, j = decimalPosition; i < pattern.Length; i++, j++)
			{
				if (j < rawDigitsSpan.Length)
					visual.WriteText(rawDigitsSpan[j]);
				else
					visual.WriteText('0');
			}
		}

		if (exponentCharacters > 0)
		{
			visual.WriteText("E");

			if (exponentValue >= 0)
				visual.WriteText('+');
			else
				visual.WriteText('-');

			exponentValue = Math.Abs(exponentValue);

			if ((exponentCharacters == 4) && (exponentValue >= 100))
			{
				visual.WriteText('%');
				visual.WriteText((exponentValue %= 10).ToString());
			}
			else
			{
				visual.WriteText(exponentValue.ToString(
					exponentCharacters == 4 ? "00" : "000"));
			}
		}

		if (trailingMinusSign)
			visual.WriteText(isNegative ? '-' : ' ');
	}
}

public class StringFormatDirective(int fieldWidth = -1) : FormatDirective
{
	public override bool UsesArgument => true;

	[ThreadStatic]
	static string? s_spaces;

	public override void Emit(Variable argument, ExecutionContext context, PrintStatement? statement)
	{
		if (argument is not StringVariable stringArgument)
			throw RuntimeException.TypeMismatch(statement);

		var str = stringArgument.ValueSpan;

		if (fieldWidth < 0)
			context.VisualLibrary.WriteText(str);
		else if (str.Length > fieldWidth)
			context.VisualLibrary.WriteText(str.Slice(0, fieldWidth));
		else
		{
			int padding = fieldWidth - str.Length;

			if ((s_spaces == null) || (s_spaces.Length < padding))
				s_spaces = new string(' ', padding * 2);

			context.VisualLibrary.WriteText(str);
			context.VisualLibrary.WriteText(s_spaces.AsSpan().Slice(0, padding));
		}
	}
}
