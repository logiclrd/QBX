using System;

namespace QBX.Numbers;

public static class DecimalExtensions
{
	public static bool IsInCurrencyRange(this decimal value)
	{
		return
			(value >= -922337203685477.5808M) &&
			(value <= +922337203685477.5807M);
	}

	public static bool IsTooPrecise(this decimal value)
	{
		value = value * 10000M;

		return value != decimal.Truncate(value);
	}

	public static decimal Fix(this decimal value)
	{
		value = value * 10000M;

		if (value > 0)
			value = decimal.Round(value, MidpointRounding.AwayFromZero);
		else
			value = decimal.Round(value, MidpointRounding.ToZero);

		return value * 0.0001M;
	}
}
