using System;
using System.Runtime.InteropServices;

using QBX.Hardware;

namespace QBX.Firmware;

public class GraphicsLibrary_1bppInterleaved : GraphicsLibrary
{
	public GraphicsLibrary_1bppInterleaved(Machine machine)
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

	public override int PixelsPerByte => 8;
	public override int MaximumAttribute => 1;

	public override void RefreshParameters()
	{
		base.RefreshParameters();

		_plane0Offset = StartAddress;
		_plane1Offset = _plane0Offset + PlaneSize;

		_stride = Width / 8;
		_planeBytesUsed = Height * _stride / 2;
	}

	protected override void ClearGraphicsImplementation(int windowStart, int windowEnd)
	{
		if ((windowStart > Clip.Y2) || (windowEnd < Clip.Y1))
			return;

		if (windowStart < Clip.Y1)
			windowStart = Clip.Y1;
		if (windowEnd > Clip.Y2)
			windowEnd = Clip.Y2;

		int evenWindowStart = windowStart >> 1;
		int evenWindowEnd = windowEnd >> 1;
		int oddWindowStart = evenWindowStart;
		int oddWindowEnd = evenWindowEnd;

		if ((windowStart & 1) != 0)
			evenWindowStart++;

		if ((windowEnd & 1) == 0)
			oddWindowEnd--;

		using (HidePointerForOperationIfPointerAware())
		{
			if ((Clip.X1 <= 0) && (Clip.X2 >= Width - 1))
			{
				var vramSpan = Array.VRAM.AsSpan();

				int planeMask = Array.Sequencer.Registers.MapMask;

				void ClearPlane(int windowStart, int windowEnd, Span<byte> vramSpan, int planeOffset)
				{
					int windowOffset = windowStart * _stride;
					int windowLength = (windowEnd - windowStart + 1) * _stride;

					if (windowLength > 0)
						vramSpan.Slice(planeOffset + windowOffset, windowLength).Clear();
				}

				ClearPlane(
					evenWindowStart,
					evenWindowEnd,
					vramSpan,
					_plane0Offset);

				ClearPlane(
					evenWindowStart,
					evenWindowEnd,
					vramSpan,
					_plane1Offset);
			}
			else
			{
				for (int y = windowStart; y <= windowEnd; y++)
					HorizontalLinePreClipped(Clip.X1, Clip.X2, y, 0);
			}
		}
	}

	public override int PixelGet(int x, int y)
	{
		using (HidePointerForOperationIfPointerAware(x, y))
		{
			if ((x >= 0) && (x < Width)
			 && (y >= 0) && (y < Height))
			{
				int planeOffset = ((y & 1) == 0)
					? _plane0Offset
					: _plane1Offset;

				y >>= 1;

				int offset = y * _stride + (x >> 3);
				int shift = 7 - (x & 7);

				var bits = Array.VRAM[planeOffset + offset];

				return (bits >> shift) & 1;
			}

			return 0;
		}
	}

	public override void PixelSet(int x, int y, int attribute)
	{
		if (!Clip.Contains(x, y))
			return;

		using (HidePointerForOperationIfPointerAware(x, y))
		{
			if ((x >= 0) && (x < Width)
			 && (y >= 0) && (y < Height))
			{
				int planeOffset = ((y & 1) == 0)
					? _plane0Offset
					: _plane1Offset;

				y >>= 1;

				int offset = y * _stride + (x >> 3);
				int shift = 7 - (x & 7);
				int bitMask = 1 << shift;

				Array.VRAM[planeOffset + offset] = unchecked((byte)(
					(Array.VRAM[planeOffset + offset] & ~bitMask) |
					((attribute & 1) << shift)));
			}
		}
	}

	public override void HorizontalLine(int x1, int x2, int y, int attribute)
	{
		if ((x2 < Clip.X1) || (x1 >= Clip.X2))
			return;
		if ((y < Clip.Y1) || (y >= Clip.Y2))
			return;

		if (x1 > x2)
			return;

		if (x1 < Clip.X1)
			x1 = Clip.X1;
		if (x2 >= Clip.X2)
			x2 = Clip.X2 - 1;

		HorizontalLinePreClipped(x1, x2, y, attribute);
	}

	void HorizontalLinePreClipped(int x1, int x2, int y, int attribute)
	{
		using (HidePointerForOperationIfPointerAware(x1, y, x2, y))
		{
			int planeOffset = ((y & 1) == 0)
				? _plane0Offset
				: _plane1Offset;

			y >>= 1;

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
					int address = planeOffset + scanOffset + x1Byte;

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
					vramSpan.Slice(planeOffset + scanOffset + firstCompleteByte, completeBytes).Fill(completeByteValue);

				if (leftPixels != 0)
				{
					byte mask = unchecked((byte)(0b11111111 >> (8 - leftPixels)));

					int address = planeOffset + scanOffset + firstCompleteByte - 1;

					if (attributeValue)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
				}

				if (rightPixels != 0)
				{
					byte mask = unchecked((byte)(0b11111111 << (8 - rightPixels)));

					int address = planeOffset + scanOffset + lastCompleteByte + 1;

					if (attributeValue)
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] | mask));
					else
						Array.VRAM[address] = unchecked((byte)(Array.VRAM[address] & ~mask));
				}
			}
		}
	}

	public override void GetSprite(int x1, int y1, int x2, int y2, Span<byte> buffer)
	{
		using (HidePointerForOperationIfPointerAware(x1, y1, x2, y2))
		{
			int x = x1, y = y1;
			int w = x2 - x1 + 1, h = y2 - y1 + 1;

			if ((x < 0) || (y < 0) || (w < 0) || (h < 0))
				throw new InvalidOperationException();
			if ((x + w > Width) || (y + h > Height))
				throw new InvalidOperationException();

			var vramSpan = Array.VRAM.AsSpan();

			var plane0 = vramSpan.Slice(_plane0Offset, PlaneSize);
			var plane1 = vramSpan.Slice(_plane1Offset, PlaneSize);

			if ((y & 1) != 0)
			{
				var tmp = plane0;

				plane0 = plane1;
				plane1 = tmp.Slice(_stride);
			}

			y >>= 1;

			plane0 = plane0.Slice(y * _stride);
			plane1 = plane1.Slice(y * _stride);

			int bytesPerScan = (w + 7) / 8;

			int headerBytes = 4;
			int dataBytes = bytesPerScan * h;

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

			int bitsForNextPixel = 0;

			int lastByteMask = unchecked((byte)(255 << (w & 7)));

			for (int yy = 0; yy < h; yy++)
			{
				int o = (yy >> 1) * _stride + (x >> 3);
				int p = yy * bytesPerScan;

				var plane = ((yy & 1) == 0) ? plane0 : plane1;

				if (rightPixelMask != 0)
				{
					// Input and output bytes are not aligned, and the offset o has only
					// some of the bits for the first output byte.
					int sample = plane[o];

					bitsForNextPixel = (sample & rightPixelMask) << rightPixelShift;

					o++;
				}

				for (int xx = 0; xx < w; xx += 8, o++, p++)
				{
					int sample = plane[o];

					data[p] = unchecked((byte)(
						bitsForNextPixel | ((sample & leftPixelMask) >> leftPixelShift)));

					bitsForNextPixel = (sample & rightPixelMask) << rightPixelShift;
				}

				data[p - 1] = unchecked((byte)(data[p - 1] & lastByteMask));

				if (rightPixelShift != 0)
					bitsForNextPixel = (plane[o++] & rightPixelMask) << rightPixelShift;
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

		if ((x < Clip.X1) || (y < Clip.Y1) || (w < 0) || (h < 0))
			throw new InvalidOperationException();
		if ((x + w - 1 > Clip.X2) || (y + h - 1 > Clip.Y2))
			throw new InvalidOperationException();

		using (HidePointerForOperationIfPointerAware(x, y, x + w - 1, y + h - 1))
		{
			var vramSpan = Array.VRAM.AsSpan();

			var plane0 = vramSpan.Slice(_plane0Offset, _planeBytesUsed);
			var plane1 = vramSpan.Slice(_plane1Offset, _planeBytesUsed);

			if ((y & 1) != 0)
			{
				var tmp = plane0;

				plane0 = plane1;
				plane1 = tmp.Slice(_stride);
			}

			y >>= 1;

			plane0 = plane0.Slice(y * _stride);
			plane1 = plane1.Slice(y * _stride);

			int bytesPerScan = (w + 7) / 8;

			int dataBytes = bytesPerScan * h;

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

			int planeMask = Array.Graphics.Registers.BitMask;

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
				lastOutputBytePixels = 8;
			}

			var action = new TAction();

			bool needToReadPlaneBits = action.UsesPlaneBits;

			for (int yy = 0; yy < h; yy++)
			{
				int o = (yy >> 1) * _stride + (x >> 3);
				int p = yy * bytesPerScan;

				var plane = ((yy & 1) == 0) ? plane0 : plane1;

				int spriteMask = leftPixelMask >> leftPixelShift;
				int unrelatedMask = unchecked((byte)~spriteMask);

				int bitsForNextPixel = 0;

				bool readPlaneBits = (unrelatedMask != 0) || needToReadPlaneBits;

				for (int xx = 0; xx < loopBytes; xx++, o++, p++)
				{
					byte planeByte = readPlaneBits ? plane[o] : (byte)0;

					int sample = data[p];

					int spriteByte = bitsForNextPixel | ((sample & leftPixelMask) >> leftPixelShift);

					plane[o] = action.ApplySpriteBits(planeByte, spriteByte, unrelatedMask, spriteMask);

					bitsForNextPixel = (sample & rightPixelMask) << rightPixelShift;

					unrelatedMask = 0;
					spriteMask = 255;
					readPlaneBits = needToReadPlaneBits;
				}

				if (lastByteMask != 0)
				{
					unrelatedMask = unchecked((byte)~lastByteMask);
					spriteMask = lastByteMask;

					if (subByteAlignment != 0)
						bitsForNextPixel = data[p] >> subByteAlignment;
					else if (lastDataBytePixels <= lastOutputBytePixels)
						bitsForNextPixel |= data[p] >> leftPixelShift;

					plane[o] = action.ApplySpriteBits(plane[o], bitsForNextPixel, unrelatedMask, spriteMask);
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

		var xorHeader = MemoryMarshal.Cast<byte, short>(xor.Slice(0, headerBytes));

		int xorW = xorHeader[0];
		int xorH = xorHeader[1];

		var andHeader = MemoryMarshal.Cast<byte, short>(and.Slice(0, headerBytes));

		int andW = andHeader[0];
		int andH = andHeader[1];

		int minW = Math.Min(andW, xorW);
		int maxW = Math.Max(andW, xorW);
		int maxH = Math.Max(andH, xorH);

		if ((andW < 0) || (andH < 0))
			throw new InvalidOperationException();
		if ((xorW < 0) || (xorH < 0))
			throw new InvalidOperationException();
		if ((x + maxW <= Clip.X1) || (y + maxH <= Clip.Y1))
			return;
		if ((x > Clip.X2) || (y > Clip.Y2))
			return;

		using (HidePointerForOperationIfPointerAware(x, y, x + maxW - 1, y + maxH - 1))
		{
			var vramSpan = Array.VRAM.AsSpan();

			var plane0 = vramSpan.Slice(_plane0Offset, _planeBytesUsed);
			var plane1 = vramSpan.Slice(_plane1Offset, _planeBytesUsed);

			if ((y & 1) != 0)
			{
				var tmp = plane0;

				plane0 = plane1;
				plane1 = tmp.Slice(_stride);
			}

			y >>= 1;

			plane0 = plane0.Slice(y * _stride);
			plane1 = plane1.Slice(y * _stride);

			int andBytesPerScan = (andW + 7) / 8;
			int xorBytesPerScan = (xorW + 7) / 8;

			int andDataBytes = andBytesPerScan * andH;
			int xorDataBytes = xorBytesPerScan * xorH;

			int totalAndBytes = headerBytes + andDataBytes;
			int totalXorBytes = headerBytes + xorDataBytes;

			if (and.Length < totalAndBytes)
				throw new InvalidOperationException();
			if (xor.Length < totalXorBytes)
				throw new InvalidOperationException();

			var andData = and.Slice(headerBytes, andDataBytes);
			var xorData = xor.Slice(headerBytes, xorDataBytes);

			int my = y - Clip.Y1;

			if (my < 0)
			{
				andData = andData.Slice(-my * andBytesPerScan);
				xorData = xorData.Slice(-my * xorBytesPerScan);

				andH += my;
				xorH += my;
				y = Clip.Y1;
			}

			if (y + andH - 1 > Clip.Y2)
				andH = Clip.Y2 - y + 1;
			if (y + xorH - 1 > Clip.Y2)
				xorH = Clip.Y2 - y + 1;

			// 76543210
			// ^-------  x = 0
			//    ^---- ---   x = 3

			int leftPixelShift = x & 7;
			int rightPixelShift = (8 - (x & 7)) & 7;
			int leftPixelMask = unchecked((byte)(255 << leftPixelShift));
			int rightPixelMask = unchecked((byte)(255 >> rightPixelShift));

			if (leftPixelShift == 0)
				rightPixelMask = 0;

			int planeMask = Array.Graphics.Registers.BitMask;

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
				int o = (yy >> 1) * _stride + (x >> 3);
				int p = yy * andBytesPerScan;
				int q = yy * xorBytesPerScan;

				var plane = ((yy & 1) == 0) ? plane0 : plane1;

				int spriteMask = leftPixelMask >> leftPixelShift;
				int unrelatedMask = unchecked((byte)~spriteMask);

				int andBitsForNextPixel = 0;
				int xorBitsForNextPixel = 0;

				var andScan = andData.Slice(p, andBytesPerScan);
				var xorScan = xorData.Slice(q, xorBytesPerScan);

				for (int xx = 0; xx < loopBytes; xx++, o++)
				{
					if (xx == andBytesPerScan)
						andScan = onesScan;
					if (xx == xorBytesPerScan)
						xorScan = zeroesScan;

					int andSample = andScan[xx];
					int xorSample = xorScan[xx];

					int andByte = andBitsForNextPixel | ((andSample & leftPixelMask) >> leftPixelShift);
					int xorByte = xorBitsForNextPixel | ((xorSample & leftPixelMask) >> leftPixelShift);

					int rx = xx + x;

					if ((rx >= Clip.X1) && (rx <= Clip.X2))
					{
						byte planeByte = plane[o];

						planeByte = andAction.ApplySpriteBits(planeByte, andByte, unrelatedMask, spriteMask);
						planeByte = xorAction.ApplySpriteBits(planeByte, xorByte, unrelatedMask, spriteMask);

						plane[o] = planeByte;
					}

					andBitsForNextPixel = (andSample & rightPixelMask) << rightPixelShift;
					xorBitsForNextPixel = (xorSample & rightPixelMask) << rightPixelShift;

					unrelatedMask = 0;
					spriteMask = 255;
				}

				if ((lastByteMask != 0) && (x + minW - 1 <= Clip.X2))
				{
					if (loopBytes == andBytesPerScan)
						andScan = onesScan;
					if (loopBytes == xorBytesPerScan)
						xorScan = zeroesScan;

					unrelatedMask = unchecked((byte)~lastByteMask);
					spriteMask = lastByteMask;

					if (subByteAlignment != 0)
					{
						andBitsForNextPixel = (andScan[loopBytes] & lastAndBytePixels) >> subByteAlignment;
						xorBitsForNextPixel = (xorScan[loopBytes] & lastXorBytePixels) >> subByteAlignment;
					}
					else
					{
						if (lastAndBytePixels <= lastOutputBytePixels)
							andBitsForNextPixel |= andScan[loopBytes] >> leftPixelShift;
						if (lastXorBytePixels <= lastOutputBytePixels)
							xorBitsForNextPixel |= xorScan[loopBytes] >> leftPixelShift;
					}

					byte planeByte = plane[o];

					planeByte = andAction.ApplySpriteBits(planeByte, andBitsForNextPixel, unrelatedMask, spriteMask);
					planeByte = xorAction.ApplySpriteBits(planeByte, xorBitsForNextPixel, unrelatedMask, spriteMask);

					plane[o] = planeByte;
				}
			}
		}
	}

	public override void ScrollUp(int scanCount, int windowStart, int windowEnd)
	{
		using (HidePointerForOperationIfPointerAware(0, windowStart, Width, windowEnd + scanCount))
		{
			var vramSpan = Array.VRAM.AsSpan();

			var plane0 = vramSpan.Slice(_plane0Offset, _planeBytesUsed);
			var plane1 = vramSpan.Slice(_plane1Offset, _planeBytesUsed);

			for (int yFrom = windowStart + scanCount, yTo = windowStart; yFrom <= windowEnd; yFrom++, yTo++)
			{
				var planeFrom = ((yFrom & 1) == 0) ? plane0 : plane1;
				var planeTo = ((yTo & 1) == 0) ? plane0 : plane1;

				var bitsFrom = planeFrom.Slice((yFrom >> 1) * _stride, _stride);
				var bitsTo = planeTo.Slice((yTo >> 1) * _stride, _stride);

				bitsFrom.CopyTo(bitsTo);
			}

			for (int yTo = windowEnd - scanCount + 1; yTo <= windowEnd; yTo++)
			{
				var planeTo = ((yTo & 1) == 0) ? plane0 : plane1;

				var bitsTo = planeTo.Slice((yTo >> 1) * _stride, _stride);

				bitsTo.Fill(0);
			}
		}
	}

	public override void ScrollDown(int scanCount, int windowStart, int windowEnd)
	{
		using (HidePointerForOperationIfPointerAware(0, windowStart, Width, windowEnd + scanCount))
		{
			var vramSpan = Array.VRAM.AsSpan();

			var plane0 = vramSpan.Slice(_plane0Offset, _planeBytesUsed);
			var plane1 = vramSpan.Slice(_plane1Offset, _planeBytesUsed);

			for (int yFrom = windowEnd - scanCount, yTo = windowEnd; yFrom >= windowStart; yFrom--, yTo--)
			{
				var planeFrom = ((yFrom & 1) == 0) ? plane0 : plane1;
				var planeTo = ((yTo & 1) == 0) ? plane0 : plane1;

				var bitsFrom = planeFrom.Slice((yFrom >> 1) * _stride, _stride);
				var bitsTo = planeTo.Slice((yTo >> 1) * _stride, _stride);

				bitsFrom.CopyTo(bitsTo);
			}

			for (int yTo = windowStart + scanCount - 1; yTo >= windowStart; yTo--)
			{
				var planeTo = ((yTo & 1) == 0) ? plane0 : plane1;

				var bitsTo = planeTo.Slice((yTo >> 1) * _stride, _stride);

				bitsTo.Fill(0);
			}
		}
	}

	protected override void DrawCharacterScan(int x, int y, int characterWidth, byte glyphScan)
	{
		if ((x & 7) != 0)
			base.DrawCharacterScan(x, y, characterWidth, glyphScan);
		else if ((DrawingAttribute & 0x80) == 0)
		{
			var vramSpan = Array.VRAM.AsSpan();

			var plane = ((y & 1) == 0)
				? vramSpan.Slice(_plane0Offset, _planeBytesUsed)
				: vramSpan.Slice(_plane1Offset, _planeBytesUsed);

			y >>= 1;

			int o = y * _stride + (x >> 3);

			if ((o >= 0) && (o < _planeBytesUsed))
				plane[o] = glyphScan;
		}
		else
		{
			var vramSpan = Array.VRAM.AsSpan();

			var plane = ((y & 1) == 0)
				? vramSpan.Slice(_plane0Offset, _planeBytesUsed)
				: vramSpan.Slice(_plane1Offset, _planeBytesUsed);

			y >>= 1;

			int o = y * _stride + (x >> 3);

			if ((o >= 0) && (o < _planeBytesUsed))
				plane[o] ^= glyphScan;
		}
	}
}
