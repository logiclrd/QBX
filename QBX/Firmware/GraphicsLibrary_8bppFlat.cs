using System;

using QBX.Hardware;

namespace QBX.Firmware;

public class GraphicsLibrary_8bppFlat : GraphicsLibrary
{
	public GraphicsLibrary_8bppFlat(GraphicsArray array)
		: base(array)
	{
		DrawingAttribute = 15;
		RefreshParameters();
	}

	public override void Clear()
	{
		Array.VRAM.AsSpan().Slice(StartAddress, Width * Height).Clear();
	}

	public override void PixelSet(int x, int y, int attribute)
	{
		if ((x >= 0) && (x < Width)
		 && (y >= 0) && (y < Height))
			Array.VRAM[StartAddress + y * Width + x] = unchecked((byte)attribute);
	}

	public override void HorizontalLine(int x1, int x2, int y, int attribute)
	{
		if ((x2 < 0) || (x1 >= Width))
			return;
		if ((y < 0) || (y >= Height))
			return;

		if (x1 < 0)
			x1 = 0;
		if (x2 >= Width)
			x2 = Width - 1;

		int o = StartAddress + y * Width + x1;

		Array.VRAM.AsSpan().Slice(o, x2 - x1 + 1).Fill(unchecked((byte)attribute));
	}
}
