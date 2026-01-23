using System;
using System.Runtime.InteropServices;

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

	public override int PixelsPerByte => 8;
	public override int MaximumAttribute => 1;

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

	public override int PixelGet(int x, int y)
	{
		if ((x >= 0) && (x < Width)
		 && (y >= 0) && (y < Height))
		{
			int offset = y * _stride + (x >> 3);
			int shift = 7 - (x & 7);

			var bits = Array.VRAM[_plane0Offset + offset];

			return (bits >> shift) & 1;
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

	public override void GetSprite(int x, int y, int w, int h, Span<byte> buffer)
	{
		if ((x < 0) || (y < 0) || (w < 0) || (h < 0))
			throw new InvalidOperationException();
		if ((x + w >= Width) || (y + h >= Height))
			throw new InvalidOperationException();

		var vramSpan = Array.VRAM.AsSpan();

		var plane = vramSpan.Slice(StartAddress);

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
			int o = (y + yy) * _stride + (x >> 3);
			int p = yy * bytesPerScan;

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

		var plane = vramSpan.Slice(StartAddress);

		var plane0 = vramSpan.Slice(_plane0Offset, _planeBytesUsed);
		var plane1 = vramSpan.Slice(_plane1Offset, _planeBytesUsed);
		var plane2 = vramSpan.Slice(_plane2Offset, _planeBytesUsed);
		var plane3 = vramSpan.Slice(_plane3Offset, _planeBytesUsed);

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

		int lastOutputBytePixels = (x + w) & 7;

		int lastByteMask = unchecked((byte)~(255 >> lastOutputBytePixels));

		int startXX = 7 - ((x - 1) & 7);

		var action = new TAction();

		for (int yy = 0; yy < h; yy++)
		{
			int o = (y + yy) * _stride + (x >> 3);
			int p = yy * bytesPerScan;

			int spriteMask = leftPixelMask >> leftPixelShift;
			int unrelatedMask = unchecked((byte)~spriteMask);

			int bitsForNextPixel = 0;

			for (int xx = startXX; xx < w; xx += 8, o++, p++)
			{
				byte planeByte = (unrelatedMask == 0) ? (byte)0 : plane[o];

				int sample = data[p];

				int spriteByte = bitsForNextPixel | ((sample & leftPixelMask) >> leftPixelShift);

				planeByte = action.ApplySpriteBits(planeByte, spriteByte, unrelatedMask, spriteMask);

				if ((planeMask & 1) != 0)
					plane0[o] = planeByte;
				if ((planeMask & 2) != 0)
					plane1[o] = planeByte;
				if ((planeMask & 4) != 0)
					plane2[o] = planeByte;
				if ((planeMask & 8) != 0)
					plane3[o] = planeByte;

				bitsForNextPixel = (sample & rightPixelMask) << rightPixelShift;

				unrelatedMask = 0;
				spriteMask = 255;
			}

			if (lastByteMask != 0)
			{
				unrelatedMask = unchecked((byte)~lastByteMask);
				spriteMask = lastByteMask;

				byte planeByte = plane[o];

				bitsForNextPixel |= data[p - 1] << (8 - lastOutputBytePixels);

				planeByte = action.ApplySpriteBits(planeByte, bitsForNextPixel, unrelatedMask, spriteMask);

				if ((planeMask & 1) != 0)
					plane0[o] = planeByte;
				if ((planeMask & 2) != 0)
					plane1[o] = planeByte;
				if ((planeMask & 4) != 0)
					plane2[o] = planeByte;
				if ((planeMask & 8) != 0)
					plane3[o] = planeByte;
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
