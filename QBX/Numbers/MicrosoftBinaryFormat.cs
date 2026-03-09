using System;

namespace QBX.Numbers;

public class MicrosoftBinaryFormat
{
	// The 32-bit MBF representation has a lot of bits in the same place as the IEEE form.
	//
	//   IEEE: seeeeeee emmmmmmm mmmmmmmm mmmmmmmm
	//   MBF:  eeeeeeee smmmmmmm mmmmmmmm mmmmmmmm
	//
	// Differences:
	// - The sign bit needs to shift to the right of the exponent.
	// - The exponent is biased slightly differently, the MBF
	//   exponent's value is larger than the IEEE exponent by 2
	//   for the same number.

	public static byte[] GetBytes(float value)
	{
		// Get the IEEE bytes
		byte[] bytes = BitConverter.GetBytes(value);

		// Shuffle bits
		int exponent = unchecked((byte)((bytes[2] >> 7) | (bytes[3] << 1)));

		byte sign = bytes[3];
		sign &= 0x80;

		exponent += 2;

		if (exponent > 255)
			throw new OverflowException();

		if (exponent <= 2)
			bytes.AsSpan().Clear();
		else
		{
			bytes[2] &= 0x7F;
			bytes[2] |= sign;

			bytes[3] = unchecked((byte)exponent);
		}

		return bytes;
	}

	public static float ToSingle(ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length != 4)
			throw new ArgumentException();

		// Shuffle bits
		int exponent = bytes[3];

		byte sign = bytes[2];
		sign &= 0x80;

		exponent -= 2;

		if (exponent <= 2)
			return 0;

		Span<byte> ieeeBytes = stackalloc byte[4];

		ieeeBytes[3] = unchecked((byte)(sign | (exponent >> 1)));
		ieeeBytes[2] = unchecked((byte)((exponent << 7) | (bytes[2] & 0x7F)));
		ieeeBytes[1] = bytes[1];
		ieeeBytes[0] = bytes[0];

		// Reinterpret back to semantic value.
		return BitConverter.ToSingle(ieeeBytes);
	}

	// Every part of the 64-bit MBF representation is different in some
	// way from the the IEEE form.
	//
	//   IEEE: seeeeeee eeeemmmm mmmmmmmm mmmmmmmm mmmmmmmm mmmmmmmm mmmmmmmm mmmmmmmm
	//   MBF:  eeeeeeee smmmmmmm mmmmmmmm mmmmmmmm mmmmmmmm mmmmmmmm mmmmmmmm mmmmmmmm
	//
	// Differences:
	// - The sign bit needs to shift to the right of the exponent.
	// - The exponent is 3 bits wider in the IEEE form. To convert
	//   the exponent, it needs to be unbiased, then truncated
	//   or extended, then re-biased.
	// - The mantissa has 3 more bits in the MBF form. Nothing
	//   to be done with these; 0-fill from IEEE and truncate from
	//   MBF.

	public static byte[] GetBytes(double value)
	{
		// Get the IEEE bytes
		byte[] bytes = BitConverter.GetBytes(value);

		// Shuffle bits
		int exponent = ((bytes[6] >> 4) | (bytes[7] << 4)) & 0x7FF;

		byte sign = bytes[7];
		sign &= 0x80;

		// For clarity, first convert the bit width.
		exponent = (exponent - 1024) + 128;

		// Then adjust the bias.
		exponent += 2;

		if (exponent > 255)
			throw new OverflowException();

		if (exponent <= 0)
			bytes.AsSpan().Clear();
		else
		{
			for (int i = 6; i > 0; i--)
				bytes[i] = unchecked((byte)((bytes[i] << 3) | (bytes[i - 1] >> 5)));
			bytes[0] <<= 3;

			bytes[6] &= 0x7F;
			bytes[6] |= sign;

			bytes[7] = unchecked((byte)exponent);
		}

		return bytes;
	}

	public static double ToDouble(ReadOnlySpan<byte> bytes)
	{
		if (bytes.Length != 8)
			throw new ArgumentException();

		// Shuffle bits
		int exponent = bytes[7];

		byte sign = bytes[6];
		sign &= 0x80;

		// For clarity, first convert the bit width.
		exponent = (exponent - 128) + 1024;

		// Then adjust the bias.
		exponent -= 2;

		Span<byte> ieeeBytes = stackalloc byte[8];

		for (int i = 0; i < 6; i++)
			ieeeBytes[i] = unchecked((byte)((bytes[i] >> 3) | (bytes[i + 1] << 5)));

		ieeeBytes[6] = unchecked((byte)(((bytes[6] >> 3) & 15) | (exponent << 4)));
		ieeeBytes[7] = unchecked((byte)(exponent >> 4));
		ieeeBytes[7] |= sign;

		// Reinterpret back to semantic value.
		return BitConverter.ToDouble(ieeeBytes);
	}
}
