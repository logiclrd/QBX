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
		using (HidePointerForOperationIfPointerAware())
		{
			var vramSpan = Array.VRAM.AsSpan();

			int windowOffset = windowStart * _stride;
			int windowLength = (windowEnd - windowStart + 1) * _stride;

			vramSpan.Slice(_plane0Offset + windowOffset, windowLength).Clear();
			vramSpan.Slice(_plane1Offset + windowOffset, windowLength).Clear();
			vramSpan.Slice(_plane2Offset + windowOffset, windowLength).Clear();
			vramSpan.Slice(_plane3Offset + windowOffset, windowLength).Clear();
		}
	}

	public override int PixelGet(int x, int y)
	{
		using (HidePointerForOperationIfPointerAware(x, y))
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
	}

	public override void PixelSet(int x, int y, int attribute)
	{
		using (HidePointerForOperationIfPointerAware(x, y))
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

		using (HidePointerForOperationIfPointerAware(x1, y, x2, y))
		{
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

	protected override byte[]? ConstructSpriteImplementation(int[] pixels, int width, int height, Span<byte> buffer)
	{
		if (pixels.Length != width * height)
			throw new ArgumentException(nameof(pixels), "Pixel data is not the expected length");

		// In 4bpp planar modes, GET/PUT buffers are a bit weird. They actually internally represent each plane
		// as a separate scan, so there are 4 scans per actual scan in the sprite. But, the header doesn't count
		// them independently. To avoid complicating the sprite-generating code, we simply transform the input
		// data so that the planes _are_ represented separately, construct a sprite from that, whose Height field
		// in the header will be counting the per-plane scans independently, and then fix up the header.

		// Separate the planes of data.
		int[] planarPixels = new int[pixels.Length * 4];

		for (int y = 0, o = 0, p = 0; y < height; y++, o += width)
			for (int bit = 0; bit < 4; bit++)
				for (int x = 0; x < width; x++, p++)
					planarPixels[p] = (pixels[o + x] >> bit) & 1;

		var sprite = base.ConstructSpriteImplementation(planarPixels, width, height * 4, buffer);

		var header = MemoryMarshal.Cast<byte, short>(sprite.AsSpan().Slice(0, 4));

		// We've created a sprite that represents each of the planes of each scan as a
		// separate scan. Its height is treating those per-plane scans as independent
		// scans, when really each group of 4 represents a single _actual_ scan.
		header[1] /= 4;

		return sprite;
	}

	public override void GetSprite(int x1, int y1, int x2, int y2, Span<byte> buffer)
	{
		int x = x1, y = y1;
		int w = x2 - x1 + 1, h = y2 - y1 + 1;

		if ((x < 0) || (y < 0) || (w < 0) || (h < 0))
			throw new InvalidOperationException();
		if ((x + w > Width) || (y + h > Height))
			throw new InvalidOperationException();

		using (HidePointerForOperationIfPointerAware(x1, y1, x2, y2))
		{
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

			int lastByteMask = unchecked((byte)(0x7F80 >> ((w - 1) & 7)));

			for (int yy = 0; yy < h; yy++)
			{
				int o = (y + yy) * _stride + (x >> 3);
				int p = yy * bytesPerScan * 4;

				int p0 = p + 0 * bytesPerScan;
				int p1 = p + 1 * bytesPerScan;
				int p2 = p + 2 * bytesPerScan;
				int p3 = p + 3 * bytesPerScan;

				if (rightPixelMask != 0)
				{
					// Input and output bytes are not aligned, and the offset o has only
					// some of the bits for the first output byte in each plane.
					int sample0 = plane0[o];
					int sample1 = plane1[o];
					int sample2 = plane2[o];
					int sample3 = plane3[o];

					bitsForNextPixel0 = (sample0 & rightPixelMask) << rightPixelShift;
					bitsForNextPixel1 = (sample1 & rightPixelMask) << rightPixelShift;
					bitsForNextPixel2 = (sample2 & rightPixelMask) << rightPixelShift;
					bitsForNextPixel3 = (sample3 & rightPixelMask) << rightPixelShift;

					o++;
				}

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
		if ((x + w > Width) || (y + h > Height))
			throw new InvalidOperationException();

		using (HidePointerForOperationIfPointerAware(x, y, x + w - 1, y + h - 1))
		{
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

			int lastDataBytePixels = ((w - 1) & 7) + 1;
			int lastOutputBytePixels = (x + w) & 7;

			int lastByteMask = unchecked((byte)~(255 >> lastOutputBytePixels));

			int subByteAlignment = 0;

			int loopBytes = ((x & 7) + w) >> 3;

			if ((x & 7) + w < 8)
			{
				// Start and end in the same output byte.
				// We'll skip the main loop and just use the tail,
				// but that means we're also skipping incrementing
				// p, which the tail depends on. So, factor that in.
				subByteAlignment = x & 7;
				lastByteMask = unchecked((byte)(((255 << (8 - w)) & 255) >> subByteAlignment));
				lastOutputBytePixels = lastDataBytePixels;
			}

			var action = new TAction();

			bool needToReadPlaneBits = action.UsesPlaneBits;

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

				bool readPlaneBits = (unrelatedMask != 0) || needToReadPlaneBits;

				for (int xx = 0; xx < loopBytes; xx++, o++, p0++, p1++, p2++, p3++)
				{
					{
						byte planeByte = readPlaneBits ? plane0[o] : (byte)0;
						int sample = data[p0];
						int spriteByte = bitsForNextPixel0 | ((sample & leftPixelMask) >> leftPixelShift);

						plane0[o] = action.ApplySpriteBits(planeByte, spriteByte, unrelatedMask, spriteMask);

						bitsForNextPixel0 = (sample & rightPixelMask) << rightPixelShift;
					}

					{
						byte planeByte = readPlaneBits ? plane1[o] : (byte)0;
						int sample = data[p1];
						int spriteByte = bitsForNextPixel1 | ((sample & leftPixelMask) >> leftPixelShift);

						plane1[o] = action.ApplySpriteBits(planeByte, spriteByte, unrelatedMask, spriteMask);

						bitsForNextPixel1 = (sample & rightPixelMask) << rightPixelShift;
					}

					{
						byte planeByte = readPlaneBits ? plane2[o] : (byte)0;
						int sample = data[p2];
						int spriteByte = bitsForNextPixel2 | ((sample & leftPixelMask) >> leftPixelShift);

						plane2[o] = action.ApplySpriteBits(planeByte, spriteByte, unrelatedMask, spriteMask);

						bitsForNextPixel2 = (sample & rightPixelMask) << rightPixelShift;
					}

					{
						byte planeByte = readPlaneBits ? plane3[o] : (byte)0;
						int sample = data[p3];
						int spriteByte = bitsForNextPixel3 | ((sample & leftPixelMask) >> leftPixelShift);

						plane3[o] = action.ApplySpriteBits(planeByte, spriteByte, unrelatedMask, spriteMask);

						bitsForNextPixel3 = (sample & rightPixelMask) << rightPixelShift;
					}

					unrelatedMask = 0;
					spriteMask = 255;
					readPlaneBits = needToReadPlaneBits;
				}

				if (lastByteMask != 0)
				{
					unrelatedMask = unchecked((byte)~lastByteMask);
					spriteMask = lastByteMask;

					if (subByteAlignment != 0)
					{
						bitsForNextPixel0 = data[p0] >> subByteAlignment;
						bitsForNextPixel1 = data[p1] >> subByteAlignment;
						bitsForNextPixel2 = data[p2] >> subByteAlignment;
						bitsForNextPixel3 = data[p3] >> subByteAlignment;
					}
					else if (lastDataBytePixels <= lastOutputBytePixels)
					{
						bitsForNextPixel0 |= data[p0] >> leftPixelShift;
						bitsForNextPixel1 |= data[p1] >> leftPixelShift;
						bitsForNextPixel2 |= data[p2] >> leftPixelShift;
						bitsForNextPixel3 |= data[p3] >> leftPixelShift;
					}

					plane0[o] = action.ApplySpriteBits(plane0[o], bitsForNextPixel0, unrelatedMask, spriteMask);
					plane1[o] = action.ApplySpriteBits(plane1[o], bitsForNextPixel1, unrelatedMask, spriteMask);
					plane2[o] = action.ApplySpriteBits(plane2[o], bitsForNextPixel2, unrelatedMask, spriteMask);
					plane3[o] = action.ApplySpriteBits(plane3[o], bitsForNextPixel3, unrelatedMask, spriteMask);
				}
			}
		}
	}

	[ThreadStatic]
	static byte[]? s_zeroes;
	[ThreadStatic]
	static byte[]? s_ones;

	public override void PutMaskedSprite(Span<byte> and, Span<byte> xor, int x, int y)
	{
		if (and.Length < 4)
			throw new InvalidOperationException();
		if (xor.Length < 4)
			throw new InvalidOperationException();

		int headerBytes = 4;

		var andHeader = MemoryMarshal.Cast<byte, short>(and.Slice(0, headerBytes));

		int andW = andHeader[0];
		int andH = andHeader[1];

		var xorHeader = MemoryMarshal.Cast<byte, short>(xor.Slice(0, headerBytes));

		int xorW = xorHeader[0];
		int xorH = xorHeader[1];

		int minW = Math.Min(andW, xorW);
		int maxW = Math.Max(andW, xorW);
		int maxH = Math.Max(andH, xorH);

		if ((andW < 0) || (andH < 0))
			throw new InvalidOperationException();
		if ((xorW < 0) || (xorH < 0))
			throw new InvalidOperationException();
		if ((x + maxW <= 0) || (y + maxH <= 0))
			return;
		if ((x >= Width) || (y >= Height))
			return;

		using (HidePointerForOperationIfPointerAware(x, y, x + maxW - 1, y + maxH - 1))
		{
			var vramSpan = Array.VRAM.AsSpan();

			var plane0 = vramSpan.Slice(_plane0Offset, _planeBytesUsed);
			var plane1 = vramSpan.Slice(_plane1Offset, _planeBytesUsed);
			var plane2 = vramSpan.Slice(_plane2Offset, _planeBytesUsed);
			var plane3 = vramSpan.Slice(_plane3Offset, _planeBytesUsed);

			int andBytesPerScan = (andW + 7) / 8;
			int xorBytesPerScan = (xorW + 7) / 8;

			int andDataBytes = andBytesPerScan * andH * 4;
			int xorDataBytes = xorBytesPerScan * xorH * 4;

			int totalAndBytes = headerBytes + andDataBytes;
			int totalXorBytes = headerBytes + xorDataBytes;

			if (and.Length < totalAndBytes)
				throw new InvalidOperationException();
			if (xor.Length < totalXorBytes)
				throw new InvalidOperationException();

			var andData = and.Slice(headerBytes, andDataBytes);
			var xorData = xor.Slice(headerBytes, xorDataBytes);

			if (y < 0)
			{
				andData = andData.Slice(-y * andBytesPerScan);
				xorData = xorData.Slice(-y * xorBytesPerScan);

				xorH += y;
				y = 0;
			}

			if (y + xorH >= Height)
				xorH = Height - y;

			// 76543210
			// ^-------  x = 0
			//    ^---- ---   x = 3

			int leftPixelShift = x & 7;
			int rightPixelShift = (8 - (x & 7)) & 7;
			int leftPixelMask = unchecked((byte)(255 << leftPixelShift));
			int rightPixelMask = unchecked((byte)(255 >> rightPixelShift));

			if (leftPixelShift == 0)
				rightPixelMask = 0;

			int lastAndBytePixels = ((andW - 1) & 7) + 1;
			int lastXorBytePixels = ((xorW - 1) & 7) + 1;
			int lastOutputBytePixels = (x + xorW) & 7;

			int lastByteMask = unchecked((byte)~(255 >> lastOutputBytePixels));

			int subByteAlignment = 0;

			int loopW = Math.Max(andW, xorW);

			int loopBytes = ((x & 7) + loopW) >> 3;

			if ((x & 7) + loopW < 8)
			{
				// Start and end in the same output byte.
				// We'll skip the main loop and just use the tail,
				// but that means we're also skipping incrementing
				// p, which the tail depends on. So, factor that in.
				subByteAlignment = x & 7;
				lastByteMask = unchecked((byte)(((255 << (8 - loopW)) & 255) >> subByteAlignment));
				lastOutputBytePixels = lastAndBytePixels | lastXorBytePixels;
			}

			if (andBytesPerScan > xorBytesPerScan)
			{
				if ((s_zeroes == null) || (s_zeroes.Length < andBytesPerScan))
					s_zeroes = new byte[andBytesPerScan * 2];
			}

			if (xorBytesPerScan > andBytesPerScan)
			{
				if ((s_ones == null) || (s_ones.Length < xorBytesPerScan))
				{
					s_ones = new byte[xorBytesPerScan * 2];

					s_ones.AsSpan().Fill(0xFF);
				}
			}

			Span<byte> zeroesScan = s_zeroes;
			Span<byte> onesScan = s_ones;

			var andAction = new SpriteOperation_And();
			var xorAction = new SpriteOperation_ExclusiveOr();

			for (int yy = 0; yy < xorH; yy++)
			{
				int o = (y + yy) * _stride + (x >> 3);
				int p = 4 * yy * andBytesPerScan;
				int q = 4 * yy * xorBytesPerScan;

				int p0 = p + 0 * andBytesPerScan;
				int p1 = p + 1 * andBytesPerScan;
				int p2 = p + 2 * andBytesPerScan;
				int p3 = p + 3 * andBytesPerScan;

				int q0 = q + 0 * xorBytesPerScan;
				int q1 = q + 1 * xorBytesPerScan;
				int q2 = q + 2 * xorBytesPerScan;
				int q3 = q + 3 * xorBytesPerScan;

				int spriteMask = leftPixelMask >> leftPixelShift;
				int unrelatedMask = unchecked((byte)~spriteMask);

				int andBitsForNextPixel0 = 0;
				int andBitsForNextPixel1 = 0;
				int andBitsForNextPixel2 = 0;
				int andBitsForNextPixel3 = 0;

				int xorBitsForNextPixel0 = 0;
				int xorBitsForNextPixel1 = 0;
				int xorBitsForNextPixel2 = 0;
				int xorBitsForNextPixel3 = 0;

				var andScan0 = andData.Slice(p0, andBytesPerScan);
				var andScan1 = andData.Slice(p1, andBytesPerScan);
				var andScan2 = andData.Slice(p2, andBytesPerScan);
				var andScan3 = andData.Slice(p3, andBytesPerScan);

				var xorScan0 = xorData.Slice(q0, xorBytesPerScan);
				var xorScan1 = xorData.Slice(q1, xorBytesPerScan);
				var xorScan2 = xorData.Slice(q2, xorBytesPerScan);
				var xorScan3 = xorData.Slice(q3, xorBytesPerScan);

				for (int xx = 0; xx < loopBytes; xx++, o++)
				{
					if (xx == andBytesPerScan)
						andScan0 = andScan1 = andScan2 = andScan3 = onesScan;
					if (xx == xorBytesPerScan)
						xorScan0 = xorScan1 = xorScan2 = xorScan3 = zeroesScan;

					int rx = xx + x;

					bool onScreen = (rx >= 0) && (rx < Width);

					{
						int andSample = andScan0[xx];
						int xorSample = xorScan0[xx];
						int andByte = andBitsForNextPixel0 | ((andSample & leftPixelMask) >> leftPixelShift);
						int xorByte = xorBitsForNextPixel0 | ((xorSample & leftPixelMask) >> leftPixelShift);

						if (onScreen)
						{
							var planeByte = plane0[o];

							planeByte = andAction.ApplySpriteBits(planeByte, andByte, unrelatedMask, spriteMask);
							planeByte = xorAction.ApplySpriteBits(planeByte, xorByte, unrelatedMask, spriteMask);

							plane0[o] = planeByte;
						}

						andBitsForNextPixel0 = (andSample & rightPixelMask) << rightPixelShift;
						xorBitsForNextPixel0 = (xorSample & rightPixelMask) << rightPixelShift;
					}

					{
						int andSample = andScan1[xx];
						int xorSample = xorScan1[xx];
						int andByte = andBitsForNextPixel1 | ((andSample & leftPixelMask) >> leftPixelShift);
						int xorByte = xorBitsForNextPixel1 | ((xorSample & leftPixelMask) >> leftPixelShift);

						if (onScreen)
						{
							var planeByte = plane1[o];

							planeByte = andAction.ApplySpriteBits(planeByte, andByte, unrelatedMask, spriteMask);
							planeByte = xorAction.ApplySpriteBits(planeByte, xorByte, unrelatedMask, spriteMask);

							plane1[o] = planeByte;
						}

						andBitsForNextPixel1 = (andSample & rightPixelMask) << rightPixelShift;
						xorBitsForNextPixel1 = (xorSample & rightPixelMask) << rightPixelShift;
					}

					{
						int andSample = andScan2[xx];
						int xorSample = xorScan2[xx];
						int andByte = andBitsForNextPixel2 | ((andSample & leftPixelMask) >> leftPixelShift);
						int xorByte = xorBitsForNextPixel2 | ((xorSample & leftPixelMask) >> leftPixelShift);

						if (onScreen)
						{
							var planeByte = plane2[o];

							planeByte = andAction.ApplySpriteBits(planeByte, andByte, unrelatedMask, spriteMask);
							planeByte = xorAction.ApplySpriteBits(planeByte, xorByte, unrelatedMask, spriteMask);

							plane2[o] = planeByte;
						}

						andBitsForNextPixel2 = (andSample & rightPixelMask) << rightPixelShift;
						xorBitsForNextPixel2 = (xorSample & rightPixelMask) << rightPixelShift;
					}

					{
						int andSample = andScan3[xx];
						int xorSample = xorScan3[xx];
						int andByte = andBitsForNextPixel3 | ((andSample & leftPixelMask) >> leftPixelShift);
						int xorByte = xorBitsForNextPixel3 | ((xorSample & leftPixelMask) >> leftPixelShift);

						if (onScreen)
						{
							var planeByte = plane3[o];

							planeByte = andAction.ApplySpriteBits(planeByte, andByte, unrelatedMask, spriteMask);
							planeByte = xorAction.ApplySpriteBits(planeByte, xorByte, unrelatedMask, spriteMask);

							plane3[o] = planeByte;
						}

						andBitsForNextPixel3 = (andSample & rightPixelMask) << rightPixelShift;
						xorBitsForNextPixel3 = (xorSample & rightPixelMask) << rightPixelShift;
					}

					unrelatedMask = 0;
					spriteMask = 255;
				}

				if ((lastByteMask != 0) && (x + minW <= Width))
				{
					if (loopBytes == andBytesPerScan)
						andScan0 = andScan1 = andScan2 = andScan3 = onesScan;
					if (loopBytes == xorBytesPerScan)
						xorScan0 = xorScan1 = xorScan2 = xorScan3 = zeroesScan;

					unrelatedMask = unchecked((byte)~lastByteMask);
					spriteMask = lastByteMask;

					if (subByteAlignment != 0)
					{
						andBitsForNextPixel0 = (andScan0[loopBytes] & lastAndBytePixels) >> subByteAlignment;
						andBitsForNextPixel1 = (andScan1[loopBytes] & lastAndBytePixels) >> subByteAlignment;
						andBitsForNextPixel2 = (andScan2[loopBytes] & lastAndBytePixels) >> subByteAlignment;
						andBitsForNextPixel3 = (andScan3[loopBytes] & lastAndBytePixels) >> subByteAlignment;

						xorBitsForNextPixel0 = (xorScan0[loopBytes] & lastXorBytePixels) >> subByteAlignment;
						xorBitsForNextPixel1 = (xorScan1[loopBytes] & lastXorBytePixels) >> subByteAlignment;
						xorBitsForNextPixel2 = (xorScan2[loopBytes] & lastXorBytePixels) >> subByteAlignment;
						xorBitsForNextPixel3 = (xorScan3[loopBytes] & lastXorBytePixels) >> subByteAlignment;
					}
					else
					{
						if (lastAndBytePixels <= lastOutputBytePixels)
						{
							andBitsForNextPixel0 |= andScan0[loopBytes] >> leftPixelShift;
							andBitsForNextPixel1 |= andScan1[loopBytes] >> leftPixelShift;
							andBitsForNextPixel2 |= andScan2[loopBytes] >> leftPixelShift;
							andBitsForNextPixel3 |= andScan3[loopBytes] >> leftPixelShift;
						}

						if (lastXorBytePixels <= lastOutputBytePixels)
						{
							xorBitsForNextPixel0 |= xorScan0[loopBytes] >> leftPixelShift;
							xorBitsForNextPixel1 |= xorScan1[loopBytes] >> leftPixelShift;
							xorBitsForNextPixel2 |= xorScan2[loopBytes] >> leftPixelShift;
							xorBitsForNextPixel3 |= xorScan3[loopBytes] >> leftPixelShift;
						}
					}

					{
						byte planeByte = plane0[o];

						planeByte = andAction.ApplySpriteBits(planeByte, andBitsForNextPixel0, unrelatedMask, spriteMask);
						planeByte = xorAction.ApplySpriteBits(planeByte, xorBitsForNextPixel0, unrelatedMask, spriteMask);

						plane0[o] = planeByte;
					}

					{
						byte planeByte = plane1[o];

						planeByte = andAction.ApplySpriteBits(planeByte, andBitsForNextPixel1, unrelatedMask, spriteMask);
						planeByte = xorAction.ApplySpriteBits(planeByte, xorBitsForNextPixel1, unrelatedMask, spriteMask);

						plane1[o] = planeByte;
					}

					{
						byte planeByte = plane2[o];

						planeByte = andAction.ApplySpriteBits(planeByte, andBitsForNextPixel2, unrelatedMask, spriteMask);
						planeByte = xorAction.ApplySpriteBits(planeByte, xorBitsForNextPixel2, unrelatedMask, spriteMask);

						plane2[o] = planeByte;
					}

					{
						byte planeByte = plane3[o];

						planeByte = andAction.ApplySpriteBits(planeByte, andBitsForNextPixel3, unrelatedMask, spriteMask);
						planeByte = xorAction.ApplySpriteBits(planeByte, xorBitsForNextPixel3, unrelatedMask, spriteMask);

						plane3[o] = planeByte;
					}
				}
			}
		}
	}

	public override void ScrollUp(int scanCount, int windowStart, int windowEnd)
	{
		var vramSpan = Array.VRAM.AsSpan();

		using (HidePointerForOperationIfPointerAware(0, windowStart, Width, windowEnd + scanCount))
		{
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
	}

	protected override void DrawCharacterScan(int x, int y, int characterWidth, byte glyphScan)
	{
		if ((x & 7) != 0)
			base.DrawCharacterScan(x, y, characterWidth, glyphScan);
		else if ((DrawingAttribute & 0x80) == 0)
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
		else
		{
			int o = y * _stride + (x >> 3);

			if ((o >= 0) && (o < _planeBytesUsed))
			{
				var vramSpan = Array.VRAM.AsSpan();

				int planeMask = Array.Graphics.Registers.BitMask;

				vramSpan.Slice(_plane0Offset, _planeBytesUsed)[o] ^= glyphScan;
				vramSpan.Slice(_plane1Offset, _planeBytesUsed)[o] ^= glyphScan;
				vramSpan.Slice(_plane2Offset, _planeBytesUsed)[o] ^= glyphScan;
				vramSpan.Slice(_plane3Offset, _planeBytesUsed)[o] ^= glyphScan;
			}
		}
	}
}
