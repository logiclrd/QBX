using System;
using System.Runtime.InteropServices;

using QBX.Hardware;

namespace QBX.Firmware;

public class GraphicsLibrary_8bppFlat : GraphicsLibrary
{
	public GraphicsLibrary_8bppFlat(Machine machine)
		: base(machine)
	{
		DrawingAttribute = 15;
		RefreshParameters();
	}

	public override int PixelsPerByte => 1;
	public override int MaximumAttribute => 255;

	protected override void ClearGraphicsImplementation(int windowStart, int windowEnd)
	{
		int windowOffset = windowStart * Width;
		int windowLength = (windowEnd - windowStart + 1) * Width;

		Array.VRAM.AsSpan().Slice(StartAddress + windowOffset, windowLength).Clear();
	}

	public override int PixelGet(int x, int y)
	{
		if ((x >= 0) && (x < Width)
		 && (y >= 0) && (y < Height))
			return Array.VRAM[StartAddress + y * Width + x];

		return 0;
	}

	public override void PixelSet(int x, int y, int attribute)
	{
		if ((x >= 0) && (x < Width)
		 && (y >= 0) && (y < Height))
			Array.VRAM[StartAddress + y * Width + x] = unchecked((byte)attribute);
	}

	public override void HorizontalLine(int x1, int x2, int y, int attribute)
	{
		if ((x2 < 0) || (x1 >= Width))
			return;
		if ((y < 0) || (y >= Height))
			return;

		if (x1 < 0)
			x1 = 0;
		if (x2 >= Width)
			x2 = Width - 1;

		int o = StartAddress + y * Width + x1;

		Array.VRAM.AsSpan().Slice(o, x2 - x1 + 1).Fill(unchecked((byte)attribute));
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

		var plane = vramSpan.Slice(StartAddress, 64000);

		int bytesPerScan = w;

		int headerBytes = 4;
		int dataBytes = bytesPerScan * h;

		int totalBytes = headerBytes + dataBytes;

		if (buffer.Length < totalBytes)
			throw new InvalidOperationException();

		var header = MemoryMarshal.Cast<byte, short>(buffer.Slice(0, headerBytes));

		header[0] = (short)(w * 8);
		header[1] = (short)h;

		var data = buffer.Slice(headerBytes, dataBytes);

		for (int yy = 0; yy < h; yy++)
		{
			int o = (y + yy) * 320 + x;
			int p = yy * bytesPerScan;

			plane.Slice(o, w).CopyTo(data.Slice(p));
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
		if (buffer.Length < 4)
			throw new InvalidOperationException();

		int headerBytes = 4;

		var header = MemoryMarshal.Cast<byte, short>(buffer.Slice(0, headerBytes));

		int w = header[0] / 8;
		int h = header[1];

		if ((x < 0) || (y < 0) || (w < 0) || (h < 0))
			throw new InvalidOperationException();
		if ((x + w >= Width) || (y + h >= Height))
			throw new InvalidOperationException();

		var vramSpan = Array.VRAM.AsSpan();

		var plane = vramSpan.Slice(StartAddress, 64000);

		int bytesPerScan = w;

		int dataBytes = bytesPerScan * h;

		int totalBytes = headerBytes + dataBytes;

		if (buffer.Length < totalBytes)
			throw new InvalidOperationException();

		var data = buffer.Slice(headerBytes, dataBytes);

		for (int yy = 0; yy < h; yy++)
		{
			int o = (y + yy) * 320 + x;
			int p = yy * bytesPerScan;

			data.Slice(p, w).CopyTo(plane.Slice(o));
		}
	}

	void PutSprite<TAction>(Span<byte> buffer, int x, int y)
		where TAction : ISpriteOperation, new()
	{
		if (buffer.Length < 4)
			throw new InvalidOperationException();

		int headerBytes = 4;

		var header = MemoryMarshal.Cast<byte, short>(buffer.Slice(0, headerBytes));

		int w = header[0] / 8;
		int h = header[1];

		if ((x < 0) || (y < 0) || (w < 0) || (h < 0))
			throw new InvalidOperationException();
		if ((x + w >= Width) || (y + h >= Height))
			throw new InvalidOperationException();

		var vramSpan = Array.VRAM.AsSpan();

		var plane = vramSpan.Slice(StartAddress, 64000);

		int bytesPerScan = w;

		int dataBytes = bytesPerScan * h;

		int totalBytes = headerBytes + dataBytes;

		if (buffer.Length < totalBytes)
			throw new InvalidOperationException();

		var data = buffer.Slice(headerBytes, dataBytes);

		var action = new TAction();

		for (int yy = 0; yy < h; yy++)
		{
			int o = (y + yy) * 320 + x;
			int p = yy * bytesPerScan;

			var planeScan = plane.Slice(o);
			var dataScan = data.Slice(p);

			for (int xx = 0; xx < w; xx++)
				planeScan[xx] = action.ApplySpriteBits(planeScan[xx], dataScan[xx], 0, 0xFF);
		}
	}

	public override void ScrollUp(int scanCount, int windowStart, int windowEnd)
	{
		int windowOffset = windowStart * Width;
		int windowLength = (windowEnd - windowStart + 1) * Width;

		int copyOffset = scanCount * Width;
		int copySize = windowLength - copyOffset;

		var vram = Array.VRAM.AsSpan().Slice(StartAddress + windowOffset, windowLength);

		vram.Slice(copyOffset).CopyTo(vram);
		vram.Slice(copySize).Fill(0);
	}

	protected override void DrawCharacterScan(int x, int y, int characterWidth, byte glyphScan)
	{
		int o = y * Width + x;

		int characterBit = 128;

		var vram = Array.VRAM.AsSpan().Slice(StartAddress);

		if (x + characterWidth > Width)
			characterWidth = Width - x;

		byte colour = unchecked((byte)DrawingAttribute);

		for (int xx = 0; xx < characterWidth; xx++)
		{
			if ((glyphScan & characterBit) != 0)
				vram[o] = colour;
			else
				vram[o] = 0;

			o++;
			characterBit >>= 1;
		}
	}
}
