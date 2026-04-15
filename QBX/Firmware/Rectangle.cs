using QBX.Utility;

namespace QBX.Firmware;

public struct Rectangle
{
	public Point TopLeft;
	public Point BottomRight;

	public Rectangle()
	{
	}

	public Rectangle(Point topLeft, Point bottomRight)
	{
		TopLeft = topLeft;
		BottomRight = bottomRight;
	}

	public Rectangle(IntegerRect screen)
	{
		TopLeft = (screen.X1, screen.Y1);
		BottomRight = (screen.X2, screen.Y2);
	}
}
