namespace QBX.Firmware;

public struct IntegerRect
{
	public int X1, Y1;
	public int X2, Y2;

	public IntegerRect()
	{
	}

	public IntegerRect(int x1, int y1, int x2, int y2)
	{
		X1 = x1;
		Y1 = y1;
		X2 = x2;
		Y2 = y2;
	}

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

	public bool Intersects(IntegerRect other)
	{
		return
			(X1 <= other.X2) && (other.X1 <= X2) &&
			(Y1 <= other.Y2) && (other.Y1 <= Y2);
	}
}
