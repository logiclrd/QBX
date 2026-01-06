using System;
using System.Runtime.InteropServices;

using SDL3;

namespace QBX.Hardware;

public class Adapter
{
	GraphicsArray _array;
	int _width, _height;
	int _widthScale, _heightScale;

	readonly long Epoch = DateTime.UtcNow.Ticks;

	long ElapsedTicks => DateTime.UtcNow.Ticks - Epoch;

	const long TicksPerCursorSwitch = TimeSpan.TicksPerSecond * 8 / 70;
	const long TicksPerBlinkSwitch = TimeSpan.TicksPerSecond * 16 / 70;

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
			var textureBuffer = new Span<byte>((void*)pixelsPtr, _height * pitch);

			Render(textureBuffer, pitch);
		}
		finally
		{
			SDL.UnlockTexture(texture);
		}
	}

	public void Render(Span<byte> target, int targetPitch)
	{
		try
		{
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

			int planeSize =
				_array.CRTController.InterleaveOnBit0 ? 8192
				: _array.CRTController.InterleaveOnBit1 ? 16384
				: 65536;

			int startAddress = _array.CRTController.StartAddress;

			var plane0 = vram.AsSpan().Slice(startAddress + 0 * planeSize, planeSize);
			var plane1 = vram.AsSpan().Slice(startAddress + 1 * planeSize, planeSize);
			var plane2 = vram.AsSpan().Slice(startAddress + 2 * planeSize, planeSize);
			var plane3 = vram.AsSpan().Slice(startAddress + 3 * planeSize, planeSize);

			bool promoteBit0ToBit13 = _array.CRTController.InterleaveOnBit0;
			bool promoteBit1ToBit14 = _array.CRTController.InterleaveOnBit1;

			bool enableText = !_array.Graphics.DisableText;

			var palette = MemoryMarshal.Cast<byte, int>(_array.DAC.PaletteBGRA);
			int fontStartA = 0x20000 + _array.Sequencer.CharacterSetAOffset;
			int fontStartB = 0x20000 + _array.Sequencer.CharacterSetBOffset;

			bool graphicsMode = _array.Graphics.DisableText;
			bool lineGraphics = _array.AttributeController.LineGraphics;
			bool use256Colours = _array.AttributeController.Use256Colours;
			bool shift256 = _array.Graphics.Shift256;

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

			// How many columns between each change in offset?
			int characterWidth = _array.Sequencer.CharacterWidth;
			int characterHeight = _array.CRTController.ScanRepeatCount + 1;

			if (shiftInterleave)
				characterHeight *= 2;

			// In theory this value is derived from the CRT Controller's Offset register.
			int stride = _width / (_array.Graphics.Shift256 ? 1 : characterWidth);

			int attributeBits76 = _array.AttributeController.AttributeBits76;
			int attributeBits54 = _array.AttributeController.AttributeBits54;
			bool overrideAttributeBits54 = _array.AttributeController.OverrideAttributeBits54;

			int cursorScanStart = _array.CRTController.CursorScanStart;
			int cursorScanEnd = _array.CRTController.CursorScanEnd;

			int characterY = 0;
			bool inCursorScan;

			int pixelBits = pixelBitsReset;
			int pixelShift = pixelShiftReset;

			int overscanColour = _array.AttributeController.Registers.OverscanPaletteIndex;

			int planeOffset = 0;

			int endHorizontalDisplay = _array.CRTController.Registers.EndHorizontalDisplay;

			int overscanBGRA = palette[
				_array.AttributeController.Registers.OverscanPaletteIndex];

			for (int y = 0; y < _height; y++)
			{
				var scanIn0 = plane0.Slice(planeOffset, stride);
				var scanIn1 = plane1.Slice(planeOffset, stride);
				var scanIn2 = plane2.Slice(planeOffset, stride);
				var scanIn3 = plane3.Slice(planeOffset, stride);
				var scanOut = MemoryMarshal.Cast<byte, int>(target.Slice(y * targetPitch, rowWidthOut));

				int offset = 0;
				int characterX = 0;
				int columnBit = 128;

				inCursorScan = (characterY >= cursorScanStart) && (characterY <= cursorScanEnd);

				for (int x = 0; x < _width; x++)
				{
					bool inRange = (offset >= 0) && (offset + 1 < planeSize);

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
										case 0: packedPixels = scanIn0[offset]; break;
										case 1: packedPixels = scanIn2[offset]; break;
										default: throw new Exception("Sanity failure");
									}

									break;
								case 1:
									switch (x & 1)
									{
										case 0: packedPixels = scanIn1[offset]; break;
										case 1: packedPixels = scanIn3[offset]; break;
										default: throw new Exception("Sanity failure");
									}

									break;
								default: throw new Exception("Sanity failure");
							}

							attribute = (packedPixels & pixelBits) >> pixelShift;
						}
						else if (use256Colours)
							attribute = scanIn0[offset];
						else
						{
							attribute =
								(((scanIn0[offset] & columnBit) != 0) ? 1 : 0) |
								(((scanIn1[offset] & columnBit) != 0) ? 2 : 0) |
								(((scanIn2[offset] & columnBit) != 0) ? 4 : 0) |
								(((scanIn3[offset] & columnBit) != 0) ? 8 : 0);
						}

						if (shiftInterleave && ((x & 1) == 1))
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
						byte ch = scanIn0[offset];
						byte attr = scanIn1[offset];

						int fontStart = ((attr & 8) == 1)
							? fontStartA
							: fontStartB;

						bool blink = false;

						if (enableBlink)
						{
							blink = ((attr & 0b1000_0000) != 0);
							attr &= 0b0111_1111;
						}

						byte fontByte = (enableCursor && cursorState && (offset == cursorOffset) && inCursorScan)
							? unchecked((byte)0b11111111)
							: (enableBlink && blink && blinkState)
							? unchecked((byte)0b00000000)
							: vram[fontStart + 32 * ch + characterY];

						if (lineGraphics && (ch >= 0xC0) && (ch <= 0xDF) && (columnBit == 0))
							columnBit = 1;

						if ((fontByte & columnBit) != 0)
							attribute = attr & 15;
						else
							attribute = attr >> 4;
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

					if (shift256)
						offset++;

					if (characterX >= characterWidth)
					{
						characterX = 0;
						columnBit = 128;

						if (offset == endHorizontalDisplay)
						{
							x++;

							int remainingPixels = scanOut.Length - x;

							if (remainingPixels > 0)
								scanOut.Slice(x).Fill(overscanBGRA);

							break;
						}

						if (!shift256)
							offset++;
					}
				}

				characterY++;

				inCursorScan = ((characterY >= cursorScanStart) && (characterY <= cursorScanEnd));

				if (characterY >= characterHeight)
				{
					characterY = 0;
					planeOffset += stride;
					cursorOffset -= stride;
				}
			}
		}
		catch { }
	}
}
