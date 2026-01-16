namespace QBX.Firmware;

public static class PointExtensions
{
	public static void Deconstruct(this Point pt, out float x, out float y)
	{
		x = pt.X;
		y = pt.Y;
	}
}
