using System;
using System.Runtime.CompilerServices;

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
		base.RefreshParameters();

		int planeSize =
			Array.CRTController.InterleaveOnBit0 ? 8192
			: Array.CRTController.InterleaveOnBit1 ? 16384
			: 65536;

		_plane0Offset = StartAddress;
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

	public override void HorizontalLine(int x1, int x2, int y, int attribute)
	{
		if ((x2 < 0) || (x1 >= Width))
			return;
		if ((y < 0) || (y >= Height))
			return;

		if (x1 > x2)
			return;

		attribute &= 3;

		if (x1 < 0)
			x1 = 0;
		if (x2 >= Width)
			x2 = Width - 1;

		bool evenScan = ((y & 1) == 0);

		int baseOffset = (y >> 1) * _stride;

		byte completeByteValue = unchecked((byte)(attribute * 0x55));

		int firstPlaneOffset = baseOffset + (evenScan ? _plane0Offset : _plane1Offset);
		int secondPlaneOffset = baseOffset + (evenScan ? _plane2Offset : _plane3Offset);

		int x1Byte = x1 >> 3;
		int x2Byte = x2 >> 3;

		int x1Index = x1 & 7;
		int x2Index = x2 & 7;

		int firstPlaneLeftPixels = 4 - ((x1Index + 1) >> 1);
		int secondPlaneLeftPixels = 4 - (x1Index >> 1);

		int firstPlaneRightPixels = 1 + (x2Index >> 1);
		int secondPlaneRightPixels = ((x2Index + 1) >> 1);

		if (x1Byte == x2Byte)
		{
			byte firstPlaneLeftMask = unchecked((byte)(0b11111111 >> (8 - firstPlaneLeftPixels * 2)));
			byte firstPlaneRightMask = unchecked((byte)(0b11111111 << (8 - firstPlaneRightPixels * 2)));

			byte secondPlaneLeftMask = unchecked((byte)(0b11111111 >> (8 - secondPlaneLeftPixels * 2)));
			byte secondPlaneRightMask = unchecked((byte)(0b11111111 << (8 - secondPlaneRightPixels * 2)));

			void SetByte(int planeOffset, int offset, byte leftMask, byte rightMask)
			{
				byte mask = unchecked((byte)(leftMask & rightMask));

				if (mask != 0)
				{
					int address = planeOffset + offset;

					Array.VRAM[address] = unchecked((byte)(
						(Array.VRAM[address] & ~mask) |
						(completeByteValue & mask)));
				}
			}

			SetByte(firstPlaneOffset, x1Byte, firstPlaneLeftMask, firstPlaneRightMask);
			SetByte(secondPlaneOffset, x1Byte, secondPlaneLeftMask, secondPlaneRightMask);
		}
		else
		{
			if (x1Index == 0)
				firstPlaneLeftPixels = 0;
			if (x1Index <= 1)
				secondPlaneLeftPixels = 0;

			if (x2Index >= 6)
				firstPlaneRightPixels = 0;
			if (x2Index == 7)
				secondPlaneRightPixels = 0;

			int firstPlaneFirstCompleteByte =
				x1Byte + ((x1Index == 0) ? 0 : 1);
			int firstPlaneLastCompleteByte =
				x2Byte - ((x2Index >= 6) ? 0 : 1);

			int secondPlaneFirstCompleteByte =
				x1Byte + ((x1Index <= 1) ? 0 : 1);
			int secondPlaneLastCompleteByte =
				x2Byte - ((x2Index == 7) ? 0 : 1);

			var vramSpan = Array.VRAM.AsSpan();

			int firstPlaneCompleteBytes = firstPlaneLastCompleteByte - firstPlaneFirstCompleteByte + 1;
			int secondPlaneCompleteBytes = secondPlaneLastCompleteByte - secondPlaneFirstCompleteByte + 1;

			if (firstPlaneCompleteBytes > 0)
			{
				vramSpan.Slice(firstPlaneOffset + firstPlaneFirstCompleteByte, firstPlaneCompleteBytes)
					.Fill(completeByteValue);
			}

			if (secondPlaneCompleteBytes > 0)
			{
				vramSpan.Slice(secondPlaneOffset + secondPlaneFirstCompleteByte, secondPlaneCompleteBytes)
					.Fill(completeByteValue);
			}

			void SetLeftPixels(int planeOffset, int offset, int count)
			{
				if (count == 0)
					return;

				byte mask = unchecked((byte)(0b11111111 >> (8 - count * 2)));

				int address = planeOffset + offset - 1;

				Array.VRAM[address] = unchecked((byte)(
					(Array.VRAM[address] & ~mask) |
					(completeByteValue & mask)));
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			void SetRightPixels(int planeOffset, int offset, int count)
			{
				if (count == 0)
					return;

				byte mask = unchecked((byte)(0b11111111 << (8 - count * 2)));

				int address = planeOffset + offset + 1;

				Array.VRAM[address] = unchecked((byte)(
					(Array.VRAM[address] & ~mask) |
					(completeByteValue & mask)));
			}

			SetLeftPixels(firstPlaneOffset, firstPlaneFirstCompleteByte, firstPlaneLeftPixels);
			SetRightPixels(firstPlaneOffset, firstPlaneLastCompleteByte, firstPlaneRightPixels);

			SetLeftPixels(secondPlaneOffset, secondPlaneFirstCompleteByte, secondPlaneLeftPixels);
			SetRightPixels(secondPlaneOffset, secondPlaneLastCompleteByte, secondPlaneRightPixels);
		}
	}
}
