using System;

using QBX.Hardware;

namespace QBX.Firmware;

public class GraphicsLibrary_1bppPacked : GraphicsLibrary
{
	public GraphicsLibrary_1bppPacked(Machine machine)
		: base(machine)
	{
		DrawingAttribute = 1;
		RefreshParameters();
	}

	const int PlaneSize = 65536;

	int _planeBytesUsed;
	int _stride;
	int _plane0Offset;
	int _plane1Offset;
	int _plane2Offset;
	int _plane3Offset;

	public override void RefreshParameters()
	{
		base.RefreshParameters();

		_plane0Offset = StartAddress;
		_plane1Offset = _plane0Offset + PlaneSize;
		_plane2Offset = _plane1Offset + PlaneSize;
		_plane3Offset = _plane2Offset + PlaneSize;

		_stride = Width / 8;
		_planeBytesUsed = Height * _stride;
	}

	protected override void ClearGraphicsImplementation(int windowStart, int windowEnd)
	{
		var vramSpan = Array.VRAM.AsSpan();

		int planeMask = Array.Graphics.Registers.BitMask;

		int windowOffset = windowStart * _stride;
		int windowLength = (windowEnd - windowStart + 1) * _stride;

		if ((planeMask & 1) != 0)
			vramSpan.Slice(_plane0Offset + windowOffset, windowLength).Clear();
		if ((planeMask & 2) != 0)
			vramSpan.Slice(_plane1Offset + windowOffset, windowLength).Clear();
		if ((planeMask & 4) != 0)
			vramSpan.Slice(_plane2Offset + windowOffset, windowLength).Clear();
		if ((planeMask & 8) != 0)
			vramSpan.Slice(_plane3Offset + windowOffset, windowLength).Clear();
	}

	public override void PixelSet(int x, int y, int attribute)
	{
		if ((x >= 0) && (x < Width)
		 && (y >= 0) && (y < Height))
		{
			int offset = y * _stride + (x >> 3);
			int shift = 7 - (x & 7);
			int bitMask = 1 << shift;

			int planeMask = Array.Graphics.Registers.BitMask;

			if ((planeMask & 1) != 0)
			{
				Array.VRAM[_plane0Offset + offset] = unchecked((byte)(
					(Array.VRAM[_plane0Offset + offset] & ~bitMask) |
					((attribute & 1) << shift)));
			}

			if ((planeMask & 2) != 0)
			{
				Array.VRAM[_plane1Offset + offset] = unchecked((byte)(
					(Array.VRAM[_plane1Offset + offset] & ~bitMask) |
					((attribute & 1) << shift)));
			}

			if ((planeMask & 4) != 0)
			{
				Array.VRAM[_plane2Offset + offset] = unchecked((byte)(
					(Array.VRAM[_plane2Offset + offset] & ~bitMask) |
					((attribute & 1) << shift)));
			}

			if ((planeMask & 8) != 0)
			{
				Array.VRAM[_plane3Offset + offset] = unchecked((byte)(
					(Array.VRAM[_plane3Offset + offset] & ~bitMask) |
					((attribute & 1) << shift)));
			}
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

		int planeMask = Array.Graphics.Registers.BitMask;

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

				if ((planeMask & 1) != 0)
				{
					if (attributeValue)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
				}

				address += PlaneSize;

				if ((planeMask & 2) != 0)
				{
					if (attributeValue)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
				}

				address += PlaneSize;

				if ((planeMask & 4) != 0)
				{
					if (attributeValue)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
				}

				address += PlaneSize;

				if ((planeMask & 8) != 0)
				{
					if (attributeValue)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
				}
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
			{
				if ((planeMask & 1) != 0)
					vramSpan.Slice(_plane0Offset + scanOffset + firstCompleteByte, completeBytes).Fill(completeByteValue);
				if ((planeMask & 2) != 0)
					vramSpan.Slice(_plane1Offset + scanOffset + firstCompleteByte, completeBytes).Fill(completeByteValue);
				if ((planeMask & 4) != 0)
					vramSpan.Slice(_plane2Offset + scanOffset + firstCompleteByte, completeBytes).Fill(completeByteValue);
				if ((planeMask & 8) != 0)
					vramSpan.Slice(_plane3Offset + scanOffset + firstCompleteByte, completeBytes).Fill(completeByteValue);
			}

			if (leftPixels != 0)
			{
				byte mask = unchecked((byte)(0b11111111 >> (8 - leftPixels)));

				int address = _plane0Offset + scanOffset + firstCompleteByte - 1;

				if ((planeMask & 1) != 0)
				{
					if (attributeValue)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
				}

				address += PlaneSize;

				if ((planeMask & 2) != 0)
				{
					if (attributeValue)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
				}

				address += PlaneSize;

				if ((planeMask & 4) != 0)
				{
					if (attributeValue)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
				}

				address += PlaneSize;

				if ((planeMask & 8) != 0)
				{
					if (attributeValue)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
				}
			}

			if (rightPixels != 0)
			{
				byte mask = unchecked((byte)(0b11111111 << (8 - rightPixels)));

				int address = _plane0Offset + scanOffset + lastCompleteByte + 1;

				if ((planeMask & 1) != 0)
				{
					if (attributeValue)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
				}

				address += PlaneSize;

				if ((planeMask & 2) != 0)
				{
					if (attributeValue)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
				}

				address += PlaneSize;

				if ((planeMask & 4) != 0)
				{
					if (attributeValue)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
				}

				address += PlaneSize;

				if ((planeMask & 8) != 0)
				{
					if (attributeValue)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
				}
			}
		}
	}

	public override void ScrollUp(int scanCount, int windowStart, int windowEnd)
	{
		var vramSpan = Array.VRAM.AsSpan();

		int planeMask = Array.Graphics.Registers.BitMask;

		int copyOffset = scanCount * _stride;

		int windowOffset = windowStart * _stride;
		int windowLength = (windowEnd - windowStart + 1) * _stride;

		if ((planeMask & 1) != 0)
		{
			var plane = vramSpan.Slice(_plane0Offset, _planeBytesUsed);

			plane = plane.Slice(windowOffset, windowLength);

			plane.Slice(copyOffset).CopyTo(plane);
			plane.Slice(plane.Length - copyOffset).Fill(0);
		}

		if ((planeMask & 2) != 0)
		{
			var plane = vramSpan.Slice(_plane1Offset, _planeBytesUsed);

			plane = plane.Slice(windowOffset, windowLength);

			plane.Slice(copyOffset).CopyTo(plane);
			plane.Slice(plane.Length - copyOffset).Fill(0);
		}

		if ((planeMask & 4) != 0)
		{
			var plane = vramSpan.Slice(_plane2Offset, _planeBytesUsed);

			plane = plane.Slice(windowOffset, windowLength);

			plane.Slice(copyOffset).CopyTo(plane);
			plane.Slice(plane.Length - copyOffset).Fill(0);
		}

		if ((planeMask & 8) != 0)
		{
			var plane = vramSpan.Slice(_plane3Offset, _planeBytesUsed);

			plane = plane.Slice(windowOffset, windowLength);

			plane.Slice(copyOffset).CopyTo(plane);
			plane.Slice(plane.Length - copyOffset).Fill(0);
		}
	}

	protected override void DrawCharacterScan(int x, int y, int characterWidth, byte glyphScan)
	{
		if ((x & 7) != 0)
			base.DrawCharacterScan(x, y, characterWidth, glyphScan);
		else
		{
			int o = y * _stride + x >> 3;

			if ((o >= 0) && (o < _planeBytesUsed))
			{
				var vramSpan = Array.VRAM.AsSpan();

				int planeMask = Array.Graphics.Registers.BitMask;

				if ((planeMask & 1) != 0)
					vramSpan.Slice(_plane0Offset, _planeBytesUsed)[o] = glyphScan;
				if ((planeMask & 2) != 0)
					vramSpan.Slice(_plane1Offset, _planeBytesUsed)[o] = glyphScan;
				if ((planeMask & 4) != 0)
					vramSpan.Slice(_plane2Offset, _planeBytesUsed)[o] = glyphScan;
				if ((planeMask & 8) != 0)
					vramSpan.Slice(_plane3Offset, _planeBytesUsed)[o] = glyphScan;
			}
		}
	}
}
