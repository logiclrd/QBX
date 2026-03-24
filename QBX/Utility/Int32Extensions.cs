namespace QBX.Utility;

public static class Int32Extensions
{
	public static int Clamp(this int x, int min, int max)
	{
		if (x < min)
			return min;
		if (x > max)
			return max;

		return x;
	}
}

