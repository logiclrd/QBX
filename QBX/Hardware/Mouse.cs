using System;

using QBX.Utility;

namespace QBX.Hardware;

public class Mouse
{
	public const int MickeysPerPixel = 10;

	public int X, Y;
	public int Width = 640, Height = 400;
	public bool LeftButton, MiddleButton, RightButton;

	public IntegerRect Bounds;

	public event Action? PositionChanged;
	public event Action<MouseButton>? ButtonChanged;

	public event Action? WarpMouse;
	public event Action? ResetGeometryOfSpace;
	public event Action? ChangeGeometryOfSpace;

	public void NotifyPositionChanged(float x, float y)
	{
		X = (int)Math.Round(x * MickeysPerPixel);
		Y = (int)Math.Round(y * MickeysPerPixel);

		if (X < 0)
			X = 0;
		if (X >= Width)
			X = Width - 1;
		if (Y < 0)
			Y = 0;
		if (Y >= Height)
			Y = Height - 1;

		PositionChanged?.Invoke();
	}

	public void NotifyPhysicalSizeChanged(int physicalWidth, int physicalHeight)
	{
		Width = physicalWidth * MickeysPerPixel;
		Height = physicalHeight * MickeysPerPixel;

		if (Width == 0)
			Width = 640;
		if (Height == 0)
			Height = 400;

		PushResetToTheGeometryOfSpace();

		PositionChanged?.Invoke();
	}

	public void PushPositionChange(int x, int y)
	{
		if (x < 0)
			x = 0;
		if (x >= Width)
			x = Width - 1;
		if (y < 0)
			y = 0;
		if (y >= Height)
			y = Height - 1;

		NotifyPositionChanged(x / MickeysPerPixel, y / MickeysPerPixel);

		WarpMouse?.Invoke();
	}

	public void PushResetToTheGeometryOfSpace()
	{
		Bounds.X1 = 0;
		Bounds.Y1 = 0;
		Bounds.X2 = Width - 1;
		Bounds.Y2 = Height - 1;

		ResetGeometryOfSpace?.Invoke();
	}

	public void PushChangeToTheGeometryOfSpace(int x1, int y1, int x2, int y2)
	{
		Bounds.X1 = x1;
		Bounds.Y1 = y1;
		Bounds.X2 = x2;
		Bounds.Y2 = y2;

		ChangeGeometryOfSpace?.Invoke();
	}

	public void NotifyButtonChanged(MouseButton which, bool isPressed)
	{
		switch (which)
		{
			case MouseButton.Left: LeftButton = isPressed; break;
			case MouseButton.Middle: MiddleButton = isPressed; break;
			case MouseButton.Right: RightButton = isPressed; break;
		}

		ButtonChanged?.Invoke(which);
	}
}
