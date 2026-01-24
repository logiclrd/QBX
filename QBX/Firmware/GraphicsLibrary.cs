using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using QBX.Hardware;

using CSharpTest.Net.Collections;

namespace QBX.Firmware;

public abstract class GraphicsLibrary : VisualLibrary
{
	protected GraphicsLibrary(Machine machine)
		: base(machine)
	{
		Window = Window.Dummy;
	}

	public byte[][] Font = CreateBlankFont();

	static byte[][] CreateBlankFont()
	{
		byte[][] ret = new byte[256][];

		ret.AsSpan().Fill(new byte[16]);

		return ret;
	}

	public int PhysicalWidth;
	public int PhysicalHeight;

	public int DrawingAttribute;
	public Window Window;
	public Point LastPoint;

	public int CharacterScans; // doesn't exist on the VGA chip in graphics modes

	public abstract int MaximumAttribute { get; }
	public abstract int PixelsPerByte { get; }

	public override void RefreshParameters()
	{
		Width = Array.MiscellaneousOutput.BasePixelWidth >> (Array.Sequencer.DotDoubling ? 1 : 0);
		Height = Array.CRTController.NumScanLines;

		PhysicalWidth = Array.MiscellaneousOutput.BasePixelWidth;
		PhysicalHeight = Height * (Array.CRTController.ScanDoubling ? 2 : 1);

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

	#region Patterns
	protected interface IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		int GetAttribute(int x, int y);
	}

	class SolidPattern(int attribute) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y) => attribute;
	}

	class TiledPattern1(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
			=> tileData[y % tileData.Length];
	}

	class TiledPattern1x2(int[] tileData) : IPattern
	{
		int even = tileData[0];
		int odd = tileData[1];

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
			=> ((y & 1) == 0) ? even : odd;
	}

	class TiledPattern1x4(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
			=> tileData[y & 3];
	}

	class TiledPattern1x8(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
			=> tileData[y & 7];
	}

	class TiledPattern1x16(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
			=> tileData[y & 15];
	}

	class TiledPattern1x32(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
			=> tileData[y & 15];
	}

	class TiledPattern1x64(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
			=> tileData[y & 31];
	}

	class TiledPattern4(int[] tileData, int ySize) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[(y % ySize) * 4 + (x & 3)];
		}
	}

	class TiledPattern4x1(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[x & 3];
		}
	}

	class TiledPattern4x2(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[(y & 1) * 4 + (x & 3)];
		}
	}

	class TiledPattern4x4(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[(y & 3) * 4 + (x & 3)];
		}
	}

	class TiledPattern4x8(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[(y & 7) * 4 + (x & 3)];
		}
	}

	class TiledPattern4x16(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[(y & 15) * 4 + (x & 3)];
		}
	}

	class TiledPattern4x32(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[(y & 31) * 4 + (x & 3)];
		}
	}

	class TiledPattern8(int[] tileData, int ySize) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[(y % ySize) * 8 + (x & 7)];
		}
	}

	class TiledPattern8x1(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[x & 7];
		}
	}

	class TiledPattern8x2(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[(y & 1) * 8 + (x & 7)];
		}
	}

	class TiledPattern8x4(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[(y & 3) * 8 + (x & 7)];
		}
	}

	class TiledPattern8x8(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[(y & 7) * 8 + (x & 7)];
		}
	}

	class TiledPattern8x16(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[(y & 15) * 8 + (x & 7)];
		}
	}

	class TiledPattern8x32(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[(y & 31) * 8 + (x & 7)];
		}
	}

	class TiledPattern8x64(int[] tileData) : IPattern
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int GetAttribute(int x, int y)
		{
			return tileData[(y & 63) * 8 + (x & 7)];
		}
	}
	#endregion

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

		PixelSet(translated.X, translated.Y, attribute);
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

	protected virtual void HorizontalLine<TPattern>(int x1, int x2, int y, TPattern pattern)
		where TPattern : IPattern
	{
		for (int x = x1; x <= x2; x++)
			PixelSet(x, y, pattern.GetAttribute(x, y));
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

		radiusY = (radiusY * PhysicalHeight + 240) / 480;

		if ((radiusX < 0) || (radiusY < 0))
			return;

		if ((radiusX == 0) && (radiusY == 0))
		{
			PixelSet(x, y, attribute);
			return;
		}

		if (radiusX == 0)
		{
			Line(x, y - radiusY, x, y + radiusY, attribute);
			return;
		}

		if (radiusY == 0)
		{
			Line(x - radiusX, y, x + radiusX, y, attribute);
			return;
		}

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
	protected virtual void PixelGetSpan(int x1, int x2, int y, Span<int> values)
	{
		for (int x = x1; x <= x2; x++)
			values[x - x1] = PixelGet(x, y);
	}

	protected virtual int PixelGetSpanFind(int x1, int x2, int y, Span<int> values, int findAttribute)
	{
		int dx = (x1 == x2) ? 1 : Math.Sign(x2 - x1);

		var spanStart = Math.Min(x1, x2);

		for (int x = x1; x != x2; x += dx)
		{
			int xx = x - spanStart;

			values[xx] = PixelGet(x, y);

			if (values[xx] == findAttribute)
				return x;
		}

		return x2 + dx;
	}

	struct Span(int x1, int x2, int y = 0, int dy = 0)
	{
		public static Span Empty => new Span(1, 0);

		public int X1 = x1;
		public int X2 = x2;

		public const int HaveNotYetScanned = int.MinValue;

		public int AlreadyScannedAndFoundBorderAtX = HaveNotYetScanned;

		public bool TryExtend = true;
		public bool PropagateUp = (dy < 0);
		public bool PropagateDown = (dy > 0);

		public readonly int Y = y;

		public int Width => X2 - X1 + 1;

		public Span Up() => new Span(X1, X2, Y - 1, dy: -1);
		public Span Down() => new Span(X1, X2, Y + 1, dy: +1);

		public Span Slice(int subX1, int subX2)
		{
			if (Intersects(subX1, subX2))
			{
				var slice = this;

				slice.X1 = Math.Max(X1, subX1);
				slice.X2 = Math.Min(X2, subX2);

				return slice;
			}
			else
				return Empty;
		}

		public bool Subsumes(Span other)
		{
			return (X1 <= other.X1) && (X2 >= other.X2);
		}

		public bool Intersects(int otherX1, int otherX2)
		{
			return (X2 >= otherX1) && (otherX2 >= X1);
		}

		public override string ToString()
		{
			if (PropagateUp && PropagateDown)
				return Y + "+/-: " + X1 + " to " + X2;
			if (PropagateUp)
				return Y + "--: " + X1 + " to " + X2;
			if (PropagateDown)
				return Y + "++: " + X1 + " to " + X2;

			return Y + ": " + X1 + " to " + X2;
		}
	}

	[return: NotNullIfNotNull(nameof(patternBytes))]
	IPattern? DecodePattern(byte[]? patternBytes)
	{
		if (patternBytes == null)
			return null;

		int bitsPerPixel =
			MaximumAttribute switch
			{
				1 => 1,
				3 => 2,
				15 => 4,
				255 => 8,

				_ => throw new Exception("Internal error")
			};

		int bytesPerRow = bitsPerPixel * PixelsPerByte / 8;

		int rows = (patternBytes.Length + bytesPerRow - 1) / bytesPerRow;

		var patternPixels = new int[PixelsPerByte * rows];
		var patternPixelsSpan = patternPixels.AsSpan();

		if (bitsPerPixel == 8)
		{
			for (int i = 0; i < patternPixels.Length; i++)
				patternPixels[i] = patternBytes[i];

			switch (rows)
			{
				case 1: return new SolidPattern(patternPixels[0]);
				case 2: return new TiledPattern1x2(patternPixels);
				case 4: return new TiledPattern1x4(patternPixels);
				case 8: return new TiledPattern1x8(patternPixels);
				case 16: return new TiledPattern1x16(patternPixels);
				case 32: return new TiledPattern1x64(patternPixels);
				case 64: return new TiledPattern1x32(patternPixels);
				default: return new TiledPattern1(patternPixels);
			}
		}
		else
		{
			switch (PixelsPerByte)
			{
				case 4:
				{
					for (int y = 0; y < rows; y++)
					{
						int rowByte = patternBytes[y];
						var rowPixels = patternPixelsSpan.Slice(y * 4, 4);

						for (int x = 0; x < PixelsPerByte; x++)
						{
							rowPixels[x] = (rowByte >> 6) & 3;
							rowByte <<= 2;
						}
					}

					switch (rows)
					{
						case 1: return new TiledPattern4x1(patternPixels);
						case 2: return new TiledPattern4x2(patternPixels);
						case 4: return new TiledPattern4x4(patternPixels);
						case 8: return new TiledPattern4x8(patternPixels);
						case 16: return new TiledPattern4x16(patternPixels);
						case 32: return new TiledPattern4x32(patternPixels);
						default: return new TiledPattern4(patternPixels, rows);
					}
				}

				case 8:
				{
					int offset = 0;

					for (int y = 0; y < rows; y++)
					{
						var rowPixels = patternPixelsSpan.Slice(y * 8, 8);

						for (int bit = 0, bitValueForThisByte = 1; bit < bitsPerPixel; bit++, bitValueForThisByte <<= 1)
						{
							int rowByte = (offset < patternBytes.Length) ? patternBytes[offset] : 0;

							for (int x = 0; x < PixelsPerByte; x++)
							{
								if ((rowByte & 128) != 0)
									rowPixels[x] |= bitValueForThisByte;

								rowByte <<= 1;
							}

							offset++;
						}
					}

					switch (rows)
					{
						case 1: return new TiledPattern8x1(patternPixels);
						case 2: return new TiledPattern8x2(patternPixels);
						case 4: return new TiledPattern8x4(patternPixels);
						case 8: return new TiledPattern8x8(patternPixels);
						case 16: return new TiledPattern8x16(patternPixels);
						case 32: return new TiledPattern8x32(patternPixels);
						case 64: return new TiledPattern8x64(patternPixels);
						default: return new TiledPattern8(patternPixels, rows);
					}
				}

				default:
					throw new Exception("Internal error: Unrecognized PixelsPerByte value " + PixelsPerByte);
			}
		}
	}

	public void BorderFill(float x, float y, int borderAttribute, int fillAttribute)
	{
		var translated = Window.TranslatePoint(x, y);

		BorderFill(translated.X, translated.Y, borderAttribute, fillAttribute);

		LastPoint = (x, y);
	}

	public void BorderFill(float x, float y, int borderAttribute, byte[] fillPatternBytes, byte[]? backgroundPatternBytes)
	{
		var translated = Window.TranslatePoint(x, y);

		BorderFill(translated.X, translated.Y, borderAttribute, fillPatternBytes, backgroundPatternBytes);

		LastPoint = (x, y);
	}

	public void BorderFill(int x, int y, int borderAttribute, int fillAttribute)
	{
		var fillPattern = new SolidPattern(fillAttribute);
		var backgroundPattern = new SolidPattern(0);

		BorderFill(x, y, borderAttribute, fillPattern, backgroundPattern);
	}

	public void BorderFill(int x, int y, int borderAttribute, byte[] fillPatternBytes, byte[]? backgroundPatternBytes)
	{
		var fillPattern = DecodePattern(fillPatternBytes);
		var backgroundPattern = DecodePattern(backgroundPatternBytes);

		backgroundPattern ??= new SolidPattern(0);

		var borderFillMethod = s_BorderFillMethodDefinition.MakeGenericMethod(fillPattern.GetType(), backgroundPattern.GetType());

		borderFillMethod.Invoke(this, [x, y, borderAttribute, fillPattern, backgroundPattern]);
	}

	static MethodInfo s_BorderFillMethodDefinition =
		typeof(GraphicsLibrary).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
		.Single(methodInfo => methodInfo.IsGenericMethodDefinition && (methodInfo.Name == nameof(BorderFill)));

	void BorderFill<TFill, TBackground>(int x, int y, int borderAttribute, TFill fillPattern, TBackground backgroundPattern)
		where TFill : IPattern
		where TBackground : IPattern
	{
		var queuedSpans = new Queue<Span>();
		var processed = new BTreeDictionary<int, int>();
		var scan = new int[Width].AsSpan();

		int width = Width;

		void AddToSet(Span span, BTreeDictionary<int, int> set)
		{
			int o = span.Y * width;

			int o1 = o + span.X1;
			int o2 = o + span.X2;

			bool checkAgain = false;

			do
			{
				foreach (var existing in set.EnumerateFrom(o1 - 1))
				{
					int existingO1 = existing.Value;
					int existingO2 = existing.Value;

					if (existingO1 <= o1)
					{
						// The existing span starts to the left of the new span and ends either within or
						// to the right of the new span.

						if (existingO2 < o2)
						{
							// existing
							//     newspan
							// `----.----'
							//    merged
							set.Remove(existingO2);
							set.Add(o2, existingO1);
						}
						// else existing completely contains the new span

						return;
					}

					if (existingO1 <= o2 + 1)
					{
						// The existing span starts within the new span and may or may not end
						// within the new span as well.

						if (existingO2 >= o2)
						{
							//     existing
							// newspan
							// `----.----'
							//    merged
							set[existingO2] = o1;

							return;
						}

						// The existing span is contained entirely in the new span.
						// There could be another one afterward that also intersects.
						// But we have to restart the loop because we modified
						// the collection.
						set.Remove(existingO2);
						checkAgain = true;
					}

					break;
				}
			}
			while (checkAgain);

			// If we escape the loop, then the new span doesn't intersect any existing spans.

			set.Add(o2, o1);
		}

		IEnumerable<Span> ExcludeSet(Span span, BTreeDictionary<int, int> set)
		{
			int o = span.Y * width;

			foreach (var setSpan in set.EnumerateFrom(o + span.X1))
			{
				int setSpanX1 = setSpan.Value - o;
				int setSpanX2 = setSpan.Key - o;

				if (!span.Intersects(setSpanX1, setSpanX2))
					break;

				if (setSpanX1 > span.X1)
					yield return span.Slice(span.X1, setSpanX1 - 1);
				if (setSpanX2 >= span.X2)
					yield break;

				span.X1 = setSpanX2 + 1;
			}

			yield return span;
		}

		// Adds the parts of the span that aren't already in the interior.
		void AddToQueue(Span newSpan)
		{
			if ((newSpan.Y < 0) || (newSpan.Y >= Height))
				return;

			foreach (var exteriorSpan in ExcludeSet(newSpan, processed))
				queuedSpans.Enqueue(newSpan);
		}

		// Start by scanning to the left and right and seeding the queue with spans to scan.
		int x1 = PixelGetSpanFind(x, 0, y, scan, borderAttribute);
		int x2 = PixelGetSpanFind(x, Width - 1, y, scan.Slice(x), borderAttribute);

		if (x1 == x) // Starting on a border pixel.
			return;

		// Don't include the border pixels themselves.
		x1++;
		x2--;

		var initialSpan = new Span(x1, x2, y);

		initialSpan.PropagateUp = true;
		initialSpan.PropagateDown = true;
		initialSpan.TryExtend = false;

		AddToQueue(initialSpan);

		// Allocate this here, because it will likely expand at some point
		// to hold more data, and that expansion requires reallocating the
		// internal array, and we should avoid doing that resize reallocation
		// multiple times.
		var newlyScanned = new List<Span>();

		bool isSolidFill = fillPattern is SolidPattern;
		int solidFillAttribute = fillPattern.GetAttribute(0, 0);

		while (queuedSpans.TryDequeue(out var potentialSpan))
		{
			// Every pixel in this span is guaranteed to be adjacent to a known interior
			// pixel. But, some of them may be border pixels. So, our job is to:
			//
			// - Identify the subspans that are not border pixels and which are thus
			//   now also known interior pixels.
			// - Queue subsequent new spans that are now known to be adjacent to known
			//   interior pixels.
			//
			// The algorithm can and does regularly queue up redundant checks that
			// are covering pixels that were already scanned, which is why we maintain
			// the "scanned" set.

			foreach (var unscannedSpan in ExcludeSet(potentialSpan, processed))
			{
				var span = unscannedSpan;

				// Gather border information in this scan. In the case of extensions,
				// we already know that every pixel in the span is going to be interior,
				// because we started at an interior pixel and walked to the next
				// border. No need to re-scan.
				if (span.AlreadyScannedAndFoundBorderAtX != Span.HaveNotYetScanned)
				{
					scan.Fill(-1);

					if ((span.AlreadyScannedAndFoundBorderAtX >= 0)
					 && (span.AlreadyScannedAndFoundBorderAtX < scan.Length))
						scan[span.AlreadyScannedAndFoundBorderAtX] = borderAttribute;
				}
				else
				{
					scan.Slice(0, span.X1).Fill(-1);
					scan.Slice(span.X2 + 1).Fill(-1);
					PixelGetSpan(span.X1, span.X2, span.Y, scan.Slice(span.X1, span.Width));
				}

				newlyScanned.Add(span);

				// Scan to the left and right of the span. If we find more interior pixels there,
				// queue spans set to propagate both up and down. (Disable extending these, though,
				// as there's no point; they're already the result of an extension and are
				// guaranteed not to find anything new past their range.)
				if (span.TryExtend)
				{
					if (scan[span.X1] != borderAttribute)
					{
						int borderX = PixelGetSpanFind(span.X1 - 1, 0, span.Y, scan, borderAttribute);

						int newX1 = borderX + 1;

						if (newX1 < span.X1)
						{
							var extension = new Span(newX1, span.X1 - 1, span.Y);

							extension.TryExtend = false;
							extension.PropagateUp = true;
							extension.PropagateDown = true;
							extension.AlreadyScannedAndFoundBorderAtX = borderX;

							AddToQueue(extension);
						}
					}

					if (scan[span.X2] != borderAttribute)
					{
						int borderX = PixelGetSpanFind(span.X2 + 1, Width - 1, span.Y, scan, borderAttribute);

						int newX2 = borderX - 1;

						if (newX2 > span.X2)
						{
							var extension = new Span(span.X2 + 1, newX2, span.Y);

							extension.TryExtend = false;
							extension.PropagateUp = true;
							extension.PropagateDown = true;
							extension.AlreadyScannedAndFoundBorderAtX = borderX;

							AddToQueue(extension);
						}
					}
				}

				// Now extract the contiguous spans of pixels to be painted and paint them.
				// Each one has a contiguous span of pixels on the adjacent row(s); queue that
				// for processing.
				x1 = span.X1;
				x2 = span.X2;

				for (x = x1; x <= x2; x++)
				{
					if ((scan[x] == borderAttribute) || (x == x2))
					{
						span.X2 = x;

						if (scan[x] == borderAttribute)
							span.X2--;

						if (span.X2 >= span.X1)
						{
							// In addition to the boundary, if a span is already entirely
							// the fill attribute, then it counts as border
							var spanScan = scan.Slice(span.X1, span.X2 - span.X1 + 1);

							bool paint = false;
							bool allBackground = true;

							for (int i = 0; i < spanScan.Length; i++)
							{
								int sx = span.X1 + i;

								if (spanScan[i] != backgroundPattern.GetAttribute(sx, span.Y))
									allBackground = false;
								if (spanScan[i] != fillPattern.GetAttribute(sx, span.Y))
									paint = true;
							}

							if (paint || allBackground)
							{
								// Don't double-paint pixels.
								if (isSolidFill)
								{
									// Less generic, more performant.
									foreach (var newInteriorSpan in ExcludeSet(span, processed))
										HorizontalLine(newInteriorSpan.X1, newInteriorSpan.X2, newInteriorSpan.Y, solidFillAttribute);
								}
								else
								{
									foreach (var newInteriorSpan in ExcludeSet(span, processed))
										HorizontalLine(newInteriorSpan.X1, newInteriorSpan.X2, newInteriorSpan.Y, fillPattern);
								}

								if (span.PropagateUp)
									AddToQueue(span.Up());

								if (span.PropagateDown)
									AddToQueue(span.Down());
							}
						}

						span.X1 = x + 1;
					}
				}
			}

			// Add these outside of the loop because adding them within the loop
			// would interfere with the loop's existing enumeration of the set.
			// All of the spans we just painted, if any, are contained within
			// these spans.
			foreach (var span in newlyScanned)
				AddToSet(span, processed);

			newlyScanned.Clear();
		}
	}
	#endregion

	#region Sprites
	protected interface ISpriteOperation
	{
		bool UsesPlaneBits { get; }

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		byte ApplySpriteBits(byte bufferByte, int spriteByte, int unrelatedMask, int spriteMask);
	}

	protected class SpriteOperation_PixelSet : ISpriteOperation
	{
		public bool UsesPlaneBits => false;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte ApplySpriteBits(byte bufferByte, int spriteByte, int unrelatedMask, int spriteMask)
		{
			return unchecked((byte)(
				(bufferByte & unrelatedMask) | (spriteByte & spriteMask)));
		}
	}

	protected class SpriteOperation_PixelSetInverted : ISpriteOperation
	{
		public bool UsesPlaneBits => false;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte ApplySpriteBits(byte bufferByte, int spriteByte, int unrelatedMask, int spriteMask)
		{
			return unchecked((byte)(
				(bufferByte & unrelatedMask) | ((~spriteByte) & spriteMask)));
		}
	}

	protected class SpriteOperation_And : ISpriteOperation
	{
		public bool UsesPlaneBits => true;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte ApplySpriteBits(byte bufferByte, int spriteByte, int unrelatedMask, int spriteMask)
		{
			return unchecked((byte)(
				(bufferByte & unrelatedMask) | ((bufferByte & spriteByte) & spriteMask)));
		}
	}

	protected class SpriteOperation_Or : ISpriteOperation
	{
		public bool UsesPlaneBits => true;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte ApplySpriteBits(byte bufferByte, int spriteByte, int unrelatedMask, int spriteMask)
		{
			return unchecked((byte)(
				(bufferByte & unrelatedMask) | ((bufferByte | spriteByte) & spriteMask)));
		}
	}

	protected class SpriteOperation_ExclusiveOr : ISpriteOperation
	{
		public bool UsesPlaneBits => true;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public byte ApplySpriteBits(byte bufferByte, int spriteByte, int unrelatedMask, int spriteMask)
		{
			return unchecked((byte)(
				(bufferByte & unrelatedMask) | ((bufferByte ^ spriteByte) & spriteMask)));
		}
	}

	public void GetSprite(float x1, float y1, float x2, float y2, Span<byte> buffer)
	{
		var pt1 = Window.TranslatePoint(x1, y1);
		var pt2 = Window.TranslatePoint(x2, y2);

		GetSprite(pt1.X, pt1.Y, pt2.X, pt2.Y, buffer);
	}

	public abstract void GetSprite(int x1, int y1, int x2, int y2, Span<byte> buffer);

	public void PutSprite(Span<byte> buffer, PutSpriteAction action, float x, float y)
	{
		var (translatedX, translatedY) = Window.TranslatePoint(x, y);

		PutSprite(buffer, action, translatedX, translatedY);
	}

	public abstract void PutSprite(Span<byte> buffer, PutSpriteAction action, int x, int y);
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
