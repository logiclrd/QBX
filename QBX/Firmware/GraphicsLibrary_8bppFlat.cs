using QBX.Hardware;

namespace QBX.Firmware;

public class GraphicsLibrary_8bppFlat(GraphicsArray array) : GraphicsLibrary(array)
{
	public override void Clear()
	{
		//Array.VRAM.AsSpan().Slice(0, 320 * 200).Clear();
		for (int i = 0; i < 320 * 200; i++)
			Array.VRAM[i] = 0;
	}

	public override void PixelSet(int x, int y, int attribute)
	{
		if ((x >= 0) && (x < Width)
		 && (y >= 0) && (y < Height))
			Array.VRAM[y * Width + x] = unchecked((byte)attribute);
	}

	public override void HorizontalLine(int x1, int x2, int y, int attribute)
	{
		if ((x2 < 0) || (x1 >= Width))
			return;
		if ((y < 0) || (y >= Height))
			return;

		int o = y * Width + x1;

		Array.VRAM.AsSpan().Slice(o, x2 - x1 + 1).Fill(unchecked((byte)attribute));
	}
}
