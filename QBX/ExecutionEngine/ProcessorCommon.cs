using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

using static QBX.Firmware.Fonts.CP437Encoding;

namespace QBX.ExecutionEngine;

public abstract class ProcessorCommon
{
	protected CodeModel.Statements.Statement? CurrentSource;

	protected static void Advance(ref Span<byte> i)
		=> i = i.Slice(1);

	protected static void SkipWhitespace(ref Span<byte> i)
	{
		while ((i.Length > 0) && ((i[0] == Space) || (i[0] == Tab)))
			i = i.Slice(1);
	}

	protected static void AdvanceAndSkipWhitespace(ref Span<byte> i)
	{
		Advance(ref i);
		SkipWhitespace(ref i);
	}

	[DoesNotReturn]
	protected Exception Fail() => throw RuntimeException.IllegalFunctionCall(CurrentSource);
	/*
	protected double ExpectNumber(ref Span<byte> input)
	{
		SkipWhitespace(ref input);

		if (input.Length == 0)
			Fail();

		double sign = 1;

		byte ch = input[0];

		while ((ch == Plus) || (ch == Minus))
		{
			if (ch == Minus)
				sign = -sign;

			Advance(ref input);
			if (input.Length == 0)
				Fail();

			ch = input[0];
		}

		bool haveDot = false;

		int numberLength;

		for (numberLength = 0; numberLength < input.Length; numberLength++)
		{
			ch = input[numberLength];

			if (ch == Dot)
			{
				if (haveDot)
					break;

				haveDot = true;
			}
			else if (!Encoding.IsDigit(ch))
				break;
		}

		if (!double.TryParse(input.Slice(0, numberLength), out var value))
			Fail();

		value *= sign;

		input = input.Slice(numberLength);

		return value;
	}
	*/
	protected void ExpectRange(int n, int min, int max)
	{
		if ((n < min) || (n > max))
			Fail();
	}

	protected int ExpectInteger(ref Span<byte> input, ExecutionContext? executionContext)
	{
		SkipWhitespace(ref input);

		if (input.Length == 0)
			Fail();

		if (input[0] == (byte)'=')
		{
			input = input.Slice(1);

			if (input.Length < 3)
				throw RuntimeException.IllegalFunctionCall();

			var descriptorBytes = input.Slice(0, 3);
			var descriptorSpan = MemoryMarshal.Cast<byte, SurfacedVariableDescriptor>(descriptorBytes);

			var descriptor = descriptorSpan[0];

			input = input.Slice(3);

			var surfaced = executionContext?.GetSurfacedVariable(descriptor.Key);

			if (surfaced == null)
				throw RuntimeException.IllegalFunctionCall();

			return surfaced.CoerceToInt(null);
		}

		byte ch = input[0];

		if (ch == EqualsSign)
		{
			Advance(ref input);

			if (input.Length < 3)
				Fail();

			var descriptorBytes = input.Slice(0, 3);
			var descriptorSpan = MemoryMarshal.Cast<byte, SurfacedVariableDescriptor>(descriptorBytes);

			var descriptor = descriptorSpan[0];

			input = input.Slice(3);

			var surfaced = executionContext?.GetSurfacedVariable(descriptor.Key);

			if (surfaced is null)
				Fail();

			return surfaced.CoerceToInt(context: null);
		}

		int sign = 1;

		while ((ch == Plus) || (ch == Minus))
		{
			if (ch == Minus)
				sign = -sign;

			Advance(ref input);
			if (input.Length == 0)
				Fail();

			ch = input[0];
		}

		if (!IsDigit(ch))
			Fail();

		int value = DigitValue(ch);
		int numDigits = 1;

		Advance(ref input);

		while (input.Length > 0)
		{
			ch = input[0];

			if (!IsDigit(ch))
				break;

			Advance(ref input);

			if (numDigits == 4)
				Fail();

			value = value * 10 + DigitValue(ch);
			numDigits++;
		}

		return sign * value;
	}

	protected int ExpectIntegerInRange(ref Span<byte> input, int min, int max, ExecutionContext? executionContext)
	{
		int value = ExpectInteger(ref input, executionContext);

		ExpectRange(value, min, max);

		return value;
	}

	protected const byte Space = 32;
	protected const byte Tab = 9;

	protected const byte A = (byte)'A';
	protected const byte B = (byte)'B';
	protected const byte C = (byte)'C';
	protected const byte D = (byte)'D';
	protected const byte E = (byte)'E';
	protected const byte F = (byte)'F';
	protected const byte G = (byte)'G';
	protected const byte H = (byte)'H';
	protected const byte I = (byte)'I';
	protected const byte J = (byte)'J';
	protected const byte K = (byte)'K';
	protected const byte L = (byte)'L';
	protected const byte M = (byte)'M';
	protected const byte N = (byte)'N';
	protected const byte O = (byte)'O';
	protected const byte P = (byte)'P';
	protected const byte Q = (byte)'Q';
	protected const byte R = (byte)'R';
	protected const byte S = (byte)'S';
	protected const byte T = (byte)'T';
	protected const byte U = (byte)'U';
	protected const byte V = (byte)'V';
	protected const byte W = (byte)'W';
	protected const byte X = (byte)'X';
	protected const byte Y = (byte)'Y';
	protected const byte Z = (byte)'Z';

	protected const byte LeftAngle = (byte)'<';
	protected const byte RightAngle = (byte)'>';

	protected const byte Sharp = (byte)'#';
	protected const byte Plus = (byte)'+';
	protected const byte Minus = (byte)'-';

	protected const byte Dot = (byte)'.';
	protected const byte Comma = (byte)',';

	protected const byte EqualsSign = (byte)'=';

	protected const double Sqrt2 = 1.414213562373095;
}
