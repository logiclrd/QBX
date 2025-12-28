using QBX.Hardware;

namespace QBX.Firmware;

public class GraphicsLibrary_8bppFlat(GraphicsArray array) : GraphicsLibrary(array)
{
	public override void Clear()
	{
		Array.VRAM.AsSpan().Slice(0, 320 * 200).Clear();
	}

	public override void PixelSet(int x, int y, int attribute)
	{
		if ((x >= 0) && (x < Width)
		 && (y >= 0) && (y < Height))
			Array.VRAM[Array.CRTController.StartAddress + y * Width + x] = unchecked((byte)attribute);
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

		int o = Array.CRTController.StartAddress + y * Width + x1;

		Array.VRAM.AsSpan().Slice(o, x2 - x1 + 1).Fill(unchecked((byte)attribute));
	}
}
