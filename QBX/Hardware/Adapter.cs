using QBX.Parser;

using SDL3;

using System.Runtime.InteropServices;

namespace QBX.Hardware;

public unsafe class Adapter
{
	GraphicsArray _array;
	int _width, _height;
	int _widthScale, _heightScale;

	readonly long Epoch = DateTime.UtcNow.Ticks;

	long ElapsedTicks => DateTime.UtcNow.Ticks - Epoch;

	const long TicksPerCursorSwitch = TimeSpan.TicksPerSecond * 16 / 60;
	const long TicksPerBlinkSwitch = TimeSpan.TicksPerSecond * 32 / 60;

	public Adapter(GraphicsArray array)
	{
		_array = array;
	}

	public bool UpdateResolution(ref int width, ref int height, ref int widthScale, ref int heightScale)
	{
		_width = _array.MiscellaneousOutput.BasePixelWidth >> (_array.Sequencer.DotDoubling ? 1 : 0);
		_height = _array.CRTController.NumScanLines;
		_widthScale = _array.Sequencer.DotDoubling ? 2 : 1; ;
		_heightScale = _array.CRTController.ScanDoubling ? 2 : 1;;

		if ((width != _width) || (height != _height) || (widthScale != _widthScale) || (heightScale != _heightScale))
		{
			(width, height, widthScale, heightScale) = (_width, _height, _widthScale, _heightScale);
			return true;
		}

		return false;
	}

	public unsafe void Render(IntPtr texture)
	{
		if (!SDL.LockTexture(texture, default, out var pixelsPtr, out var pitch))
			return;

		try
		{
			var textureBuffer = new Span<byte>((void *)pixelsPtr, _height * pitch);
			int rowWidthOut = _width * 4;

			int bitsPerPixelPerPlane =
				_array.Graphics.Shift256
				? 8
				: _array.Graphics.ShiftInterleave
				? 2
				: 1; // 16-colour modes are still 1 bpp _per plane_

			bool shiftInterleave = _array.Graphics.ShiftInterleave;

			int pixelBitsReset = unchecked((byte)(0b1111111100000000 >> bitsPerPixelPerPlane));
			int pixelShiftReset = 8 - bitsPerPixelPerPlane;

			byte[] vram = _array.VRAM;

			var plane0 = vram.AsSpan().Slice(0x00000, 0x10000);
			var plane1 = vram.AsSpan().Slice(0x10000, 0x10000);
			var plane2 = vram.AsSpan().Slice(0x20000, 0x10000);
			var plane3 = vram.AsSpan().Slice(0x30000, 0x10000);

			bool promoteBit0ToBit13 = _array.CRTController.InterleaveOnBit0;
			bool promoteBit1ToBit14 = _array.CRTController.InterleaveOnBit1;

			bool enableText = !_array.Graphics.DisableText;
			bool oddEvenMode = _array.Sequencer.OddEvenAddressingMode;

			var palette = MemoryMarshal.Cast<byte, int>(_array.DAC.PaletteBGRA);
			int fontStartA = 0x20000 + _array.Sequencer.CharacterSetAOffset;
			int fontStartB = 0x20000 + _array.Sequencer.CharacterSetBOffset;

			int baseAddress = _array.CRTController.StartAddress;

			// In theory this value is derived from the CRT Controller's Offset register.
			int stride = _width /
				(_array.Graphics.Shift256 ? 1
				: _array.Graphics.ShiftInterleave ? 4
				: 8);

			bool graphicsMode = _array.Graphics.DisableText;
			bool lineGraphics = _array.AttributeController.LineGraphics;
			bool use256Colours = _array.AttributeController.Use256Colours;

			int advance = graphicsMode ? 1 : 2;

			long tick = ElapsedTicks;

			bool enableCursor = _array.CRTController.CursorVisible;
			bool cursorState = false;
			int cursorOffset = _array.CRTController.CursorAddress;

			if (enableCursor)
				cursorState = ((tick / TicksPerCursorSwitch) & 1) != 0;

			bool enableBlink = _array.AttributeController.EnableBlinking;
			bool blinkState = false;

			if (enableBlink)
				blinkState = ((tick / TicksPerBlinkSwitch) & 1) != 0;

			int characterWidth = graphicsMode ? 1 : _array.Sequencer.CharacterWidth;
			int characterHeight = _array.CRTController.ScanRepeatCount + 1;

			int attributeBits76 = _array.AttributeController.AttributeBits76;
			int attributeBits54 = _array.AttributeController.AttributeBits54;
			bool overrideAttributeBits54 = _array.AttributeController.OverrideAttributeBits54;

			int cursorScanStart = _array.CRTController.CursorScanStart;
			int cursorScanEnd = _array.CRTController.CursorScanEnd;

			int characterY = 0;
			bool inCursorScan = (cursorScanStart == 0);

			int pixelBits = pixelBitsReset;
			int pixelShift = pixelShiftReset;

			int overscanColour = _array.AttributeController.Registers.OverscanPaletteIndex;

			for (int y = 0; y < _height; y++)
			{
				var scanIn0 = plane0.Slice(y * stride, stride);
				var scanIn1 = plane1.Slice(y * stride, stride);
				var scanIn2 = plane2.Slice(y * stride, stride);
				var scanIn3 = plane3.Slice(y * stride, stride);
				var scanOut = MemoryMarshal.Cast<byte, int>(textureBuffer.Slice(y * pitch, rowWidthOut));

				int offset = 0;
				int characterX = 0;
				int columnBit = 128;

				for (int x = 0; x < _width; x++)
				{
					int effectiveOffset = offset;

					if (promoteBit0ToBit13)
					{
						effectiveOffset = (effectiveOffset >> 1) + ((effectiveOffset & 1) << 13);

						if (promoteBit1ToBit14)
							effectiveOffset = (effectiveOffset >> 1) + ((effectiveOffset & 1) << 13);
					}
					else if (promoteBit1ToBit14)
					{
						// ?
					}

					int address = baseAddress + effectiveOffset;

					bool inRange = (effectiveOffset >= 0) && (effectiveOffset + 1 < vram.Length);

					int attribute;

					if (!inRange)
						attribute = overscanColour;
					else if (graphicsMode)
					{
						if (shiftInterleave)
						{
							byte packedPixels;

							switch (y & 1)
							{
								case 0:
									switch (x & 1)
									{
										case 0: packedPixels = scanIn0[effectiveOffset]; break;
										case 1: packedPixels = scanIn2[effectiveOffset]; break;
										default: throw new Exception("Sanity failure");
									}

									break;
								case 1:
									switch (x & 1)
									{
										case 0: packedPixels = scanIn1[effectiveOffset]; break;
										case 1: packedPixels = scanIn3[effectiveOffset]; break;
										default: throw new Exception("Sanity failure");
									}

									break;
								default: throw new Exception("Sanity failure");
							}

							attribute = (packedPixels & pixelBits) >> pixelShift;
						}
						else if (use256Colours)
							attribute = scanIn0[address];
						else
						{
							attribute =
								(((scanIn0[effectiveOffset] & columnBit) != 0) ? 8 : 0) |
								(((scanIn1[effectiveOffset] & columnBit) != 0) ? 4 : 0) |
								(((scanIn2[effectiveOffset] & columnBit) != 0) ? 2 : 0) |
								(((scanIn3[effectiveOffset] & columnBit) != 0) ? 1 : 0);
						}

						if (shiftInterleave && ((x & 1) == 0))
						{
							pixelBits >>= bitsPerPixelPerPlane;
							pixelShift -= bitsPerPixelPerPlane;

							if (pixelBits == 0)
							{
								pixelBits = pixelBitsReset;
								pixelShift = pixelShiftReset;
							}
						}
					}
					else
					{
						byte ch = vram[address];
						byte attr = vram[address + 1];

						int fontStart = ((attr & 8) == 1)
							? fontStartA
							: fontStartB;

						byte fontByte = (enableCursor && cursorState && (offset == cursorOffset) && inCursorScan)
							? unchecked((byte)0b11111111)
							: vram[fontStart + 32 * ch + characterY];

						if (lineGraphics && (ch >= 0xC0) && (ch <= 0xDF) && (columnBit == 0))
							columnBit = 1;

						int effectiveForegroundColour = attr & 15;
						int backgroundColour = attr >> 4;

						if (enableBlink & ((attr & 8) != 0))
						{
							effectiveForegroundColour = backgroundColour;
							attr = unchecked((byte)(attr & ~8));
						}

						attribute = ((fontByte & columnBit) != 0)
							? effectiveForegroundColour
							: backgroundColour;
					}

					int paletteIndex;

					if (use256Colours)
						paletteIndex = attribute;
					else
					{
						attribute |= attributeBits76;

						if (overrideAttributeBits54)
							attribute = unchecked((byte)((attribute & ~48) | attributeBits54));

						paletteIndex = _array.AttributeController.Registers.Attribute[attribute];
					}

					scanOut[x] = palette[paletteIndex];

					characterX++;

					columnBit >>= 1;

					if (characterX >= characterWidth)
					{
						characterX = 0;
						columnBit = 128;
						offset += advance;
					}
				}

				characterY++;

				inCursorScan = ((characterY >= cursorScanStart) && (characterY <= cursorScanEnd));

				if (characterY >= characterHeight)
				{
					characterY = 0;
					offset += stride;
				}
			}
		}
		catch { }
		finally
		{
			SDL.UnlockTexture(texture);
		}
	}
}
