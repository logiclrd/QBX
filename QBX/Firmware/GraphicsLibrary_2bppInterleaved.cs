using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using QBX.Hardware;

namespace QBX.Firmware;

public class GraphicsLibrary_2bppInterleaved : GraphicsLibrary
{
	public GraphicsLibrary_2bppInterleaved(Machine machine)
		: base(machine)
	{
		DrawingAttribute = 3;
		RefreshParameters();
	}

	int _planeBytesUsed;
	int _stride;
	int _plane0Offset;
	int _plane1Offset;
	int _plane2Offset;
	int _plane3Offset;

	public override int PixelsPerByte => 4;
	public override int MaximumAttribute => 3;

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
				bool evenColumn = ((x & 1) == 0);
				bool evenScan = ((y & 1) == 0);

				int planeOffset = evenScan
					? (evenColumn ? _plane0Offset : _plane2Offset)
					: (evenColumn ? _plane1Offset : _plane3Offset);

				int offset = (y >> 1) * _stride + (x >> 3);
				int shift = 6 - ((x >> 1) & 3) * 2;

				int address = planeOffset + offset;

				var bits = Array.VRAM[address];

				return (bits >> shift) & 0b11;
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
			attribute &= 3;

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

	public override void GetSprite(int x1, int y1, int x2, int y2, Span<byte> buffer)
	{
		int x = x1, y = y1;
		int w = x2 - x1 + 1, h = y2 - y1 + 1;

		// TODO: rework to avoid using PixelGet
		if ((x < 0) || (y < 0) || (w < 0) || (h < 0))
			throw new InvalidOperationException();
		if ((x + w > Width) || (y + h > Height))
			throw new InvalidOperationException();

		using (HidePointerForOperationIfPointerAware(x1, y1, x2, y2))
		{
			var vramSpan = Array.VRAM.AsSpan();

			var plane = vramSpan.Slice(StartAddress, 64000);

			int bytesPerScan = (w + 3) / 4;

			int headerBytes = 4;
			int dataBytes = bytesPerScan * h;

			int totalBytes = headerBytes + dataBytes;

			if (buffer.Length < totalBytes)
				throw new InvalidOperationException();

			var header = MemoryMarshal.Cast<byte, short>(buffer.Slice(0, headerBytes));

			header[0] = (short)(w * 2);
			header[1] = (short)h;

			var data = buffer.Slice(headerBytes);

			int o = 0;

			for (int yy = 0; yy < h; yy++)
				for (int xx = 0; xx < w; xx += 4, o++)
				{
					int p0 = PixelGet(x + xx + 0, y + yy);
					int p1 = (xx + 1 < w) ? PixelGet(x + xx + 1, y + yy) : 0;
					int p2 = (xx + 2 < w) ? PixelGet(x + xx + 2, y + yy) : 0;
					int p3 = (xx + 3 < w) ? PixelGet(x + xx + 3, y + yy) : 0;

					data[o] = unchecked((byte)(
						(p0 << 6) | (p1 << 4) | (p2 << 2) | p3));
				}
		}
	}

	public override void PutSprite(Span<byte> buffer, PutSpriteAction action, int x, int y)
	{
		switch (action)
		{
			case PutSpriteAction.PixelSet: PutSprite(buffer, x, y); break;
			case PutSpriteAction.PixelSetInverted: PutSprite<SpriteOperation_PixelSetInverted>(buffer, x, y); break;
			case PutSpriteAction.And: PutSprite<SpriteOperation_And>(buffer, x, y); break;
			case PutSpriteAction.Or: PutSprite<SpriteOperation_Or>(buffer, x, y); break;
			case PutSpriteAction.ExclusiveOr: PutSprite<SpriteOperation_ExclusiveOr>(buffer, x, y); break;
		}
	}

	void PutSprite(Span<byte> buffer, int x, int y)
	{
		// TODO: rework to avoid using PixelGet
		if (buffer.Length < 4)
			throw new InvalidOperationException();

		int headerBytes = 4;

		var header = MemoryMarshal.Cast<byte, short>(buffer.Slice(0, headerBytes));

		int w = header[0] / 2;
		int h = header[1];

		if ((x < 0) || (y < 0) || (w < 0) || (h < 0))
			throw new InvalidOperationException();
		if ((x + w > Width) || (y + h > Height))
			throw new InvalidOperationException();

		using (HidePointerForOperationIfPointerAware(x, y, x + w - 1, y + h - 1))
		{
			var vramSpan = Array.VRAM.AsSpan();

			var plane = vramSpan.Slice(StartAddress, 64000);

			int bytesPerScan = (w + 3) / 4;

			int dataBytes = bytesPerScan * h;

			int totalBytes = headerBytes + dataBytes;

			if (buffer.Length < totalBytes)
				throw new InvalidOperationException();

			var data = buffer.Slice(headerBytes);

			int o = 0;
			int packed = 0;

			for (int yy = 0; yy < h; yy++)
			{
				for (int xx = 0; xx < w; xx++)
				{
					if ((xx & 3) == 0)
						packed = data[o++];

					PixelSet(x + xx, y + yy, (packed >> 6) & 3);

					packed <<= 2;
				}
			}
		}
	}

	void PutSprite<TAction>(Span<byte> buffer, int x, int y)
		where TAction : ISpriteOperation, new()
	{
		// TODO: rework to avoid using PixelGet
		if (buffer.Length < 4)
			throw new InvalidOperationException();

		int headerBytes = 4;

		var header = MemoryMarshal.Cast<byte, short>(buffer.Slice(0, headerBytes));

		int w = header[0] / 2;
		int h = header[1];

		if ((x < 0) || (y < 0) || (w < 0) || (h < 0))
			throw new InvalidOperationException();
		if ((x + w > Width) || (y + h > Height))
			throw new InvalidOperationException();

		using (HidePointerForOperationIfPointerAware(x, y, x + w - 1, y + h - 1))
		{
			var vramSpan = Array.VRAM.AsSpan();

			var plane = vramSpan.Slice(StartAddress, 64000);

			int bytesPerScan = (w + 3) / 4;

			int dataBytes = bytesPerScan * h;

			int totalBytes = headerBytes + dataBytes;

			if (buffer.Length < totalBytes)
				throw new InvalidOperationException();

			var data = buffer.Slice(headerBytes);

			int o = 0;

			var action = new TAction();

			for (int yy = 0; yy < h; yy++)
			{
				int packed = data[o++];

				for (int xx = 0; xx < w; xx++)
				{
					byte planePixel = unchecked((byte)PixelGet(x + xx, y + yy));
					int dataPixel = ((packed >> 6) & 3);

					PixelSet(x + xx, y + yy, action.ApplySpriteBits(planePixel, dataPixel, 0, 0xFF));

					packed <<= 2;

					if ((xx & 3) == 3)
						packed = data[o++];
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
		// TODO: rework to avoid using PixelGet
		if (xor.Length < 4)
			throw new InvalidOperationException();
		if (and.Length < 4)
			throw new InvalidOperationException();

		int headerBytes = 4;

		var andHeader = MemoryMarshal.Cast<byte, short>(and.Slice(0, headerBytes));

		int andW = andHeader[0] / 2;
		int andH = andHeader[1];

		var xorHeader = MemoryMarshal.Cast<byte, short>(xor.Slice(0, headerBytes));

		int xorW = xorHeader[0] / 2;
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

			var plane = vramSpan.Slice(StartAddress, 64000);

			int andBytesPerScan = (andW + 3) / 4;
			int xorBytesPerScan = (xorW + 3) / 4;

			int andDataBytes = andBytesPerScan * andH;
			int xorDataBytes = xorBytesPerScan * xorH;

			int totalAndBytes = headerBytes + andDataBytes;
			int totalXorBytes = headerBytes + xorDataBytes;

			if (and.Length < totalAndBytes)
				throw new InvalidOperationException();
			if (xor.Length < totalXorBytes)
				throw new InvalidOperationException();

			var andData = and.Slice(headerBytes);
			var xorData = xor.Slice(headerBytes);

			if (y < 0)
			{
				xorData = xorData.Slice(-y * xorBytesPerScan);
				andData = andData.Slice(-y * andBytesPerScan);

				andH += y;
				xorH += y;
				y = 0;
			}

			if (y + andH >= Height)
				andH = Height - y;
			if (y + xorH >= Height)
				xorH = Height - y;

			maxH = Math.Max(andH, xorH);

			int andPacked = 0;
			int xorPacked = 0;

			int maxBytesPerScan = Math.Max(andBytesPerScan, xorBytesPerScan);

			if ((andH < maxH) || (andW < maxW))
			{
				if ((s_ones == null) || (s_ones.Length < maxBytesPerScan))
				{
					s_ones = new byte[maxBytesPerScan * 2];
					s_ones.AsSpan().Fill(0xFF);
				}
			}

			if ((xorH < maxH) || (xorW < maxW))
			{
				if ((s_zeroes == null) || (s_zeroes.Length < maxBytesPerScan))
					s_zeroes = new byte[maxBytesPerScan * 2];
			}

			var onesScan = s_ones.AsSpan();
			var zeroesScan = s_zeroes.AsSpan();

			for (int yy = 0; yy < maxH; yy++)
			{
				var andScan = (yy < andH) ? andData.Slice(yy * andBytesPerScan) : onesScan;
				var xorScan = (yy < xorH) ? xorData.Slice(yy * xorBytesPerScan) : zeroesScan;

				for (int xx = 0, o = 0; xx < maxW; xx++)
				{
					if ((xx & 3) == 0)
					{
						if (o == andScan.Length)
							andScan = onesScan;
						if (o == xorScan.Length)
							xorScan = zeroesScan;

						xorPacked = xorScan[o];
						andPacked = andScan[o];

						o++;
					}

					int rx = xx + x;

					if ((rx >= 0) && (rx < Width))
					{
						int andBits = (andPacked >> 6) & 3;
						int xorBits = (xorPacked >> 6) & 3;

						if (andBits == 0)
							PixelSet(x + xx, y + yy, (xorPacked >> 6) & 3);
						else
						{
							int existing = PixelGet(x + xx, y + yy);

							int newColour = (existing & andBits) ^ xorBits;

							PixelSet(x + xx, y + yy, newColour & 3);
						}
					}

					andPacked <<= 2;
					xorPacked <<= 2;
				}
			}
		}
	}

	public override void ScrollUp(int scanCount, int windowStart, int windowEnd)
	{
		if (scanCount == 0)
			return;

		using (HidePointerForOperationIfPointerAware(0, windowStart, Width, windowEnd + scanCount))
		{
			var vramSpan = Array.VRAM.AsSpan();

			var plane0 = vramSpan.Slice(_plane0Offset, _planeBytesUsed);
			var plane1 = vramSpan.Slice(_plane1Offset, _planeBytesUsed);
			var plane2 = vramSpan.Slice(_plane2Offset, _planeBytesUsed);
			var plane3 = vramSpan.Slice(_plane3Offset, _planeBytesUsed);

			int windowOffset = windowStart * _stride;
			int windowLength = (windowEnd - windowStart + 1) * _stride;

			plane0 = plane0.Slice(windowOffset, windowLength);
			plane1 = plane1.Slice(windowOffset, windowLength);
			plane2 = plane2.Slice(windowOffset, windowLength);
			plane3 = plane3.Slice(windowOffset, windowLength);

			if ((scanCount & 1) == 0)
			{
				// Even number of scans: planes do not swap.
				int copyOffset = (scanCount >> 1) * _stride;

				plane0.Slice(copyOffset).CopyTo(plane0);
				plane2.Slice(copyOffset).CopyTo(plane2);
				plane1.Slice(copyOffset).CopyTo(plane1);
				plane3.Slice(copyOffset).CopyTo(plane3);

				plane0.Slice(plane0.Length - copyOffset).Fill(0);
				plane1.Slice(plane1.Length - copyOffset).Fill(0);
				plane2.Slice(plane2.Length - copyOffset).Fill(0);
				plane3.Slice(plane3.Length - copyOffset).Fill(0);
			}
			else
			{
				// Odd number of scans: planes do swap.
				int windowScans = windowEnd - windowStart + 1;

				int evenFirstScan;
				int oddFirstScan;

				int evenTotalScanCount;
				int oddTotalScanCount;

				if ((windowStart & 1) == 0)
				{
					evenFirstScan = windowStart >> 1;
					oddFirstScan = evenFirstScan;

					evenTotalScanCount = windowScans >> 1;
					oddTotalScanCount = ((windowScans - 1) >> 1) + 1;
				}
				else
				{
					evenFirstScan = (windowStart >> 1) + 1;
					oddFirstScan = evenFirstScan - 1;

					evenTotalScanCount = (windowScans - 1) >> 1;
					oddTotalScanCount = (windowScans >> 1) + 1;
				}

				plane0 = plane0.Slice(evenFirstScan * _stride, evenTotalScanCount * _stride);
				plane1 = plane1.Slice(oddFirstScan * _stride, oddTotalScanCount * _stride);
				plane2 = plane2.Slice(evenFirstScan * _stride, evenTotalScanCount * _stride);
				plane3 = plane3.Slice(oddFirstScan * _stride, oddTotalScanCount * _stride);

				int copyCount = Height - scanCount;
				int copyOffset = (scanCount >> 1) * _stride;

				var copy0 = plane0;
				var copy1 = plane1;
				var copy2 = plane2;
				var copy3 = plane3;

				if (evenFirstScan > oddFirstScan)
				{
					Span<byte> tmp;

					tmp = copy0;
					copy0 = copy1;
					copy1 = tmp;

					tmp = copy2;
					copy2 = copy3;
					copy3 = tmp;
				}

				for (int i = 0; i < copyCount; i++)
				{
					copy1.Slice(copyOffset, _stride).CopyTo(copy0);
					copy3.Slice(copyOffset, _stride).CopyTo(copy2);

					Span<byte> tmp;

					tmp = copy0;
					copy0 = copy1;
					copy1 = tmp.Slice(_stride);

					tmp = copy2;
					copy2 = copy3;
					copy3 = tmp.Slice(_stride);
				}

				int evenFillScanCount;
				int oddFillScanCount;

				if ((windowEnd & 1) == 0)
				{
					evenFillScanCount = scanCount >> 1;
					oddFillScanCount = (scanCount + 1) >> 1;
				}
				else
				{
					evenFillScanCount = (scanCount + 1) >> 1;
					oddFillScanCount = scanCount >> 1;
				}

				int halfHeight = (windowEnd - windowStart + 1) >> 1;

				int oddFillOffset = (oddTotalScanCount - oddFillScanCount) * _stride;
				int evenFillOffset = (evenTotalScanCount - evenFillScanCount) * _stride;

				if (oddFillScanCount > 0)
				{
					plane1.Slice(oddFillOffset).Fill(0);
					plane3.Slice(oddFillOffset).Fill(0);
				}

				if (evenFillScanCount > 0)
				{
					plane0.Slice(evenFillOffset).Fill(0);
					plane2.Slice(evenFillOffset).Fill(0);
				}
			}
		}
	}
}
