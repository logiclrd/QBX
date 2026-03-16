namespace QBX.Firmware;

public struct Point
{
	public float X, Y;

	public Point(float x, float y)
	{
		X = x;
		Y = y;
	}

	public static implicit operator Point((float, float) tuple)
		=> new Point(tuple.Item1, tuple.Item2);

	public static implicit operator Point((double, double) tuple)
		=> new Point((float)tuple.Item1, (float)tuple.Item2);

	public static Point operator +(Point left, Point right)
		=> new Point(left.X + right.X, left.Y + right.Y);

	public static Point operator +(Point left, (double, double) tuple)
		=> new Point((float)(left.X + tuple.Item1), (float)(left.Y + tuple.Item2));
}
