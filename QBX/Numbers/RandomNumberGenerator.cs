using QBX.ExecutionEngine.Execution.Variables;
using System;
using System.Runtime.Intrinsics.Arm;

namespace QBX.Numbers;

public static class RandomNumberGenerator
{
	const int Multiplier = 0x343FD;
	const int Increment = 0x260C3;
	const int Modulus = 0x1000000;

	static long s_x;

	public static void Reseed(double value)
	{
		var noFracPart = double.Truncate(value);
		double fracPart = value - noFracPart;

		while ((noFracPart < int.MinValue) || (noFracPart > int.MaxValue))
			noFracPart *= 0.5f;

		int intPart = (int)noFracPart;

		Reseed(intPart, fracPart);
	}

	public static void Reseed(int intPart, double fracPart)
	{
		// This algorithm mimics the QBASIC algorithm in that it seeds
		// the random number generator to one of 65536 different values
		// floating around the midpoint of RND's range.
		//
		// It is not, however, numerically equivalent.

		if ((intPart == 0) && (fracPart == 0))
		{
			// Special case: RND(0) returns 0 after RANDOMIZE 0.
			s_x = 0;
		}
		else
		{
			s_x = 0x3FEC00;
			s_x += (intPart & 0xFFFF) * 0x100;
			s_x += (int)double.Floor(fracPart * 0x10000) * 0x100;

			s_x %= Modulus;
		}
	}

	public static void Advance()
	{
		s_x = (s_x * Multiplier + Increment) % Modulus;
	}

	public static float CurrentValue => (s_x / (float)Modulus);
}
