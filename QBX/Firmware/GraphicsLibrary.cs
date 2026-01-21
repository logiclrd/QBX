using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

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

	#region PixelGet
	public virtual int PixelGet(float x, float y)
	{
		var translated = Window.TranslatePoint(x, y);

		return PixelGet(translated.X, translated.Y);

	}

	public abstract int PixelGet(int x, int y);
	#endregion

	#region PixelSet
	public void PixelSet(float x, float y)
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

	#region Paint
	protected void Scan(int x1, int x2, int y, int findAttribute, Span<bool> matches)
	{
		for (int x = x1; x <= x2; x++)
			matches[x - x1] = PixelGet(x, y) == findAttribute;
	}

	protected int Find(int x1, int x2, int y, int findAttribute)
	{
		int dx = (x1 == x2) ? 1 : Math.Sign(x2 - x1);

		// Include x2 in the loop range.
		x2 += dx;

		for (int x = x1; x != x2; x += dx)
			if (PixelGet(x, y) == findAttribute)
				return x;

		return -1;
	}

	static StreamWriter? dbg;

	class FillAction(int x1, int x2, int y, int dy) : IComparable<FillAction>
	{
		static int s_nextSequence;

		public readonly int Sequence = s_nextSequence++;

		public readonly int Y = y;

		public int X1 = x1;
		public int X2 = x2;
		public int DY = dy;

		public int LimitRows;
		public bool LimitLeft;
		public bool LimitRight;

		public bool IsCancelled;
		public int AddedInIteration;
		public FillAction? ExcludePixelsFromAdjacentSpans;

		public int Width => X2 - X1 + 1;

		public int CompareTo(FillAction? other)
		{
			if (other == null)
				return 1;

			if (X1 != other.X1)
				return X1.CompareTo(other.X1);
			else
				return Sequence.CompareTo(other.Sequence);
		}

		public bool MergeRight(FillAction other)
		{
			if (other.X1 < X1)
				throw new InvalidOperationException();

			if (other.X1 > X2 + 1)
				return false;

			if (other.X2 > X2)
				X2 = other.X2;

			return true;
		}

		public FillAction Clone()
			=> new FillAction(X1, X2, Y, DY);

		public FillAction Clone(int newY)
			=> new FillAction(X1, X2, newY, DY);

		public void CopyTo(FillAction other)
		{
			if (Y != other.Y)
				throw new InvalidOperationException();

			other.X1 = X1;
			other.X2 = X2;
			other.DY = DY;
			other.IsCancelled = IsCancelled;
		}

		public bool Subsumes(FillAction other)
		{
			return (X1 <= other.X1) && (X2 >= other.X2);
		}

		public bool Intersects(FillAction other)
		{
			return (X2 >= other.X1) && (other.X2 >= X1);
		}

		public void Exclude(FillAction other)
		{
			if (!IsCancelled && !other.IsCancelled)
			{
				if ((other.X1 <= X1) && (other.X2 >= X2))
				{
					dbg?.WriteLine("EXCLUDE: {0} fully subsumed", this);
					X2 = X1 - 1;
				}
				else
				{
					if ((X1 >= other.X1) && (X1 <= other.X2))
					{
						dbg?.Write("EXCLUDE: {0} -> ", this);
						X1 = other.X2 + 1;
						LimitLeft = true;
						dbg?.WriteLine(this);
					}
					if ((X2 >= other.X1) && (X2 <= other.X2))
					{
						dbg?.Write("EXCLUDE: {0} -> ", this);
						X2 = other.X1 - 1;
						LimitRight = true;
						dbg?.WriteLine(this);
					}
				}

				IsCancelled = (X1 > X2);
			}
		}

		public FillAction IntersectionWith(FillAction other)
		{
			return new FillAction(
				Math.Max(X1, other.X1),
				Math.Min(X2, other.X2),
				Y,
				DY);
		}

		public override string ToString()
		{
			return (IsCancelled ? "CANCELLED " : null) + Y + (DY > 0 ? "++" : DY < 0 ? "--" : null) + ": " + X1 + " to " + X2;
		}
	}

	public void BorderFill(int x, int y, int borderAttribute, int fillAttribute)
	{
		dbg?.Close();

		File.WriteAllBytes(@"C:\code\QBX\paintdbg.init", Machine.GraphicsArray.VRAM.AsSpan().Slice(0, 64000));
		dbg = new StreamWriter(@"C:\code\QBX\paintdbg.txt") { AutoFlush = true };
		//dbg = null;

		var queuedActions = new Queue<FillAction>();
		var queuedActionsByY = new List<FillAction>[Height];
		var scan = new bool[Width].AsSpan();

		int iteration = 0;

		void AddToQueueWithException(FillAction newSpan, FillAction? exclude)
		{
			if ((exclude == null) || !newSpan.Intersects(exclude))
				AddToQueue(newSpan);
			else
			{
				var left = newSpan.Clone();
				var right = newSpan;

				left.X2 = exclude.X1 - 1;
				right.X1 = exclude.X2 + 2;

				if (left.X1 <= left.X2)
					AddToQueue(left);
				if (right.X1 <= right.X2)
					AddToQueue(right);
			}
		}

		void AddToQueue(FillAction newSpan)
		{
			if ((newSpan.Y < 0) || (newSpan.Y >= Height))
				return;

			var actionsForY = queuedActionsByY[newSpan.Y] ??= new List<FillAction>();

			bool sortActions = false;
			List<FillAction>? newActions = null;

			foreach (var queuedAction in actionsForY)
			{
				if (newSpan.Subsumes(queuedAction))
				{
					dbg?.WriteLine("EXISTING QUEUED ACTION OBVIATED: {0}", queuedAction);

					if (queuedAction.DY == newSpan.DY)
					{
						queuedAction.X1 = newSpan.X1;
						queuedAction.X2 = newSpan.X2;

						return;
					}

					queuedAction.ExcludePixelsFromAdjacentSpans = queuedAction.Clone();

					int leftPartSize = Math.Max(0, queuedAction.X1 - newSpan.X1);
					int rightPartSize = Math.Max(0, newSpan.X2 - queuedAction.X2);

					if ((leftPartSize == 0) && (rightPartSize == 0))
					{
						queuedAction.ExcludePixelsFromAdjacentSpans = newSpan;
						return; // nothing to queue; these are exactly the same size
					}

					if ((leftPartSize > 0) && (rightPartSize > 0))
					{
						// This existing queue entry takes a chunk out of the new
						// span, leaving behind some on the left and some on the
						// right.
						//
						// We want to keep the both sides, but also prevent both sides
						// from seeing the pixels covered by the new action.

						var newLeft = new FillAction(newSpan.X1, queuedAction.X1 - 1, newSpan.Y, newSpan.DY);

						newLeft.LimitRight = true;

						newSpan.X1 = queuedAction.X2 + 1;
						newSpan.LimitLeft = true;

						sortActions = true;

						newActions ??= new List<FillAction>();
						newActions.Add(newLeft);
					}
					else if (leftPartSize > 0)
					{
						dbg?.Write("TRIM: X2 {0}", newSpan.X2);

						newSpan.X2 = newSpan.X1 + leftPartSize - 1;
						newSpan.LimitRight = true;

						dbg?.WriteLine(" => {0}", newSpan.X2);
					}
					else if (rightPartSize > 0)
					{
						dbg?.Write("TRIM: X1 {0}", newSpan.X1);

						newSpan.X1 = newSpan.X2 - rightPartSize + 1;
						newSpan.LimitLeft = true;

						dbg?.WriteLine(" => {0}", newSpan.X1);

						sortActions = true;
					}
				}
				else if (queuedAction.Subsumes(newSpan))
				{
					dbg?.WriteLine("LIMITING TO ONE MORE ROW: {0} (is subsumed by existing {1})", newSpan, queuedAction);

					newSpan.DY = 0;
					newSpan.LimitLeft = true;
					newSpan.LimitRight = true;

					int leftPartSize = (newSpan.X1 > queuedAction.X1)
						? newSpan.X1 - queuedAction.X1
						: -1;

					int rightPartSize = (newSpan.X2 < queuedAction.X2)
						? queuedAction.X2 - newSpan.X2
						: -1;

					if ((leftPartSize > 0) && (rightPartSize > 0))
					{
						// The new action takes a chunk out of the existing queued action.
						// This only happens when the existing queued action came from
						// an expansion scan that extended the range in one row and then
						// queued a scan of an adjacent row.
						//
						// We want to keep the both sides, but also prevent both sides
						// from seeing the pixels covered by the new action.
						//
						// We can alter the queue entry to be one of the sides but will
						// have to add one for the other side.

						dbg?.Write("SPLIT: {0} into ", queuedAction);

						var newRight = queuedAction.Clone();

						queuedAction.X2 = newSpan.X1 - 1;
						queuedAction.LimitRight = true;

						newRight.X1 = newSpan.X2 + 1;
						newRight.LimitLeft = true;

						dbg?.WriteLine("{0} and {1}", queuedAction, newRight);

						newActions ??= new List<FillAction>();
						newActions.Add(newRight);
					}
					else if (leftPartSize > 0)
					{
						dbg?.Write("TRIM: X2 {0}", queuedAction.X2);

						queuedAction.X2 = newSpan.X1 - 1;
						queuedAction.LimitRight = true;

						dbg?.WriteLine(" => {0}", queuedAction.X2);
					}
					else if (rightPartSize > 0)
					{
						dbg?.Write("TRIM: X1 {0}", queuedAction.X1);

						queuedAction.X1 = newSpan.X2 + 1;
						queuedAction.LimitLeft = true;

						dbg?.WriteLine(" => {0}", queuedAction.X1);

						sortActions = true;
					}
					else
					{
						dbg?.WriteLine("STOP AFTER (DY = 0)");
						queuedAction.ExcludePixelsFromAdjacentSpans = newSpan.Clone();
					}
				}
				else if (newSpan.Intersects(queuedAction))
				{
					dbg?.WriteLine("HANDLING OVERLAP: {0} vs. {1}", newSpan, queuedAction);

					var overlap = queuedAction.IntersectionWith(newSpan);

					newSpan.Exclude(queuedAction);

					queuedAction.Exclude(overlap);

					overlap.DY = 0;

					dbg?.WriteLine("OVERLAP {0}, NEW SPAN {1}, QUEUED ACTION {2}", overlap, newSpan, queuedAction);

					newActions ??= new List<FillAction>();
					newActions.Add(overlap);

					sortActions = true;
				}
			}

			if (sortActions)
				actionsForY.Sort();

			if (newActions != null)
			{
				foreach (var newAction in newActions)
					AddToQueue(newAction);
			}

			int index = actionsForY.BinarySearch(newSpan);

			if (index >= 0)
				throw new Exception("Internal error: Exclusion should have eliminated key collisions");

			actionsForY.Insert(~index, newSpan);

			queuedActions.Enqueue(newSpan);

			newSpan.AddedInIteration = iteration;

			dbg?.WriteLine("QUEUE: " + newSpan);
		}

		// Start by scanning to the left and right and seeding the queue with spans to scan.
		int x1 = Find(x, 0, y, borderAttribute);
		int x2 = Find(x, Width - 1, y, borderAttribute);

		if (x1 == x) // Starting on a border pixel.
			return;

		// Don't include the border pixels themselves.
		x1++;

		if (x2 >= 0)
			x2--;
		else
			x2 = Width - 1;

		var scanUp = new FillAction(x1, x2, y, -1);
		var scanDown = new FillAction(x1, x2, y, +1);

		AddToQueue(scanUp);

		while (queuedActions.TryDequeue(out var action))
		{
			iteration++;

			dbg?.WriteLine();
			dbg?.WriteLine("[" + iteration + "] DEQUEUE: " + action);
			foreach (var upcoming in queuedActions)
				dbg?.WriteLine("- " + upcoming);

			var actionsForY = queuedActionsByY[action.Y];

			int index = actionsForY.BinarySearch(action);

			if (index >= 0)
				actionsForY.RemoveAt(index);
			else
				index = 0; // Leave index pointing at the next action, if any

			if (action.IsCancelled)
				continue;

			const int ZoomCenterX = 160;
			const int ZoomCenterY = 100;
			const int ZoomScale = 2;

			for (int j = 160; j < 200; j++)
			{
				for (int i = 0; i < 320; i++)
				{
					int dx = i - 160;
					int dy = j - 180;

					dx = dx / ZoomScale;
					dy = dy / ZoomScale;

					dx = dx + ZoomCenterX;
					dy = dy + ZoomCenterY;

					PixelSet(i, j, PixelGet(dx, dy));
				}
			}

			int TX(int x) => (x - ZoomCenterX) * ZoomScale + 160;
			int TY(int y) => (y - ZoomCenterY) * ZoomScale + 180;

			void DebugPlot(FillAction queuedAct, bool current)
			{
				int qy = TY(queuedAct.Y);
				if (qy < 160)
					return;
				int qx1 = TX(queuedAct.X1);
				int qx2 = TX(queuedAct.X2);

				int a = current ? 12 : queuedAct.IsCancelled ? 8 : (queuedAct.DY > 0) ? 11 : (queuedAct.DY < 0) ? 14 : 7;

				for (int i = qx1; i <= qx2; i++)
					PixelSet(i, qy, a);
			}

			foreach (var queuedAct in queuedActions)
				DebugPlot(queuedAct, current: false);

			DebugPlot(action, current: true);

			System.Threading.Thread.Sleep(50);

			if (iteration >= 193)
				Debugger.Break();

			scan.Slice(0, action.X1).Clear();
			scan.Slice(action.X2 + 1).Clear();
			Scan(action.X1, action.X2, action.Y, borderAttribute, scan.Slice(action.X1, action.Width));

			if (!action.LimitLeft && !scan[action.X1])
			{
				int newX1 = Find(action.X1 - 1, 0, action.Y, borderAttribute);

				if (newX1 >= 0)
				{
					scan[newX1] = true;
					newX1++;
				}
				else
					newX1 = 0;

				dbg?.WriteLine("extend left to " + newX1);

				int checkX1 = action.X1;

				int i = index;

				while (i > 0)
				{
					i--;

					if (actionsForY[i].IsCancelled)
						continue;

					if ((actionsForY[i].X2 >= newX1) && (actionsForY[i].X1 <= checkX1))
					{
						dbg?.WriteLine("TRIM OVERLAP LEFT");

						actionsForY[i].LimitRight = true;

						if (actionsForY[i].DY != action.DY)
						{
							dbg?.WriteLine("COLLISION");

							newX1 = actionsForY[i].X2 + 1;

							break;
						}
						else
						{
							//  ^------^ ^--------^
							//         3 5

							int pixelsExposed = action.X1 - actionsForY[i].X2 - 1;

							if (pixelsExposed > 0)
							{
								var reverse = new FillAction(actionsForY[i].X2 + 1, action.X1 - 1, action.Y - action.DY, -action.DY);

								dbg?.WriteLine("generated reverse span: " + reverse);

								AddToQueueWithException(reverse, action.ExcludePixelsFromAdjacentSpans);
							}

							dbg?.WriteLine("MERGED: {0}", actionsForY[i]);

							action.X1 = actionsForY[i].X1;

							if (actionsForY[i].X1 >= newX1)
								actionsForY[i].IsCancelled = true;
							else
								actionsForY[i].X2 = newX1 - 1;
						}
					}
				}

				if ((newX1 < action.X1) && (action.DY != 0))
				{
					var reverse = new FillAction(newX1, action.X1 - 1, action.Y - action.DY, -action.DY);

					reverse.LimitRight = true;

					dbg?.WriteLine("generated reverse span: " + reverse);

					AddToQueueWithException(reverse, action.ExcludePixelsFromAdjacentSpans);
				}

				action.X1 = Math.Max(0, newX1);
			}

			if (!action.LimitRight && !scan[action.X2])
			{
				int newX2 = Find(action.X2 + 1, Width - 1, action.Y, borderAttribute);

				if (newX2 >= 0)
				{
					scan[newX2] = true;
					newX2--;
				}
				else
					newX2 = Width - 1;

				dbg?.WriteLine("extend right to " + newX2);

				int checkX2 = action.X2;

				// We have already removed action, so the indices are shifted left.
				for (int i = index; i < actionsForY.Count; i++)
				{
					if (actionsForY[i].IsCancelled)
						continue;

					if ((actionsForY[i].X1 <= newX2) && (actionsForY[i].X2 >= checkX2))
					{
						dbg?.WriteLine("TRIM OVERLAP RIGHT");

						actionsForY[i].LimitLeft = true;

						if (actionsForY[i].DY != action.DY)
						{
							dbg?.WriteLine("COLLISION");

							if (actionsForY[i].X2 <= newX2)
							{
								if (actionsForY[i].DY == 0)
									actionsForY[i].DY = action.DY;
								if (action.DY == 0)
									action.DY = actionsForY[i].DY;
							}

							newX2 = actionsForY[i].X1 - 1;

							break;
						}
						else 
						{
							//  ^------^ ^--------^
							//         3 5

							int pixelsExposed = actionsForY[i].X1 - action.X2 - 1;

							if (pixelsExposed > 0)
							{
								var reverse = new FillAction(action.X2 + 1, actionsForY[i].X1 - 1, action.Y - action.DY, -action.DY);

								dbg?.WriteLine("generated reverse span: " + reverse);

								AddToQueueWithException(reverse, action.ExcludePixelsFromAdjacentSpans);
							}

							dbg?.WriteLine("MERGED: {0}", actionsForY[i]);

							action.X2 = actionsForY[i].X2;

							if (actionsForY[i].X2 <= newX2)
								actionsForY[i].IsCancelled = true;
							else
								actionsForY[i].X1 = newX2 + 1;
						}
					}
				}

				if ((newX2 > action.X2) && (action.DY != 0))
				{
					var reverse = new FillAction(action.X2 + 1, newX2, action.Y - action.DY, -action.DY);

					reverse.LimitLeft = true;

					dbg?.WriteLine("generated reverse span: " + reverse);

					AddToQueueWithException(reverse, action.ExcludePixelsFromAdjacentSpans);
				}

				action.X2 = Math.Min(newX2, Width - 1);
			}

			// Now that we've broadened the connected span as much as possible, eliminate
			// other queued actions that would check the same pixels.
			if (action != scanUp)
			{
				foreach (var otherAction in actionsForY)
					if (otherAction != action)
						otherAction.Exclude(action);

				actionsForY.Sort();
			}

			// Now extract the contiguous spans of pixels to be painted and paint them.
			// Each one has a contiguous span of pixels on the adjacent row; queue that
			// row (per the action's DY) for processing.
			var span = action.Clone();

			for (x = action.X1; x <= action.X2; x++)
			{
				if ((scan[x] == true) || (x == action.X2))
				{
					span.X2 = x;

					if (scan[x])
						span.X2--;

					if (span.X2 >= span.X1)
					{
						dbg?.WriteLine("PAINT: {0} {1}..{2}", action.Y, span.X1, span.X2);

						HorizontalLine(span.X1, span.X2, action.Y, fillAttribute);

						if ((action.DY != 0) && (action.LimitRows != 1))
						{
							int ny = action.Y + action.DY;

							if ((ny >= 0) && (ny < queuedActionsByY.Length))
							{
								var nextSpan = span.Clone(action.Y + action.DY);

								if (action.LimitRows > 1)
									nextSpan.LimitRows = action.LimitRows - 1;

								AddToQueueWithException(nextSpan, action.ExcludePixelsFromAdjacentSpans);
							}
						}
					}

					span.X1 = x + 1;
				}
			}

			// The seed scans scanUp and scanDown overlap, so scanDown needs to be delayed
			// until after scanUp has finished processing.
			if (scanDown != null)
			{
				AddToQueue(scanDown);
				scanDown = null;
			}
		}
	}
	#endregion

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
