namespace QBX.Firmware;

public struct Rect
{
	public int X1, Y1;
	public int X2, Y2;

	public static Rect Unrestricted
	{
		get
		{
			var rect = new Rect();

			rect.X1 = rect.Y1 = int.MinValue;
			rect.X2 = rect.Y2 = int.MaxValue;

			return rect;
		}
	}

	public static Rect Empty
	{
		get
		{
			var rect = new Rect();

			rect.X1 = rect.Y1 = int.MaxValue;
			rect.X2 = rect.Y2 = int.MinValue;

			return rect;
		}
	}

	public bool Contains(int x, int y)
	{
		return
			(x >= X1) && (x <= X2) &&
			(y >= Y1) && (y <= Y2);
	}
}
