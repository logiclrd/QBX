using System;

namespace QBX.Firmware;

public class Window
{
	public readonly Point TopLeft;
	public readonly Point BottomRight;

	public readonly float ScaleX, ScaleY;
	public readonly float ScaleXAbsolute, ScaleYAbsolute;
	public readonly float ScaleBackX, ScaleBackY;

	public readonly static Window Dummy = new Window(0, 0, 1, 1, 1, 1);

	public Window(float topLeftX, float topLeftY, float bottomRightX, float bottomRightY, int pixelWidth, int pixelHeight)
	{
		TopLeft = (topLeftX, topLeftY);
		BottomRight = (bottomRightX, bottomRightY);

		ScaleX = (pixelWidth - 1) / (bottomRightX - topLeftX);
		ScaleY = (pixelHeight - 1) / (bottomRightY - topLeftY);

		ScaleXAbsolute = Math.Abs(ScaleX);
		ScaleYAbsolute = Math.Abs(ScaleY);

		ScaleBackX = 1.0f / ScaleX;
		ScaleBackY = 1.0f / ScaleY;
	}

	public (int X, int Y) TranslatePoint(float x, float y)
	{
		return
			(
				(int)Math.Round((x - TopLeft.X) * ScaleX),
				(int)Math.Round((y - TopLeft.Y) * ScaleY)
			);
	}

	public (float X, float Y) TranslateBack(int x, int y)
	{
		return
			(
				x * ScaleBackX + TopLeft.X,
				y * ScaleBackY + TopLeft.Y
			);
	}

	public int TranslateWidth(float width)
		=> (int)Math.Round(width * ScaleXAbsolute);
	public int TranslateHeight(float height)
		=> (int)Math.Round(height * ScaleYAbsolute);
}
