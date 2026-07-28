using System;

namespace QBX.Hardware.AdLib;

struct IIRParams
{
	public double B0, B1, B2;
	public double A1, A2;

	public override string ToString() => $"{B0} {B1} {B2} {A1} {A2}";

	struct Calculator
	{
		const double Fₛ = 44100;
		const double f0 = 1000; // centre frequency between bass and treble shelves

		const double S = 2; // Slope parameter -- slightly steeper than critically-damped

		const double ω0 = 2 * Math.PI * (f0 / Fₛ);

		double A;

		double cos_ω0;
		double sin_ω0;

		double α, β;

		public Calculator(double dbGain)
		{
			A = Math.Pow(10.0, dbGain / 40.0);

			cos_ω0 = Math.Cos(ω0);
			sin_ω0 = Math.Sin(ω0);

			α = 0.5 * sin_ω0 * Math.Sqrt((A + 1/A) * (1/S - 1) + 2);
			β = 2 * α * Math.Sqrt(A);
		}

		public void GetLowShelfParameters(out IIRParams param)
		{
			double a0 = (A + 1) + (A - 1) * cos_ω0 + β;

			param.B0 = A * ((A + 1) - (A - 1) * cos_ω0 + β) / a0;
			param.B1 = 2 * A * ((A - 1) - (A + 1) * cos_ω0) / a0;
			param.B2 = A * ((A + 1) - (A - 1) * cos_ω0 - β) / a0;

			param.A1 = -2 * ((A - 1) + (A + 1) * cos_ω0) / a0;
			param.A2 = ((A + 1) + (A - 1) * cos_ω0 - β) / a0;
		}

		public void GetHighShelfParameters(out IIRParams param)
		{
			double a0 = (A + 1) - (A - 1) * cos_ω0 + β;

			param.B0 = A * ((A + 1) + (A - 1) * cos_ω0 + β) / a0;
			param.B1 = -2 * A * ((A - 1) + (A + 1) * cos_ω0) / a0;
			param.B2 = A * ((A + 1) + (A - 1) * cos_ω0 - β) / a0;

			param.A1 = 2 * ((A - 1) - (A + 1) * cos_ω0) / a0;
			param.A2 = ((A + 1) - (A - 1) * cos_ω0 - β) / a0;
		}
	}

	public void CalculateLowShelf(double dbGain)
	{
		// Bass filter; passes high frequencies unchanged, attenuates low frequencies
		new Calculator(dbGain).GetLowShelfParameters(out this);
	}

	public void CalculateHighShelf(double dbGain)
	{
		// Treble filter; passes low frequencies unchanged, attenuates high frequencies
		new Calculator(dbGain).GetHighShelfParameters(out this);
	}
}
