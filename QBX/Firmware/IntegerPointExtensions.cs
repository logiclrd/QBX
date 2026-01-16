namespace QBX.Firmware;

public static class IntegerPointExtensions
{
	public static void Deconstruct(this IntegerPoint pt, out int x, out int y)
	{
		x = pt.X;
		y = pt.Y;
	}
}
