using System;

using QBX.Hardware;

namespace QBX.Firmware;

public class GraphicsLibrary_8bppFlat : GraphicsLibrary
{
	public GraphicsLibrary_8bppFlat(Machine machine)
		: base(machine)
	{
		DrawingAttribute = 15;
		RefreshParameters();
	}

	protected override void ClearGraphicsImplementation(int windowStart, int windowEnd)
	{
		int windowOffset = windowStart * Width;
		int windowLength = (windowEnd - windowStart + 1) * Width;

		Array.VRAM.AsSpan().Slice(StartAddress + windowOffset, windowLength).Clear();
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

	public override void ScrollUp(int scanCount, int windowStart, int windowEnd)
	{
		int windowOffset = windowStart * Width;
		int windowLength = (windowEnd - windowStart + 1) * Width;

		int copyOffset = scanCount * Width;
		int copySize = windowLength - copyOffset;

		var vram = Array.VRAM.AsSpan().Slice(StartAddress + windowOffset, windowLength);

		vram.Slice(copyOffset).CopyTo(vram);
		vram.Slice(copySize).Fill(0);
	}

	protected override void DrawCharacterScan(int x, int y, int characterWidth, byte glyphScan)
	{
		int o = y * Width + x;

		int characterBit = 128;

		var vram = Array.VRAM.AsSpan().Slice(StartAddress);

		if (x + characterWidth > Width)
			characterWidth = Width - x;

		byte colour = unchecked((byte)DrawingAttribute);

		for (int xx = 0; xx < characterWidth; xx++)
		{
			if ((glyphScan & characterBit) != 0)
				vram[o] = colour;
			else
				vram[o] = 0;

			o++;
			characterBit >>= 1;
		}
	}
}
