namespace QBX.Numbers;

public static class DecimalExtensions
{
	public static bool IsInCurrencyRange(this decimal value)
	{
		return
			(value >= -922337203685477.5808M) &&
			(value <= +922337203685477.5807M);
	}

	public static decimal Fix(this decimal value)
	{
		return decimal.Round(value * 10000M) * 0.0001M;
	}
}
