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

	public override void PutMaskedSprite(Span<byte> buffer, Span<byte> mask, int x, int y)
	{
		if (buffer.Length < 4)
			throw new InvalidOperationException();
		if (mask.Length < 4)
			throw new InvalidOperationException();

		int headerBytes = 4;

		var header = MemoryMarshal.Cast<byte, short>(buffer.Slice(0, headerBytes));

		int w = header[0];
		int h = header[1];

		var maskHeader = MemoryMarshal.Cast<byte, short>(mask.Slice(0, headerBytes));

		int maskW = maskHeader[0];
		int maskH = maskHeader[1];

		if ((maskW < w) || (maskH < h))
			throw new InvalidOperationException("Mask is not large enough for the sprite");

		if ((w < 0) || (h < 0))
			throw new InvalidOperationException();
		if ((x + w <= 0) || (y + h <= 0))
			return;
		if ((x >= Width) || (y >= Height))
			return;

		using (HidePointerForOperationIfPointerAware(x, y, x + w - 1, y + h - 1))
		{
			var vramSpan = Array.VRAM.AsSpan();

			var plane0 = vramSpan.Slice(_plane0Offset, _planeBytesUsed);
			var plane1 = vramSpan.Slice(_plane1Offset, _planeBytesUsed);
			var plane2 = vramSpan.Slice(_plane2Offset, _planeBytesUsed);
			var plane3 = vramSpan.Slice(_plane3Offset, _planeBytesUsed);

			int bytesPerScan = (w + 7) / 8;
			int maskBytesPerScan = (maskW + 7) / 8;

			int dataBytes = bytesPerScan * h * 4;
			int maskDataBytes = maskBytesPerScan * h * 4; // If maskH > h, use h anyway. We don't need more than that.

			int totalBytes = headerBytes + dataBytes;
			int totalMaskBytes = headerBytes + maskDataBytes;

			if (buffer.Length < totalBytes)
				throw new InvalidOperationException();
			if (mask.Length < totalMaskBytes)
				throw new InvalidOperationException();

			var data = buffer.Slice(headerBytes, dataBytes);
			var maskData = mask.Slice(headerBytes, maskDataBytes);

			if (y < 0)
			{
				data = data.Slice(-y * bytesPerScan);
				maskData = maskData.Slice(-y * maskBytesPerScan);

				h += y;
				y = 0;
			}

			if (y + h >= Height)
				h = Height - y;

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

			if (loopBytes == 0)
			{
				// Start and end in the same output byte.
				// We'll skip the main loop and just use the tail,
				// but that means we're also skipping incrementing
				// p, which the tail depends on. So, factor that in.
				subByteAlignment = x & 7;
				lastByteMask = unchecked((byte)(((255 << (8 - w)) & 255) >> subByteAlignment));
				lastOutputBytePixels = lastDataBytePixels;
			}

			var action = new SpriteOperation_PixelSet();

			for (int yy = 0; yy < h; yy++)
			{
				int o = (y + yy) * _stride + (x >> 3);
				int p = 4 * yy * bytesPerScan;
				int q = 4 * yy * maskBytesPerScan;

				int p0 = p + 0 * bytesPerScan;
				int p1 = p + 1 * bytesPerScan;
				int p2 = p + 2 * bytesPerScan;
				int p3 = p + 3 * bytesPerScan;

				int q0 = q + 0 * maskBytesPerScan;
				int q1 = q + 1 * maskBytesPerScan;
				int q2 = q + 2 * maskBytesPerScan;
				int q3 = q + 3 * maskBytesPerScan;

				int spriteMask = leftPixelMask >> leftPixelShift;
				int unrelatedMask = unchecked((byte)~spriteMask);

				int bitsForNextPixel0 = 0;
				int bitsForNextPixel1 = 0;
				int bitsForNextPixel2 = 0;
				int bitsForNextPixel3 = 0;

				int maskBitsForNextPixel0 = 0;
				int maskBitsForNextPixel1 = 0;
				int maskBitsForNextPixel2 = 0;
				int maskBitsForNextPixel3 = 0;

				for (int xx = 0; xx < loopBytes; xx++, o++, p0++, p1++, p2++, p3++, q0++, q1++, q2++, q3++)
				{
					int rx = xx + x;

					bool onScreen = (rx >= 0) && (rx < Width);

					{
						int sample = data[p0];
						int maskSample = maskData[q0];
						int spriteByte = bitsForNextPixel0 | ((sample & leftPixelMask) >> leftPixelShift);
						int spriteMaskByte = maskBitsForNextPixel0 | ((maskSample & leftPixelMask) >> leftPixelShift);

						if (onScreen)
							plane0[o] = action.ApplySpriteBits(plane0[o], spriteByte, unrelatedMask | ~spriteMaskByte, spriteMask & spriteMaskByte);

						bitsForNextPixel0 = (sample & rightPixelMask) << rightPixelShift;
						maskBitsForNextPixel0 = (maskSample & rightPixelMask) << rightPixelShift;
					}

					{
						int sample = data[p1];
						int maskSample = maskData[q1];
						int spriteByte = bitsForNextPixel1 | ((sample & leftPixelMask) >> leftPixelShift);
						int spriteMaskByte = maskBitsForNextPixel1 | ((maskSample & leftPixelMask) >> leftPixelShift);

						if (onScreen)
							plane1[o] = action.ApplySpriteBits(plane1[o], spriteByte, unrelatedMask | ~spriteMaskByte, spriteMask & spriteMaskByte);

						bitsForNextPixel1 = (sample & rightPixelMask) << rightPixelShift;
						maskBitsForNextPixel1 = (maskSample & rightPixelMask) << rightPixelShift;
					}

					{
						int sample = data[p2];
						int maskSample = maskData[q2];
						int spriteByte = bitsForNextPixel2 | ((sample & leftPixelMask) >> leftPixelShift);
						int spriteMaskByte = maskBitsForNextPixel2 | ((maskSample & leftPixelMask) >> leftPixelShift);

						if (onScreen)
							plane2[o] = action.ApplySpriteBits(plane2[o], spriteByte, unrelatedMask | ~spriteMaskByte, spriteMask & spriteMaskByte);

						bitsForNextPixel2 = (sample & rightPixelMask) << rightPixelShift;
						maskBitsForNextPixel2 = (maskSample & rightPixelMask) << rightPixelShift;
					}

					{
						int sample = data[p3];
						int maskSample = maskData[q3];
						int spriteByte = bitsForNextPixel3 | ((sample & leftPixelMask) >> leftPixelShift);
						int spriteMaskByte = maskBitsForNextPixel3 | ((maskSample & leftPixelMask) >> leftPixelShift);

						if (onScreen)
							plane3[o] = action.ApplySpriteBits(plane3[o], spriteByte, unrelatedMask | ~spriteMaskByte, spriteMask & spriteMaskByte);

						bitsForNextPixel3 = (sample & rightPixelMask) << rightPixelShift;
						maskBitsForNextPixel3 = (maskSample & rightPixelMask) << rightPixelShift;
					}

					unrelatedMask = 0;
					spriteMask = 255;
				}

				if ((lastByteMask != 0) && (x + w <= Width))
				{
					unrelatedMask = unchecked((byte)~lastByteMask);
					spriteMask = lastByteMask;

					if (subByteAlignment != 0)
					{
						bitsForNextPixel0 = data[p0] >> subByteAlignment;
						bitsForNextPixel1 = data[p1] >> subByteAlignment;
						bitsForNextPixel2 = data[p2] >> subByteAlignment;
						bitsForNextPixel3 = data[p3] >> subByteAlignment;

						maskBitsForNextPixel0 = maskData[q0] >> subByteAlignment;
						maskBitsForNextPixel1 = maskData[q1] >> subByteAlignment;
						maskBitsForNextPixel2 = maskData[q2] >> subByteAlignment;
						maskBitsForNextPixel3 = maskData[q3] >> subByteAlignment;
					}
					else if (lastDataBytePixels <= lastOutputBytePixels)
					{
						bitsForNextPixel0 |= data[p0] >> leftPixelShift;
						bitsForNextPixel1 |= data[p1] >> leftPixelShift;
						bitsForNextPixel2 |= data[p2] >> leftPixelShift;
						bitsForNextPixel3 |= data[p3] >> leftPixelShift;

						maskBitsForNextPixel0 |= maskData[q0] >> leftPixelShift;
						maskBitsForNextPixel1 |= maskData[q1] >> leftPixelShift;
						maskBitsForNextPixel2 |= maskData[q2] >> leftPixelShift;
						maskBitsForNextPixel3 |= maskData[q3] >> leftPixelShift;
					}

					plane0[o] = action.ApplySpriteBits(plane0[o], bitsForNextPixel0, unrelatedMask | ~maskBitsForNextPixel0, spriteMask & maskBitsForNextPixel0);
					plane1[o] = action.ApplySpriteBits(plane1[o], bitsForNextPixel1, unrelatedMask | ~maskBitsForNextPixel1, spriteMask & maskBitsForNextPixel1);
					plane2[o] = action.ApplySpriteBits(plane2[o], bitsForNextPixel2, unrelatedMask | ~maskBitsForNextPixel2, spriteMask & maskBitsForNextPixel2);
					plane3[o] = action.ApplySpriteBits(plane3[o], bitsForNextPixel3, unrelatedMask | ~maskBitsForNextPixel3, spriteMask & maskBitsForNextPixel3);
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

	protected override byte[] MakePointerSprite() =>
		[
			16, 0, 16, 0, 0, 0, 0, 0, 0, 0, 0, 0, 64, 0, 64, 0, 64, 0, 64, 0, 96, 0,
			96, 0, 96, 0, 96, 0, 112, 0, 112, 0, 112, 0, 112, 0, 120, 0, 120, 0, 120,
			0, 120, 0, 124, 0, 124, 0, 124, 0, 124, 0, 126, 0, 126, 0, 126, 0, 126,
			0, 127, 0, 127, 0, 127, 0, 127, 0, 127, 128, 127, 128, 127, 128, 127,
			128, 124, 0, 124, 0, 124, 0, 124, 0, 108, 0, 108, 0, 108, 0, 108, 0, 70,
			0, 70, 0, 70, 0, 70, 0, 6, 0, 6, 0, 6, 0, 6, 0, 3, 0, 3, 0, 3, 0, 3, 0,
			3, 0, 3, 0, 3, 0, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0
		];

	protected override byte[] MakePointerMask() =>
		[
			16, 0, 16, 0, 192, 0, 192, 0, 192, 0, 192, 0, 224, 0, 224, 0, 224, 0,
			224, 0, 240, 0, 240, 0, 240, 0, 240, 0, 248, 0, 248, 0, 248, 0, 248,
			0, 252, 0, 252, 0, 252, 0, 252, 0, 254, 0, 254, 0, 254, 0, 254, 0,
			255, 0, 255, 0, 255, 0, 255, 0, 255, 128, 255, 128, 255, 128, 255,
			128, 255, 192, 255, 192, 255, 192, 255, 192, 255, 224, 255, 224, 255,
			224, 255, 224, 254, 0, 254, 0, 254, 0, 254, 0, 255, 0, 255, 0, 255,
			0, 255, 0, 207, 0, 207, 0, 207, 0, 207, 0, 7, 128, 7, 128, 7, 128, 7,
			128, 7, 128, 7, 128, 7, 128, 7, 128, 3, 0, 3, 0, 3, 0, 3, 0
		];
}
