namespace QBX.Firmware;

public struct Point
{
	public int X, Y;

	public Point(int x, int y)
	{
		X = x;
		Y = y;
	}

	public static implicit operator Point((int, int) tuple)
		=> new Point(tuple.Item1, tuple.Item2);
}
