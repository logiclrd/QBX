using System;

using QBX.Hardware;

namespace QBX.Firmware;

public abstract class GraphicsLibrary : VisualLibrary
{
	protected GraphicsLibrary(Machine machine)
		: base(machine)
	{
		Window = Window.Dummy;
	}

	public int Aspect;

	public byte[][] Font = CreateBlankFont();

	static byte[][] CreateBlankFont()
	{
		byte[][] ret = new byte[256][];

		ret.AsSpan().Fill(new byte[16]);

		return ret;
	}

	public int DrawingAttribute;
	public Window Window;
	public Point LastPoint;

	public int CharacterScans; // doesn't exist on the VGA chip in graphics modes

	public override void RefreshParameters()
	{
		Width = Array.MiscellaneousOutput.BasePixelWidth >> (Array.Sequencer.DotDoubling ? 1 : 0);
		Height = Array.CRTController.NumScanLines;

		Window = new Window(0, 0, Width, Height, Width, Height);

		if (CharacterScans == 0)
		{
			if (Height >= 400)
				CharacterScans = 16;
			else if (Height >= 340)
				CharacterScans = 14;
			else
				CharacterScans = 8;
		}

		CharacterWidth = Width / Array.Sequencer.CharacterWidth;
		CharacterHeight = Height / CharacterScans;

		if ((Width >= 640) && (Height <= 240))
			Aspect = 2;
		else
			Aspect = 1;

		Font = Machine.VideoFirmware.GetFont(CharacterScans);

		base.RefreshParameters();
	}

	public bool SetCharacterScans(int newScans)
	{
		var font = Machine.VideoFirmware.TryGetFont(newScans);

		if (font == null)
			return false;

		Font = font;

		CharacterScans = newScans;
		CharacterHeight = Height / CharacterScans;

		if (CursorY >= CharacterHeight)
			MoveCursor(CursorX, CharacterHeight - 1);

		return true;
	}

	protected sealed override void ClearImplementation(int fromCharacterLine = 0, int toCharacterLine = -1)
	{
		int windowStart = fromCharacterLine * CharacterScans;
		int windowEnd =
			toCharacterLine < 0
			? Height - 1
			: (toCharacterLine + 1) * CharacterScans - 1;

		ClearGraphicsImplementation(windowStart, windowEnd);
	}

	protected abstract void ClearGraphicsImplementation(int windowStart, int windowEnd);

	public void SetDrawingAttribute(int attribute)
	{
		DrawingAttribute = attribute;
	}

	#region PixelSet
	public virtual void PixelSet(float x, float y)
		=> PixelSet(x, y, DrawingAttribute);

	public void PixelSet(float x, float y, int attribute)
	{
		var translated = Window.TranslatePoint(x, y);

		PixelSet(translated.X, translated.Y);
	}

	public abstract void PixelSet(int x, int y, int attribute);
	#endregion PixelSet

	#region HorizontalLine
	public virtual void HorizontalLine(int x1, int x2, int y)
		=> HorizontalLine(x1, x2, y, DrawingAttribute);

	public virtual void HorizontalLine(int x1, int x2, int y, int attribute)
	{
		for (int x = x1; x <= x2; x++)
			PixelSet(x, y, attribute);
	}
	#endregion

	#region Line
	public void Line(Point pt1, Point pt2)
		=> Line(pt1, pt2, DrawingAttribute);

	public void Line(Point pt1, Point pt2, int attribute)
		=> Line(pt1.X, pt1.Y, pt2.X, pt2.Y, attribute);

	public void LineTo(Point pt2)
		=> LineTo(pt2, DrawingAttribute);

	public void LineTo(Point pt2, int attribute)
		=> Line(LastPoint.X, LastPoint.Y, pt2.X, pt2.Y, attribute);

	public void LineTo(float x2, float y2)
		=> LineTo(x2, y2, DrawingAttribute);

	public void LineTo(float x2, float y2, int attribute)
		=> Line(LastPoint.X, LastPoint.Y, x2, y2, attribute);

	public void Line(float x1, float y1, float x2, float y2)
		=> Line(x1, y1, x2, y2, DrawingAttribute);

	public void Line(float x1, float y1, float x2, float y2, int attribute)
	{
		var translated1 = Window.TranslatePoint(x1, y1);
		var translated2 = Window.TranslatePoint(x2, y2);

		Line(translated1.X, translated1.Y, translated2.X, translated2.Y, attribute);

		LastPoint = (x2, y2);
	}

	public void Line(int x1, int y1, int x2, int y2, int attribute)
	{
		int dx = Math.Abs(x1 - x2);
		int dy = Math.Abs(y1 - y2);

		LastPoint = Window.TranslateBack(x2, y2);

		if (dx > dy)
		{
			if (x1 > x2)
				(x1, y1, x2, y2) = (x2, y2, x1, y1);

			int sy = Math.Sign(y2 - y1);

			int xStart = x1;
			int y = y1;
			int yError = 0;

			for (int x = x1; x <= x2; x++)
			{
				yError += dy;

				if (yError >= dx)
				{
					HorizontalLine(xStart, x - 1, y, attribute);

					xStart = x;

					yError -= dx;
					y += sy;
				}
			}

			HorizontalLine(xStart, x2, y, attribute);
		}
		else
		{
			if (y1 > y2)
				(x1, y1, x2, y2) = (x2, y2, x1, y1);

			int sx = Math.Sign(x2 - x1);

			for (int x = x1, y = y1, xError = 0; y <= y2; y++)
			{
				PixelSet(x, y, attribute);

				xError += dx;

				if (xError >= dy)
				{
					xError -= dy;
					x += sx;
				}
			}
		}
	}
	#endregion

	#region LineStyle
	public void LineStyle(Point pt1, Point pt2, int styleBits)
		=> LineStyle(pt1, pt2, DrawingAttribute, styleBits);

	public void LineStyle(Point pt1, Point pt2, int attribute, int styleBits)
		=> LineStyle(pt1.X, pt1.Y, pt2.X, pt2.Y, attribute, styleBits);

	public void LineStyleTo(Point pt2, int styleBits)
		=> LineStyleTo(pt2, DrawingAttribute, styleBits);

	public void LineStyleTo(Point pt2, int attribute, int styleBits)
		=> LineStyle(LastPoint.X, LastPoint.Y, pt2.X, pt2.Y, attribute, styleBits);

	public void LineStyleTo(float x2, float y2, int styleBits)
		=> LineStyleTo(x2, y2, DrawingAttribute, styleBits);

	public void LineStyleTo(float x2, float y2, int attribute, int styleBits)
		=> LineStyle(LastPoint.X, LastPoint.Y, x2, y2, attribute, styleBits);

	public void LineStyle(float x1, float y1, float x2, float y2, int styleBits)
		=> LineStyle(x1, y1, x2, y2, DrawingAttribute, styleBits);

	public void LineStyle(float x1, float y1, float x2, float y2, int attribute, int styleBits)
	{
		var translated1 = Window.TranslatePoint(x1, y1);
		var translated2 = Window.TranslatePoint(x2, y2);

		LineStyle(translated1.X, translated1.Y, translated2.X, translated2.Y, attribute, styleBits);

		LastPoint = (x2, y2);
	}

	public void LineStyle(int x1, int y1, int x2, int y2, int attribute, int styleBits)
	{
		int dx = Math.Abs(x1 - x2);
		int dy = Math.Abs(y1 - y2);

		LastPoint = Window.TranslateBack(x2, y2);

		if (dx > dy)
		{
			if (x1 > x2)
				(x1, y1, x2, y2) = (x2, y2, x1, y1);

			int sy = Math.Sign(y2 - y1);

			int y = y1;
			int yError = 0;

			int test = 0x8000;

			for (int x = x1; x <= x2; x++)
			{
				if ((styleBits & test) != 0)
					PixelSet(x, y, attribute);

				test >>= 1;
				if (test == 0)
					test = 0x8000;

				yError += dy;

				if (yError >= dx)
				{
					yError -= dx;
					y += sy;
				}
			}
		}
		else
		{
			if (y1 > y2)
				(x1, y1, x2, y2) = (x2, y2, x1, y1);

			int sx = Math.Sign(x2 - x1);

			int test = 0x8000;

			for (int x = x1, y = y1, xError = 0; y <= y2; y++)
			{
				if ((styleBits & test) != 0)
					PixelSet(x, y, attribute);

				test >>= 1;
				if (test == 0)
					test = 0x8000;

				xError += dx;

				if (xError >= dy)
				{
					xError -= dy;
					x += sx;
				}
			}
		}
	}
	#endregion

	#region Box
	public void Box(Point pt1, Point pt2)
		=> Box(pt1, pt2, DrawingAttribute);

	public void Box(Point pt1, Point pt2, int attribute)
		=> Box(pt1.X, pt1.Y, pt2.X, pt2.Y, attribute);

	public void BoxTo(Point pt2)
		=> BoxTo(pt2, DrawingAttribute);

	public void BoxTo(Point pt2, int attribute)
		=> Box(LastPoint.X, LastPoint.Y, pt2.X, pt2.Y, attribute);

	public void BoxTo(float x2, float y2)
		=> BoxTo(x2, y2, DrawingAttribute);

	public void BoxTo(float x2, float y2, int attribute)
		=> Box(LastPoint.X, LastPoint.Y, x2, y2, attribute);

	public void Box(float x1, float y1, float x2, float y2)
		=> Box(x1, y1, x2, y2, DrawingAttribute);

	public void Box(float x1, float y1, float x2, float y2, int attribute)
	{
		var translated1 = Window.TranslatePoint(x1, y1);
		var translated2 = Window.TranslatePoint(x2, y2);

		Box(translated1.X, translated1.Y, translated2.X, translated2.Y, attribute);

		LastPoint = (x2, y2);
	}

	public void Box(int x1, int y1, int x2, int y2, int attribute)
	{
		LastPoint = Window.TranslateBack(x2, y2);

		if (y1 > y2)
			(y1, y2) = (y2, y1);
		if (x1 > x2)
			(x1, x2) = (x2, x1);

		bool drawLeft = true;
		bool drawRight = true;

		if (x1 < 0)
		{
			x1 = 0;
			drawLeft = false;
		}

		if (x2 >= Width)
		{
			x2 = Width - 1;
			drawRight = false;
		}

		if (x1 > x2) // box is entirely off the screen
			return;

		if (y1 >= 0)
			HorizontalLine(x1, x2, y1++, attribute);
		else
			y1 = 0;

		if (y2 < Height)
			HorizontalLine(x1, x2, y2--, attribute);
		else
			y2 = Height - 1;

		if (drawLeft || drawRight)
		{
			for (int y = y1; y <= y2; y++)
			{
				if (drawLeft)
					PixelSet(x1, y, attribute);
				if (drawRight)
					PixelSet(x2, y, attribute);
			}
		}
	}
	#endregion

	#region BoxStyle
	public void BoxStyle(Point pt1, Point pt2, int styleBits)
		=> BoxStyle(pt1, pt2, DrawingAttribute);

	public void BoxStyle(Point pt1, Point pt2, int attribute, int styleBits)
		=> BoxStyle(pt1.X, pt1.Y, pt2.X, pt2.Y, attribute);

	public void BoxStyleTo(Point pt2, int styleBits)
		=> BoxStyleTo(pt2, DrawingAttribute);

	public void BoxStyleTo(Point pt2, int attribute, int styleBits)
		=> BoxStyle(LastPoint.X, LastPoint.Y, pt2.X, pt2.Y, attribute);

	public void BoxStyleTo(float x2, float y2, int styleBits)
		=> BoxStyleTo(x2, y2, DrawingAttribute);

	public void BoxStyleTo(float x2, float y2, int attribute, int styleBits)
		=> BoxStyle(LastPoint.X, LastPoint.Y, x2, y2, attribute);

	public void BoxStyle(float x1, float y1, float x2, float y2, int styleBits)
		=> BoxStyle(x1, y1, x2, y2, DrawingAttribute);

	public void BoxStyle(float x1, float y1, float x2, float y2, int attribute, int styleBits)
	{
		var translated1 = Window.TranslatePoint(x1, y1);
		var translated2 = Window.TranslatePoint(x2, y2);

		BoxStyle(translated1.X, translated1.Y, translated2.X, translated2.Y, attribute, styleBits);

		LastPoint = (x2, y2);
	}

	public void BoxStyle(int x1, int y1, int x2, int y2, int attribute, int styleBits)
	{
		LastPoint = Window.TranslateBack(x2, y2);

		if (y1 > y2)
			(y1, y2) = (y2, y1);
		if (x1 > x2)
			(x1, x2) = (x2, x1);

		bool drawLeft = true;
		bool drawRight = true;

		if (x1 < 0)
		{
			x1 = 0;
			drawLeft = false;
		}

		if (x2 >= Width)
		{
			x2 = Width - 1;
			drawRight = false;
		}

		if (x1 > x2) // box is entirely off the screen
			return;

		int test = 0x8000;

		if (y2 < Height)
		{
			for (int x = x1; x < x2; x++)
			{
				if ((styleBits & test) != 0)
					PixelSet(x, y2, attribute);

				test >>= 1;
				if (test == 0)
					test = 0x8000;
			}
		}
		else
			y2 = Height - 1;

		if (y1 >= 0)
		{
			for (int x = x1; x < x2; x++)
			{
				if ((styleBits & test) != 0)
					PixelSet(x, y1, attribute);

				test >>= 1;
				if (test == 0)
					test = 0x8000;
			}
		}
		else
			y1 = 0;

		if (drawRight)
		{
			for (int y = y1; y <= y2; y++)
			{
				if ((styleBits & test) != 0)
					PixelSet(x2, y, attribute);

				test >>= 1;
				if (test == 0)
					test = 0x8000;
			}
		}

		if (drawLeft)
		{
			for (int y = y1; y <= y2; y++)
			{
				if ((styleBits & test) != 0)
					PixelSet(x1, y, attribute);

				test >>= 1;
				if (test == 0)
					test = 0x8000;
			}
		}
	}
	#endregion

	#region FillBox
	public void FillBox(Point pt1, Point pt2)
		=> FillBox(pt1, pt2, DrawingAttribute);

	public void FillBox(Point pt1, Point pt2, int attribute)
		=> FillBox(pt1.X, pt1.Y, pt2.X, pt2.Y, attribute);

	public void FillBoxTo(Point pt2)
		=> FillBoxTo(pt2, DrawingAttribute);

	public void FillBoxTo(Point pt2, int attribute)
		=> FillBox(LastPoint.X, LastPoint.Y, pt2.X, pt2.Y, attribute);

	public void FillBoxTo(float x2, float y2)
		=> FillBoxTo(x2, y2, DrawingAttribute);

	public void FillBoxTo(float x2, float y2, int attribute)
		=> FillBox(LastPoint.X, LastPoint.Y, x2, y2, attribute);

	public void FillBox(float x1, float y1, float x2, float y2)
		=> FillBox(x1, y1, x2, y2, DrawingAttribute);

	public void FillBox(float x1, float y1, float x2, float y2, int attribute)
	{
		var translated1 = Window.TranslatePoint(x1, y1);
		var translated2 = Window.TranslatePoint(x2, y2);

		FillBox(translated1.X, translated1.Y, translated2.X, translated2.Y, attribute);

		LastPoint = (x2, y2);
	}

	public void FillBox(int x1, int y1, int x2, int y2, int attribute)
	{
		LastPoint = Window.TranslateBack(x2, y2);

		if (y1 > y2)
			(y1, y2) = (y2, y1);
		if (x1 > x2)
			(x1, x2) = (x2, x1);

		if (y1 < 0)
			y1 = 0;
		if (y2 >= Height)
			y2 = Height - 1;

		if (x1 < 0)
			x1 = 0;
		if (x2 >= Width)
			x2 = Width - 1;

		if (x1 > x2) // box is entirely off the screen
			return;

		for (int y = y1; y <= y2; y++)
			HorizontalLine(x1, x2, y, attribute);
	}
	#endregion

	struct PixelSpan
	{
		public int X1, X2;
		public int Y;

		public bool IsEmpty => Y == int.MinValue;

		public static PixelSpan Empty => new() { Y = int.MinValue };

		public PixelSpan(int x, int y)
		{
			X1 = X2 = x;
			Y = y;
		}

		public bool Extend(int x, int y)
		{
			if (IsEmpty)
			{
				X1 = X2 = x;
				Y = y;

				return true;
			}

			if (y != Y)
				return false;

			if (X1 == x + 1)
			{
				X1--;
				return true;
			}

			if (X2 == x - 1)
			{
				X2++;
				return true;
			}

			return false;
		}
	}

	#region Ellipse
	public void Ellipse(float x, float y, float radiusX, float radiusY, double startAngle, double endAngle, bool drawStartRadius, bool drawEndRadius)
		=> Ellipse(x, y, radiusX, radiusY, startAngle, endAngle, drawStartRadius, drawEndRadius, DrawingAttribute);

	public void Ellipse(float x, float y, float radiusX, float radiusY, double startAngle, double endAngle, bool drawStartRadius, bool drawEndRadius, int attribute)
	{
		var translated = Window.TranslatePoint(x, y);

		int translatedRadiusX = Window.TranslateWidth(radiusX);
		int translatedRadiusY = Window.TranslateHeight(radiusY);

		Ellipse(translated.X, translated.Y, translatedRadiusX, translatedRadiusY, startAngle, endAngle, drawStartRadius, drawEndRadius, attribute);

		LastPoint = (x, y);
	}

	public void Ellipse(int x, int y, int radiusX, int radiusY, double startAngle, double endAngle, bool drawStartRadius, bool drawEndRadius)
	{
		Ellipse(x, y, radiusX, radiusY, startAngle, endAngle, drawStartRadius, drawEndRadius, DrawingAttribute);
	}

	public void Ellipse(int x, int y, int radiusX, int radiusY, double startAngle, double endAngle, bool drawStartRadius, bool drawEndRadius, int attribute)
	{
		(int X, int Y) PointAtAngle(double angle)
		{
			double x = Math.Cos(angle);
			double y = Math.Sin(angle);

			x *= radiusX;
			y *= radiusY;

			return ((int)Math.Round(x), (int)Math.Round(y));
		}

		radiusY = (radiusY + Aspect - 1) / Aspect;

		IntegerPoint startPoint, endPoint;
		int startOctant, endOctant;
		IntegerRect startClip, endClip, startStopExclude;

		startPoint = PointAtAngle(startAngle);
		endPoint = PointAtAngle(endAngle);

		startStopExclude = IntegerRect.Empty;

		// Unless it's a perfect circle, after deformation, the angles aren't linear.
		// The provided angles are interpreted as though we will be drawing a perfect
		// circle, and then the resulting section is deformed to the requested
		// ellipse. Our quadrant calculations need to work with the *actual* angle,
		// so convert the ellipse point back.
		startAngle = Math.Atan2(startPoint.Y, startPoint.X);
		endAngle = Math.Atan2(endPoint.Y, endPoint.X);

		if (startAngle < 0)
			startAngle += 2 * Math.PI;
		if (endAngle < 0)
			endAngle += 2 * Math.PI;

		startOctant = endOctant = -1;
		startClip = endClip = IntegerRect.Unrestricted;

		// Octants divide where the tangent is a multiple of 45 degrees
		//
		// - Divisions at the axes where the tangent is horizontal/vertical
		// - Divisions at the angle that produces the points with +/- 45-degree tangents.
		//
		// These angles are only even subdivisions when the ellipse is a perfect circle.
		//
		// The point of intersection with these tangent lines occurs at
		//
		// (rx^2 / sqrt(rx^2 + ry^2), ry^2 / sqrt(rx^2 + ry^2))

		double superRadius = Math.Sqrt(radiusX * radiusX + radiusY * radiusY);
		double octantChangeX = radiusX * radiusX / superRadius;
		double octantChangeY = radiusY * radiusY / superRadius;

		double octantChangeAngle = Math.Atan2(octantChangeY, octantChangeX);

		if (startAngle < octantChangeAngle)
		{
			startOctant = 0;
			(startClip.X2, startClip.Y1) = startPoint;
		}
		else if (startAngle < Math.PI * 0.5) // top edge
		{
			startOctant = 1;
			(startClip.X2, startClip.Y1) = startPoint;
		}
		else if (startAngle < Math.PI - octantChangeAngle)
		{
			startOctant = 2;
			(startClip.X2, startClip.Y2) = startPoint;
		}
		else if (startAngle < Math.PI) // left edge
		{
			startOctant = 3;
			(startClip.X2, startClip.Y2) = startPoint;
		}
		else if (startAngle < Math.PI + octantChangeAngle)
		{
			startOctant = 4;
			(startClip.X1, startClip.Y2) = startPoint;
		}
		else if (startAngle < Math.PI * 1.5) // bottom edge
		{
			startOctant = 5;
			(startClip.X1, startClip.Y2) = startPoint;
		}
		else if (startAngle < 2 * Math.PI - octantChangeAngle)
		{
			startOctant = 6;
			(startClip.X1, startClip.Y1) = startPoint;
		}
		else if (startAngle < 2 * Math.PI) // right edge
		{
			startOctant = 7;
			(startClip.X1, startClip.Y1) = startPoint;
		}

		// Obvious difference: draw up to end angle instead of starting at end angle
		// Subtle difference: end angle of 0 is treated as the end of the circle

		if (endAngle == 0)
			endAngle = 2 * Math.PI;

		if (endAngle < octantChangeAngle)
		{
			endOctant = 0;
			(endClip.X1, endClip.Y2) = endPoint;
		}
		else if (endAngle < Math.PI * 0.5) // top edge
		{
			endOctant = 1;
			(endClip.X1, endClip.Y2) = endPoint;
		}
		else if (endAngle < Math.PI - octantChangeAngle)
		{
			endOctant = 2;
			(endClip.X1, endClip.Y1) = endPoint;
		}
		else if (endAngle < Math.PI) // left edge
		{
			endOctant = 3;
			(endClip.X1, endClip.Y1) = endPoint;
		}
		else if (endAngle < Math.PI + octantChangeAngle)
		{
			endOctant = 4;
			(endClip.X2, endClip.Y1) = endPoint;
		}
		else if (endAngle < Math.PI * 1.5) // bottom edge
		{
			endOctant = 5;
			(endClip.X2, endClip.Y1) = endPoint;
		}
		else if (endAngle < 2 * Math.PI - octantChangeAngle)
		{
			endOctant = 6;
			(endClip.X2, endClip.Y2) = endPoint;
		}
		else if (endAngle < 2 * Math.PI) // right edge
		{
			endOctant = 7;
			(endClip.X2, endClip.Y2) = endPoint;
		}

		if (startOctant == endOctant)
		{
			startStopExclude.X1 = Math.Max(startClip.X1, endClip.X1);
			startStopExclude.Y1 = Math.Max(startClip.Y1, endClip.Y1);
			startStopExclude.X2 = Math.Min(startClip.X2, endClip.X2);
			startStopExclude.Y2 = Math.Min(startClip.Y2, endClip.Y2);

			if (startStopExclude.X1 > startStopExclude.X2)
				(startStopExclude.X1, startStopExclude.X2) = (startStopExclude.X2, startStopExclude.X1);
			if (startStopExclude.Y1 > startStopExclude.Y2)
				(startStopExclude.Y1, startStopExclude.Y2) = (startStopExclude.Y2, startStopExclude.Y1);
		}

		// Now draw
		if (drawStartRadius)
			Line(x, y, x + startPoint.X, y - startPoint.Y, attribute);
		if (drawEndRadius)
			Line(x, y, x + endPoint.X, y - endPoint.Y, attribute);

		// Common
		// - Algorithm based on "A Fast Bresenham Type Algorithm For Drawing Ellipses" by John Kennedy
		int twoRadiusXSquared = 2 * radiusX * radiusX;
		int twoRadiusYSquared = 2 * radiusY * radiusY;
		int xChange, yChange;
		int ellipseError;
		int xStop, yStop;
		int sx, sy;
		bool wrap = startAngle > endAngle;

		bool CheckOctant(int octant, int dx, int dy)
		{
			if (startOctant <= endOctant)
			{
				if (!wrap && ((octant < startOctant) || (octant > endOctant)))
					return false;
			}
			else
			{
				if ((octant < startOctant) && (octant > endOctant))
					return false;
			}

			if ((startOctant == endOctant) && wrap)
			{
				if ((octant == startOctant) && startStopExclude.Contains(dx, dy))
					return false;
			}
			else
			{
				if ((octant == startOctant) && !startClip.Contains(dx, dy))
					return false;
				if ((octant == endOctant) && !endClip.Contains(dx, dy))
					return false;
			}

			return true;
		}


		// Quadrants 0, 3, 4 and 7
		void PixelPlot(int octant, int xSign, int ySign)
		{
			int dx = sx * xSign;
			int dy = sy * ySign;

			if (CheckOctant(octant, dx, dy))
				PixelSet(x + dx, y - dy, attribute);
		}

		sx = radiusX;
		sy = 0;

		xChange = radiusY * radiusY * (1 - 2 * radiusX);
		yChange = radiusX * radiusX;

		ellipseError = 0;

		xStop = twoRadiusYSquared * radiusX;
		yStop = 0;

		while (xStop >= yStop)
		{
			PixelPlot(0, +1, +1);
			PixelPlot(3, -1, +1);
			PixelPlot(4, -1, -1);
			PixelPlot(7, +1, -1);

			sy++;
			yStop += twoRadiusXSquared;
			ellipseError += yChange;
			yChange += twoRadiusXSquared;

			if (2 * ellipseError + xChange > 0)
			{
				sx--;
				xStop -= twoRadiusYSquared;
				ellipseError += xChange;
				xChange += twoRadiusYSquared;
			}
		}

		// Quadrants 1, 2, 5 and 6
		sx = 0;
		sy = radiusY;

		xChange = radiusY * radiusY;
		yChange = radiusX * radiusX * (1 - 2 * radiusY);

		ellipseError = 0;

		xStop = 0;
		yStop = twoRadiusXSquared * radiusY;

		var spans = new PixelSpan[7];

		spans[1] = PixelSpan.Empty;
		spans[2] = PixelSpan.Empty;
		spans[5] = PixelSpan.Empty;
		spans[6] = PixelSpan.Empty;

		void SpanPlot(int octant, int xSign, int ySign)
		{
			int dx = sx * xSign;
			int dy = sy * ySign;

			if (CheckOctant(octant, dx, dy))
			{
				ref var span = ref spans[octant];

				if (!span.Extend(dx, dy))
				{
					HorizontalLine(x + span.X1, x + span.X2, y - span.Y, attribute);
					spans[octant] = new PixelSpan(dx, dy);
				}
			}
		}

		while (xStop <= yStop)
		{
			SpanPlot(1, +1, +1);
			SpanPlot(2, -1, +1);
			SpanPlot(5, -1, -1);
			SpanPlot(6, +1, -1);

			sx++;
			xStop += twoRadiusYSquared;
			ellipseError += xChange;
			xChange += twoRadiusYSquared;

			if (2 * ellipseError + yChange > 0)
			{
				sy--;
				yStop -= twoRadiusXSquared;
				ellipseError += yChange;
				yChange += twoRadiusXSquared;
			}
		}

		if (!spans[1].IsEmpty)
			HorizontalLine(x + spans[1].X1, x + spans[1].X2, y - spans[1].Y, attribute);
		if (!spans[2].IsEmpty)
			HorizontalLine(x + spans[2].X1, x + spans[2].X2, y - spans[2].Y, attribute);
		if (!spans[5].IsEmpty)
			HorizontalLine(x + spans[5].X1, x + spans[5].X2, y - spans[5].Y, attribute);
		if (!spans[6].IsEmpty)
			HorizontalLine(x + spans[6].X1, x + spans[6].X2, y - spans[6].Y, attribute);

		LastPoint = Window.TranslateBack(x, y);
	}
	#endregion Ellipse

	#region Text
	public override void WriteText(ReadOnlySpan<byte> buffer)
	{
		if (buffer.Length == 0)
			return;

		ResolvePassiveNewLine();

		int characterWidth = Array.Sequencer.CharacterWidth;
		int characterHeight = CharacterScans;

		while (!buffer.IsEmpty)
		{
			byte character = buffer[0];

			WriteCharacterAt(CursorX * characterWidth, CursorY * characterHeight, character);

			AdvanceCursor();

			buffer = buffer.Slice(1);
		}
	}

	public void WriteCharacterAt(int x, int y, byte character)
	{
		int characterWidth = Array.Sequencer.CharacterWidth;
		int characterHeight = CharacterScans;

		byte[] glyph = Font[character];

		for (int yy = 0; yy < characterHeight; yy++)
		{
			byte glyphScan = (yy < glyph.Length) ? glyph[yy] : (byte)0;

			DrawCharacterScan(x, y + yy, characterWidth, glyphScan);
		}
	}

	public override void ScrollText()
	{
		ScrollUp(
			CharacterScans,
			CharacterScans * CharacterLineWindowStart,
			CharacterScans * (CharacterLineWindowEnd + 1) - 1);
	}

	public void ScrollUp(int scanCount)
	{
		ScrollUp(scanCount, 0, Height - 1);
	}

	public abstract void ScrollUp(int scanCount, int windowStart, int windowEnd);

	protected virtual void DrawCharacterScan(int x, int y, int characterWidth, byte glyphScan)
	{
		int columnBit = 128;

		for (int xx = 0; xx < characterWidth; xx++)
		{
			int attribute = (glyphScan & columnBit) != 0
				? DrawingAttribute
				: 0;

			PixelSet(x + xx, y, attribute);

			columnBit >>= 1;
		}
	}
	#endregion Text
}
