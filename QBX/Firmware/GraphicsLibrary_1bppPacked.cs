using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using QBX.Hardware;

namespace QBX.Firmware;

public class GraphicsLibrary_1bppPacked : GraphicsLibrary
{
	public GraphicsLibrary_1bppPacked(GraphicsArray array)
		: base(array)
	{
		RefreshParameters();
	}

	int _planeBytesUsed;
	int _stride;
	int _plane0Offset;

	public override void RefreshParameters()
	{
		base.RefreshParameters();

		_plane0Offset = Array.CRTController.StartAddress;

		_stride = Width / 8;
		_planeBytesUsed = Height * _stride;
	}

	public override void Clear()
	{
		var vramSpan = Array.VRAM.AsSpan();

		vramSpan.Slice(_plane0Offset, _planeBytesUsed).Clear();
	}

	public override void PixelSet(int x, int y, int attribute)
	{
		if ((x >= 0) && (x < Width)
		 && (y >= 0) && (y < Height))
		{
			int offset = y * _stride + (x >> 3);
			int shift = 7 - (x & 7);
			int bitMask = 1 << shift;

			Array.VRAM[_plane0Offset + offset] = unchecked((byte)(
				(Array.VRAM[_plane0Offset + offset] & ~bitMask) |
				((attribute & 1) << shift)));
		}
	}

	public override void HorizontalLine(int x1, int x2, int y, int attribute)
	{
		if ((x2 < 0) || (x1 >= Width))
			return;
		if ((y < 0) || (y >= Height))
			return;

		if (x1 > x2)
			return;

		if (x1 < 0)
			x1 = 0;
		if (x2 >= Width)
			x2 = Width - 1;

		int scanOffset = y * _stride;

		bool attributeValue = (attribute & 1) != 0;
		byte completeByteValue = unchecked((byte)((attribute & 1) * 0xFF));

		int x1Byte = x1 >> 3;
		int x2Byte = x2 >> 3;

		int x1Index = x1 & 7;
		int x2Index = x2 & 7;

		int leftPixels = 8 - x1Index;
		int rightPixels = 1 + x2Index;

		if (x1Byte == x2Byte)
		{
			byte leftMask = unchecked((byte)(0b11111111 >> (8 - leftPixels)));
			byte rightMask = unchecked((byte)(0b11111111 << (8 - rightPixels)));

			byte mask = unchecked((byte)(leftMask & rightMask));

			if (mask != 0)
			{
				int address = _plane0Offset + scanOffset + x1Byte;

				if (attributeValue)
					Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
				else
					Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
			}
		}
		else
		{
			if (x1Index == 0)
				leftPixels = 0;
			if (x2Index == 7)
				rightPixels = 0;

			int firstCompleteByte =
				x1Byte + ((x1Index == 0) ? 0 : 1);
			int lastCompleteByte =
				x2Byte - ((x2Index == 7) ? 0 : 1);

			var vramSpan = Array.VRAM.AsSpan();

			int completeBytes = lastCompleteByte - firstCompleteByte + 1;

			if (completeBytes > 0)
				vramSpan.Slice(_plane0Offset + scanOffset + firstCompleteByte, completeBytes).Fill(completeByteValue);

			if (leftPixels != 0)
			{
				byte mask = unchecked((byte)(0b11111111 >> (8 - leftPixels)));

				int address = _plane0Offset + scanOffset + firstCompleteByte - 1;

				if (attributeValue)
					Array.VRAM[address] = unchecked((byte)((Array.VRAM[address] | mask)));
				else
					Array.VRAM[address] = unchecked((byte)((Array.VRAM[address] & ~mask)));
			}

			if (rightPixels != 0)
			{
				byte mask = unchecked((byte)(0b11111111 << (8 - rightPixels)));

				int address = _plane0Offset + scanOffset + lastCompleteByte + 1;

				if (attributeValue)
					Array.VRAM[address] = unchecked((byte)((Array.VRAM[address] | mask)));
				else
					Array.VRAM[address] = unchecked((byte)((Array.VRAM[address] & ~mask)));
			}
		}
	}
}
