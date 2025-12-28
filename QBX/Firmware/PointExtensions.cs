namespace QBX.Firmware;

public static class PointExtensions
{
	public static void Deconstruct(this Point pt, out int x, out int y)
	{
		x = pt.X;
		y = pt.Y;
	}
}
