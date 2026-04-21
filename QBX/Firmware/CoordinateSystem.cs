using System;

using QBX.Utility;

namespace QBX.Firmware;

public class CoordinateSystem
{
	IntegerRect _screen;
	IntegerRect _viewport;
	bool _haveWindow;
	Rectangle _window;

	CoordinateType _coordinateType = CoordinateType.Screen;
	IntegerRect _target;

	float _scaleX, _scaleY;
	float _scaleXAbsolute, _scaleYAbsolute;
	float _scaleBackX, _scaleBackY;

	public IntegerRect Screen => _screen;
	public IntegerRect Viewport => _viewport;
	public bool HaveWindow => _haveWindow;
	public Rectangle Window => _window;

	public CoordinateType CoordinateType => _coordinateType;
	public IntegerRect Target => _target;

	public float ScaleX => _scaleX;
	public float ScaleY => _scaleY;
	public float ScaleXAbsolute => _scaleXAbsolute;
	public float ScaleYAbsolute => _scaleYAbsolute;

	IntegerRect Basis =>
		((_coordinateType == CoordinateType.Viewport) || _haveWindow)
		? _viewport
		: _screen;

	public Point ViewportCentre => Basis.Centre;

	public readonly static CoordinateSystem Dummy = new CoordinateSystem(1, 1);

	public CoordinateSystem(int screenWidth, int screenHeight)
	{
		_screen = new IntegerRect(0, 0, screenWidth - 1, screenHeight - 1);

		ResetViewportInternal();
		ResetWindowInternal();

		UpdateScale();
	}

	void UpdateScale()
	{
		_target =
			_coordinateType switch
			{
				CoordinateType.Screen => _screen,
				CoordinateType.Viewport or _ => _haveWindow ? Basis : _screen.Offset(_viewport.X1, _viewport.Y1),
			};

		_scaleX = (_target.X2 - _target.X1) / (_window.BottomRight.X - _window.TopLeft.X);
		_scaleY = (_target.Y2 - _target.Y1) / (_window.BottomRight.Y - _window.TopLeft.Y);

		_scaleXAbsolute = Math.Abs(_scaleX);
		_scaleYAbsolute = Math.Abs(_scaleY);

		_scaleBackX = 1.0f / _scaleX;
		_scaleBackY = 1.0f / _scaleY;
	}

	void ResetViewportInternal()
	{
		SetViewportInternal(
			_screen,
			CoordinateType.Screen);
	}

	void SetViewportInternal(IntegerRect viewport, CoordinateType coordinateType)
	{
		_viewport = viewport;
		_coordinateType = coordinateType;

		if (!_haveWindow)
			ResetWindowInternal();
	}

	void ResetWindowInternal()
	{
		SetWindowInternal((_screen.X1, _screen.Y1), (_screen.X2, _screen.Y2));

		// Got set by SetWindowInternal
		_haveWindow = false;
	}

	void SetWindowInternal(Point topLeft, Point bottomRight)
		=> SetWindowInternal(new Rectangle(topLeft, bottomRight));

	void SetWindowInternal(Rectangle window)
	{
		_window = window;
		_haveWindow = true;
	}

	public void ResetViewport()
	{
		ResetViewportInternal();
		UpdateScale();
	}

	public void SetViewport(IntegerRect viewport, CoordinateType coordinateType)
	{
		SetViewportInternal(viewport, coordinateType);
		UpdateScale();
	}

	public void ResetWindow()
	{
		ResetWindowInternal();

		UpdateScale();
	}

	public void SetWindow(Point topLeft, Point bottomRight)
		=> SetWindow(new Rectangle(topLeft, bottomRight));

	public void SetWindow(Rectangle window)
	{
		SetWindowInternal(window);

		UpdateScale();
	}

	public (int X, int Y) TranslateWindowToScreen(float x, float y)
	{
		var target = _target;

		var view = TranslateWindowToView(x, y);

		return (view.X + target.X1, view.Y + target.Y1);
	}

	public (int X, int Y) TranslateWindowToView(float x, float y)
	{
		return
			(
				(int)Math.Round((x - _window.TopLeft.X) * _scaleX),
				(int)Math.Round((y - _window.TopLeft.Y) * _scaleY)
			);
	}

	public (float X, float Y) TranslateScreenToWindow(int x, int y)
	{
		var target = _target;

		return
			(
				(x - target.X1 + 0.5f) * _scaleBackX + _window.TopLeft.X,
				(y - target.Y1 + 0.5f) * _scaleBackY + _window.TopLeft.Y
			);
	}

	public (float X, float Y) TranslateViewToWindow(float x, float y)
	{
		return
			(
				x * _scaleBackX + _window.TopLeft.X,
				y * _scaleBackY + _window.TopLeft.Y
			);
	}

	public Point TranslateBack(Point pt)
	{
		var target = _target;

		return
			(
				(pt.X - target.X1) * _scaleBackX + _window.TopLeft.X,
				(pt.Y - target.Y1) * _scaleBackY + _window.TopLeft.Y
			);
	}

	public int TranslateWidth(float width)
		=> (int)Math.Round(width * _scaleXAbsolute);
	public int TranslateHeight(float height)
		=> (int)Math.Round(height * _scaleYAbsolute);
}
