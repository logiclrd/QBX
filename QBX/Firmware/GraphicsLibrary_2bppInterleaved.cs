using QBX.Hardware;

namespace QBX.Firmware;

public class GraphicsLibrary_2bppInterleaved : GraphicsLibrary
{
	public GraphicsLibrary_2bppInterleaved(GraphicsArray array)
		: base(array)
	{
		RefreshParameters();
	}

	int _planeBytesUsed;
	int _stride;
	int _plane0Offset;
	int _plane1Offset;
	int _plane2Offset;
	int _plane3Offset;

	public override void RefreshParameters()
	{
		int planeSize =
			Array.CRTController.InterleaveOnBit0 ? 8192
			: Array.CRTController.InterleaveOnBit1 ? 16384
			: 65536;

		_plane0Offset = Array.CRTController.StartAddress;
		_plane1Offset = _plane0Offset + planeSize;
		_plane2Offset = _plane1Offset + planeSize;
		_plane3Offset = _plane2Offset + planeSize;

		int scansInEachPlane = Height / 2;

		_stride = Width / 8;

		_planeBytesUsed = scansInEachPlane * _stride;
	}

	public override void Clear()
	{
		var vramSpan = Array.VRAM.AsSpan();

		vramSpan.Slice(_plane0Offset, _planeBytesUsed).Clear();
		vramSpan.Slice(_plane1Offset, _planeBytesUsed).Clear();
		vramSpan.Slice(_plane2Offset, _planeBytesUsed).Clear();
		vramSpan.Slice(_plane3Offset, _planeBytesUsed).Clear();
	}

	public override void PixelSet(int x, int y, int attribute)
	{
		if ((x >= 0) && (x < Width)
		 && (y >= 0) && (y < Height))
		{
			attribute &= 3;

			bool evenColumn = ((x & 1) == 0);
			bool evenScan = ((y & 1) == 0);

			int planeOffset = evenScan
				? (evenColumn ? _plane0Offset : _plane2Offset)
				: (evenColumn ? _plane1Offset : _plane3Offset);

			int offset = (y >> 1) * _stride + (x >> 3);
			int shift = 6 - ((x >> 1) & 3) * 2;
			int bitMask = 0b11 << shift;

			int address = planeOffset + offset;

			Array.VRAM[address] = unchecked((byte)(
				(Array.VRAM[address] & ~bitMask) |
				(attribute << shift)));
		}
	}
/*
	public override void HorizontalLine(int x1, int x2, int y, int attribute)
	{
		if ((x2 < 0) || (x1 >= Width))
			return;
		if ((y < 0) || (y >= Height))
			return;

		int o = y * Width + x1;

		Array.VRAM.AsSpan().Slice(o, x2 - x1 + 1).Fill(unchecked((byte)attribute));
	}
*/
}
