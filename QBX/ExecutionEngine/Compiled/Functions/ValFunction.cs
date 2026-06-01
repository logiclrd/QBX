using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text.RegularExpressions;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public class ValFunction : Function
{
	public Evaluable? Argument;

	protected override void SetArgument(int index, Evaluable value)
	{
		if (!value.Type.IsString)
			throw CompilerException.TypeMismatch(value.Source);

		Argument = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref Argument);
	}

	public override DataType Type => DataType.Double;

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (Argument == null)
			throw new Exception("ValFunction with no Argument");

		var type = Argument.Type;

		StringVariable argumentValue;

		if (type.IsString)
			argumentValue = (StringVariable)Argument.Evaluate(context, stackFrame);
		else
			throw new Exception("Internal error");

		var parsedValue = PermissiveParse(argumentValue.ValueString);

		return new DoubleVariable(parsedValue);
	}

	static readonly Regex PermissiveParseRegex = new Regex(
		@"^[+-]?(\d+\.\d*|\.\d*|\d+)([dDeE][+-]?\d{1,3})?",
		RegexOptions.Singleline);

	static Dictionary<char, int> HexDigitMap =
		new Dictionary<char, int>()
		{
			{ '0', 0 },
			{ '1', 1 },
			{ '2', 2 },
			{ '3', 3 },
			{ '4', 4 },
			{ '5', 5 },
			{ '6', 6 },
			{ '7', 7 },
			{ '8', 8 },
			{ '9', 9 },
			{ 'A', 10 }, { 'a', 10 },
			{ 'B', 11 }, { 'b', 11 },
			{ 'C', 12 }, { 'c', 12 },
			{ 'D', 13 }, { 'd', 13 },
			{ 'E', 14 }, { 'e', 14 },
			{ 'F', 15 }, { 'f', 15 },
		};

	static Dictionary<char, int> OctalDigitMap =
		new Dictionary<char, int>()
		{
			{ '0', 0 },
			{ '1', 1 },
			{ '2', 2 },
			{ '3', 3 },
			{ '4', 4 },
			{ '5', 5 },
			{ '6', 6 },
			{ '7', 7 },
		};

	internal static double PermissiveParse(ReadOnlySpan<char> str)
	{
		while ((str.Length > 0) && char.IsWhiteSpace(str[0]))
			str = str.Slice(1);

		if (str.Length > 128)
			str = str.Slice(0, 128);

		if (str.StartsWith("&H") || str.StartsWith("&h")
		 || str.StartsWith("&O") || str.StartsWith("&o"))
		{
			double parsedValue = 0;

			if (str.Length > 2)
			{
				// Hexadecimal or octal
				int radix, bitsPerDigit;
				Dictionary<char, int> alphabet;

				switch (str[1])
				{
					case 'H':
					case 'h':
						radix = 16;
						bitsPerDigit = 4;
						alphabet = HexDigitMap;
						break;
					case 'O':
					case 'o':
						radix = 8;
						bitsPerDigit = 3;
						alphabet = OctalDigitMap;
						break;
					default:
						throw new Exception("Sanity failure");
				}

				if (alphabet.TryGetValue(str[2], out var digitValue))
				{
					int bitsAccumulated;

					if (radix == 8)
						bitsAccumulated = BitOperations.Log2(unchecked((uint)digitValue)) + 1;
					else
						bitsAccumulated = bitsPerDigit;

					int accumulator = digitValue;

					int i = 3;

					while (i < str.Length)
					{
						if (!alphabet.TryGetValue(str[i], out digitValue))
							break;

						accumulator = (accumulator << bitsPerDigit) | digitValue;
						bitsAccumulated += bitsPerDigit;

						if (bitsAccumulated + bitsPerDigit > 32)
						{
							// QuickBasic BUG: When parsing octal, if we max out the bits, then
							// the lower 16 bits shift in an extra digit that isn't there.
							if ((bitsPerDigit == 3)
							 && (i + 1 < str.Length)
							 && alphabet.ContainsKey(str[i + 1]))
							{
								const int FFFF0000 = unchecked((int)0xFFFF0000);

								accumulator =
									(accumulator & FFFF0000) |
									((accumulator & (0x0000FFFF >> 3)) << 3);
							}

							break;
						}

						i++;
					}

					if (bitsAccumulated <= 16)
						parsedValue = unchecked((short)accumulator); // truncate to 16-bit INTEGER
					else
						parsedValue = accumulator;
				}
			}

			return parsedValue;
		}
		else
		{
			// Try to parse a decimal number.
			foreach (var match in PermissiveParseRegex.EnumerateMatches(str))
			{
				var parseInput = str.Slice(match.Index, match.Length);

				int doubleExponentCharacterIndex = parseInput.IndexOf('d');

				if (doubleExponentCharacterIndex < 0)
					doubleExponentCharacterIndex = parseInput.IndexOf('D');

				if (doubleExponentCharacterIndex >= 0)
				{
					var clone = new char[parseInput.Length];

					parseInput.CopyTo(clone);
					parseInput = clone;

					clone[doubleExponentCharacterIndex] = 'E';
				}

				while ((parseInput.Length > 0) && (parseInput[0] == '0'))
					parseInput = parseInput.Slice(1);

				if (parseInput.Length == 0)
					return 0;

				if (double.TryParse(parseInput, out var parsedValue))
					return parsedValue;
			}
		}

		return 0;
	}
}
