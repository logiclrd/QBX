using System;
using System.Runtime.CompilerServices;

using QBX.Hardware;

namespace QBX.Firmware;

public class GraphicsLibrary_4bppPlanar : GraphicsLibrary
{
	public GraphicsLibrary_4bppPlanar(GraphicsArray array)
		: base(array)
	{
		DrawingAttribute = 15;
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
		base.RefreshParameters();

		const int PlaneSize = 65536;

		_plane0Offset = StartAddress;
		_plane1Offset = _plane0Offset + PlaneSize;
		_plane2Offset = _plane1Offset + PlaneSize;
		_plane3Offset = _plane2Offset + PlaneSize;

		_stride = Width / 8;
		_planeBytesUsed = Height * _stride;
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
			int offset = y * _stride + (x >> 3);
			int shift = 7 - (x & 7);
			int bitMask = 1 << shift;

			Array.VRAM[_plane0Offset + offset] = unchecked((byte)(
				(Array.VRAM[_plane0Offset + offset] & ~bitMask) |
				((attribute & 1) << shift)));

			Array.VRAM[_plane1Offset + offset] = unchecked((byte)(
				(Array.VRAM[_plane1Offset + offset] & ~bitMask) |
				((attribute & 2) >> 1 << shift)));

			Array.VRAM[_plane2Offset + offset] = unchecked((byte)(
				(Array.VRAM[_plane2Offset + offset] & ~bitMask) |
				((attribute & 4) >> 2 << shift)));

			Array.VRAM[_plane3Offset + offset] = unchecked((byte)(
				(Array.VRAM[_plane3Offset + offset] & ~bitMask) |
				((attribute & 8) >> 3 << shift)));
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

			void SetByte(int planeOffset, int offset, byte leftMask, byte rightMask, int attributeBit)
			{
				byte mask = unchecked((byte)(leftMask & rightMask));

				if (mask != 0)
				{
					int address = planeOffset + scanOffset + offset;

					if ((attribute & attributeBit) == 0)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
				}
			}

			SetByte(_plane0Offset, x1Byte, leftMask, rightMask, 1);
			SetByte(_plane1Offset, x1Byte, leftMask, rightMask, 2);
			SetByte(_plane2Offset, x1Byte, leftMask, rightMask, 4);
			SetByte(_plane3Offset, x1Byte, leftMask, rightMask, 8);
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
				vramSpan.Slice(_plane0Offset + scanOffset + firstCompleteByte, completeBytes)
					.Fill(((attribute & 1) == 0) ? (byte)0 : (byte)255);
				vramSpan.Slice(_plane1Offset + scanOffset + firstCompleteByte, completeBytes)
					.Fill(((attribute & 2) == 0) ? (byte)0 : (byte)255);
				vramSpan.Slice(_plane2Offset + scanOffset + firstCompleteByte, completeBytes)
					.Fill(((attribute & 4) == 0) ? (byte)0 : (byte)255);
				vramSpan.Slice(_plane3Offset + scanOffset + firstCompleteByte, completeBytes)
					.Fill(((attribute & 8) == 0) ? (byte)0 : (byte)255);
			}

			void SetLeftPixels(int planeOffset, int offset, int count, int attributeBit)
			{
				if (count == 0)
					return;

				byte mask = unchecked((byte)(0b11111111 >> (8 - count)));

				int address = planeOffset + scanOffset + offset - 1;

				if ((attribute & attributeBit) == 0)
					Array.VRAM[address] = unchecked((byte)((Array.VRAM[address] & ~mask)));
				else
					Array.VRAM[address] = unchecked((byte)((Array.VRAM[address] | mask)));
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void SetRightPixels(int planeOffset, int offset, int count, int attributeBit)
			{
				if (count == 0)
					return;

				byte mask = unchecked((byte)(0b11111111 << (8 - count)));

				int address = planeOffset + scanOffset + offset + 1;

				if ((attribute & attributeBit) == 0)
					Array.VRAM[address] = unchecked((byte)((Array.VRAM[address] & ~mask)));
				else
					Array.VRAM[address] = unchecked((byte)((Array.VRAM[address] | mask)));
			}

			SetLeftPixels(_plane0Offset, firstCompleteByte, leftPixels, 1);
			SetLeftPixels(_plane1Offset, firstCompleteByte, leftPixels, 2);
			SetLeftPixels(_plane2Offset, firstCompleteByte, leftPixels, 4);
			SetLeftPixels(_plane3Offset, firstCompleteByte, leftPixels, 8);

			SetRightPixels(_plane0Offset, lastCompleteByte, rightPixels, 1);
			SetRightPixels(_plane1Offset, lastCompleteByte, rightPixels, 2);
			SetRightPixels(_plane2Offset, lastCompleteByte, rightPixels, 4);
			SetRightPixels(_plane3Offset, lastCompleteByte, rightPixels, 8);
		}
	}
}
