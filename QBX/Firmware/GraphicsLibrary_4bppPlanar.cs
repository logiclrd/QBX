using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using QBX.Hardware;

namespace QBX.Firmware;

public class GraphicsLibrary_4bppPlanar : GraphicsLibrary
{
	public GraphicsLibrary_4bppPlanar(Machine machine)
		: base(machine)
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

	public override int PixelsPerByte => 8;
	public override int MaximumAttribute => 15;

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

	protected override void ClearGraphicsImplementation(int windowStart, int windowEnd)
	{
		var vramSpan = Array.VRAM.AsSpan();

		int windowOffset = windowStart * _stride;
		int windowLength = (windowEnd - windowStart + 1) * _stride;

		vramSpan.Slice(_plane0Offset + windowOffset, windowLength).Clear();
		vramSpan.Slice(_plane1Offset + windowOffset, windowLength).Clear();
		vramSpan.Slice(_plane2Offset + windowOffset, windowLength).Clear();
		vramSpan.Slice(_plane3Offset + windowOffset, windowLength).Clear();
	}

	public override int PixelGet(int x, int y)
	{
		if ((x >= 0) && (x < Width)
		 && (y >= 0) && (y < Height))
		{
			int offset = y * _stride + (x >> 3);
			int shift = 7 - (x & 7);
			int bitMask = 1 << shift;

			var bits0 = (Array.VRAM[_plane0Offset + offset] >> shift) & 1;
			var bits1 = (Array.VRAM[_plane1Offset + offset] >> shift) & 1;
			var bits2 = (Array.VRAM[_plane2Offset + offset] >> shift) & 1;
			var bits3 = (Array.VRAM[_plane3Offset + offset] >> shift) & 1;

			return bits0 | (bits1 << 1) | (bits2 << 2) | (bits3 << 3);
		}

		return 0;
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

	public override void GetSprite(int x1, int y1, int x2, int y2, Span<byte> buffer)
	{
		int x = x1, y = y1;
		int w = x2 - x1 + 1, h = y2 - y1 + 1;

		if ((x < 0) || (y < 0) || (w < 0) || (h < 0))
			throw new InvalidOperationException();
		if ((x + w >= Width) || (y + h >= Height))
			throw new InvalidOperationException();

		var vramSpan = Array.VRAM.AsSpan();

		var plane0 = vramSpan.Slice(_plane0Offset, _planeBytesUsed);
		var plane1 = vramSpan.Slice(_plane1Offset, _planeBytesUsed);
		var plane2 = vramSpan.Slice(_plane2Offset, _planeBytesUsed);
		var plane3 = vramSpan.Slice(_plane3Offset, _planeBytesUsed);

		int bytesPerScan = (w + 7) / 8;

		int headerBytes = 4;
		int dataBytes = bytesPerScan * h * 4;

		int totalBytes = headerBytes + dataBytes;

		if (buffer.Length < totalBytes)
			throw new InvalidOperationException();

		var header = MemoryMarshal.Cast<byte, short>(buffer.Slice(0, headerBytes));

		header[0] = (short)w;
		header[1] = (short)h;

		var data = buffer.Slice(headerBytes, dataBytes);

		// 76543210
		// ^-------  x = 0
		//    ^---- ---   x = 3

		int leftPixelShift = (8 - (x & 7)) & 7;
		int rightPixelShift = x & 7;
		int leftPixelMask = unchecked((byte)(255 << leftPixelShift));
		int rightPixelMask = unchecked((byte)(255 >> rightPixelShift));

		if (leftPixelShift == 0)
			rightPixelMask = 0;

		int bitsForNextPixel0 = 0;
		int bitsForNextPixel1 = 0;
		int bitsForNextPixel2 = 0;
		int bitsForNextPixel3 = 0;

		int lastByteMask = unchecked((byte)(255 << (w & 7)));

		for (int yy = 0; yy < h; yy++)
		{
			int o = (y + yy) * _stride + (x >> 3);
			int p = yy * bytesPerScan * 4;

			int p0 = p + 0 * bytesPerScan;
			int p1 = p + 1 * bytesPerScan;
			int p2 = p + 2 * bytesPerScan;
			int p3 = p + 3 * bytesPerScan;

			for (int xx = 0; xx < w; xx += 8, o++, p0++, p1++, p2++, p3++)
			{
				int sample0 = plane0[o];
				int sample1 = plane1[o];
				int sample2 = plane2[o];
				int sample3 = plane3[o];

				data[p0] = unchecked((byte)(
					bitsForNextPixel0 | ((sample0 & leftPixelMask) >> leftPixelShift)));
				data[p1] = unchecked((byte)(
					bitsForNextPixel1 | ((sample1 & leftPixelMask) >> leftPixelShift)));
				data[p2] = unchecked((byte)(
					bitsForNextPixel2 | ((sample2 & leftPixelMask) >> leftPixelShift)));
				data[p3] = unchecked((byte)(
					bitsForNextPixel3 | ((sample3 & leftPixelMask) >> leftPixelShift)));

				bitsForNextPixel0 = (sample0 & rightPixelMask) << rightPixelShift;
				bitsForNextPixel1 = (sample1 & rightPixelMask) << rightPixelShift;
				bitsForNextPixel2 = (sample2 & rightPixelMask) << rightPixelShift;
				bitsForNextPixel3 = (sample3 & rightPixelMask) << rightPixelShift;
			}

			data[p0 - 1] = unchecked((byte)(data[p0 - 1] & lastByteMask));
			data[p1 - 1] = unchecked((byte)(data[p1 - 1] & lastByteMask));
			data[p2 - 1] = unchecked((byte)(data[p2 - 1] & lastByteMask));
			data[p3 - 1] = unchecked((byte)(data[p3 - 1] & lastByteMask));
		}
	}

	public override void PutSprite(Span<byte> buffer, PutSpriteAction action, int x, int y)
	{
		switch (action)
		{
			case PutSpriteAction.PixelSet: PutSprite<SpriteOperation_PixelSet>(buffer, x, y); break;
			case PutSpriteAction.PixelSetInverted: PutSprite<SpriteOperation_PixelSetInverted>(buffer, x, y); break;
			case PutSpriteAction.And: PutSprite<SpriteOperation_And>(buffer, x, y); break;
			case PutSpriteAction.Or: PutSprite<SpriteOperation_Or>(buffer, x, y); break;
			case PutSpriteAction.ExclusiveOr: PutSprite<SpriteOperation_ExclusiveOr>(buffer, x, y); break;
		}
	}

	void PutSprite<TAction>(Span<byte> buffer, int x, int y)
		where TAction : ISpriteOperation, new()
	{
		if (buffer.Length < 4)
			throw new InvalidOperationException();

		int headerBytes = 4;

		var header = MemoryMarshal.Cast<byte, short>(buffer.Slice(0, headerBytes));

		int w = header[0];
		int h = header[1];

		if ((x < 0) || (y < 0) || (w < 0) || (h < 0))
			throw new InvalidOperationException();
		if ((x + w >= Width) || (y + h >= Height))
			throw new InvalidOperationException();

		var vramSpan = Array.VRAM.AsSpan();

		var plane0 = vramSpan.Slice(_plane0Offset, _planeBytesUsed);
		var plane1 = vramSpan.Slice(_plane1Offset, _planeBytesUsed);
		var plane2 = vramSpan.Slice(_plane2Offset, _planeBytesUsed);
		var plane3 = vramSpan.Slice(_plane3Offset, _planeBytesUsed);

		int bytesPerScan = (w + 7) / 8;

		int dataBytes = bytesPerScan * h * 4;

		int totalBytes = headerBytes + dataBytes;

		if (buffer.Length < totalBytes)
			throw new InvalidOperationException();

		var data = buffer.Slice(headerBytes, dataBytes);

		// 76543210
		// ^-------  x = 0
		//    ^---- ---   x = 3

		int leftPixelShift = x & 7;
		int rightPixelShift = (8 - (x & 7)) & 7;
		int leftPixelMask = unchecked((byte)(255 << leftPixelShift));
		int rightPixelMask = unchecked((byte)(255 >> rightPixelShift));

		if (leftPixelShift == 0)
			rightPixelMask = 0;

		int lastOutputBytePixels = (x + w) & 7;

		int lastByteMask = unchecked((byte)~(255 >> lastOutputBytePixels));

		int startXX = 7 - ((x - 1) & 7);

		var action = new TAction();

		for (int yy = 0; yy < h; yy++)
		{
			int o = (y + yy) * _stride + (x >> 3);
			int p = 4 * yy * bytesPerScan;

			int p0 = p + 0 * bytesPerScan;
			int p1 = p + 1 * bytesPerScan;
			int p2 = p + 2 * bytesPerScan;
			int p3 = p + 3 * bytesPerScan;

			int spriteMask = leftPixelMask >> leftPixelShift;
			int unrelatedMask = unchecked((byte)~spriteMask);

			int bitsForNextPixel0 = 0;
			int bitsForNextPixel1 = 0;
			int bitsForNextPixel2 = 0;
			int bitsForNextPixel3 = 0;

			for (int xx = startXX; xx < w; xx += 8, o++, p0++, p1++, p2++, p3++)
			{
				{
					byte planeByte = (unrelatedMask == 0) ? (byte)0 : plane0[o];
					int sample = data[p0];
					int spriteByte = bitsForNextPixel0 | ((sample & leftPixelMask) >> leftPixelShift);

					plane0[o] = action.ApplySpriteBits(planeByte, spriteByte, unrelatedMask, spriteMask);

					bitsForNextPixel0 = (sample & rightPixelMask) << rightPixelShift;
				}

				{
					byte planeByte = (unrelatedMask == 0) ? (byte)0 : plane1[o];
					int sample = data[p1];
					int spriteByte = bitsForNextPixel1 | ((sample & leftPixelMask) >> leftPixelShift);

					plane1[o] = action.ApplySpriteBits(planeByte, spriteByte, unrelatedMask, spriteMask);

					bitsForNextPixel1 = (sample & rightPixelMask) << rightPixelShift;
				}

				{
					byte planeByte = (unrelatedMask == 0) ? (byte)0 : plane2[o];
					int sample = data[p2];
					int spriteByte = bitsForNextPixel2 | ((sample & leftPixelMask) >> leftPixelShift);

					plane2[o] = action.ApplySpriteBits(planeByte, spriteByte, unrelatedMask, spriteMask);

					bitsForNextPixel2 = (sample & rightPixelMask) << rightPixelShift;
				}

				{
					byte planeByte = (unrelatedMask == 0) ? (byte)0 : plane3[o];
					int sample = data[p3];
					int spriteByte = bitsForNextPixel3 | ((sample & leftPixelMask) >> leftPixelShift);

					plane3[o] = action.ApplySpriteBits(planeByte, spriteByte, unrelatedMask, spriteMask);

					bitsForNextPixel3 = (sample & rightPixelMask) << rightPixelShift;
				}

				unrelatedMask = 0;
				spriteMask = 255;
			}

			if (lastByteMask != 0)
			{
				unrelatedMask = unchecked((byte)~lastByteMask);
				spriteMask = lastByteMask;

				{
					byte planeByte = plane0[o];

					bitsForNextPixel0 |= data[p0 - 1] << (8 - lastOutputBytePixels);

					plane0[o] = action.ApplySpriteBits(planeByte, bitsForNextPixel0, unrelatedMask, spriteMask);
				}

				{
					byte planeByte = plane1[o];

					bitsForNextPixel1 |= data[p1 - 1] << (8 - lastOutputBytePixels);

					plane1[o] = action.ApplySpriteBits(planeByte, bitsForNextPixel1, unrelatedMask, spriteMask);
				}

				{
					byte planeByte = plane2[o];

					bitsForNextPixel2 |= data[p2 - 1] << (8 - lastOutputBytePixels);

					plane2[o] = action.ApplySpriteBits(planeByte, bitsForNextPixel2, unrelatedMask, spriteMask);
				}

				{
					byte planeByte = plane3[o];

					bitsForNextPixel3 |= data[p3 - 1] << (8 - lastOutputBytePixels);

					plane3[o] = action.ApplySpriteBits(planeByte, bitsForNextPixel3, unrelatedMask, spriteMask);
				}
			}
		}
	}

	public override void ScrollUp(int scanCount, int windowStart, int windowEnd)
	{
		var vramSpan = Array.VRAM.AsSpan();

		int copyOffset = scanCount * _stride;

		int windowOffset = windowStart * _stride;
		int windowLength = (windowEnd - windowStart + 1) * _stride;

		{
			var plane = vramSpan.Slice(_plane0Offset, _planeBytesUsed);

			plane = plane.Slice(windowOffset, windowLength);

			plane.Slice(copyOffset).CopyTo(plane);
			plane.Slice(plane.Length - copyOffset).Fill(0);
		}

		{
			var plane = vramSpan.Slice(_plane1Offset, _planeBytesUsed);

			plane = plane.Slice(windowOffset, windowLength);

			plane.Slice(copyOffset).CopyTo(plane);
			plane.Slice(plane.Length - copyOffset).Fill(0);
		}

		{
			var plane = vramSpan.Slice(_plane2Offset, _planeBytesUsed);

			plane = plane.Slice(windowOffset, windowLength);

			plane.Slice(copyOffset).CopyTo(plane);
			plane.Slice(plane.Length - copyOffset).Fill(0);
		}

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
			int o = y * _stride + (x >> 3);

			if ((o >= 0) && (o < _planeBytesUsed))
			{
				var vramSpan = Array.VRAM.AsSpan();

				int planeMask = Array.Graphics.Registers.BitMask;

				vramSpan.Slice(_plane0Offset, _planeBytesUsed)[o] = ((DrawingAttribute & 1) != 0) ? glyphScan : (byte)0;
				vramSpan.Slice(_plane1Offset, _planeBytesUsed)[o] = ((DrawingAttribute & 2) != 0) ? glyphScan : (byte)0;
				vramSpan.Slice(_plane2Offset, _planeBytesUsed)[o] = ((DrawingAttribute & 4) != 0) ? glyphScan : (byte)0;
				vramSpan.Slice(_plane3Offset, _planeBytesUsed)[o] = ((DrawingAttribute & 8) != 0) ? glyphScan : (byte)0;
			}
		}
	}
}
