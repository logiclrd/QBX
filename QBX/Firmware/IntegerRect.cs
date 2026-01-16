namespace QBX.Firmware;

public struct IntegerRect
{
	public int X1, Y1;
	public int X2, Y2;

	public static IntegerRect Unrestricted
	{
		get
		{
			var rect = new IntegerRect();

			rect.X1 = rect.Y1 = int.MinValue;
			rect.X2 = rect.Y2 = int.MaxValue;

			return rect;
		}
	}

	public static IntegerRect Empty
	{
		get
		{
			var rect = new IntegerRect();

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
