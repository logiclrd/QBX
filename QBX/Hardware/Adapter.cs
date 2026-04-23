using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

using SDL3;

namespace QBX.Hardware;

public class Adapter
{
	GraphicsArray _array;
	int _width, _height;
	int _physicalWidth, _physicalHeight;

	readonly long Epoch = DateTime.UtcNow.Ticks;

	long ElapsedTicks => DateTime.UtcNow.Ticks - Epoch;

	const long TicksPerCursorSwitch = TimeSpan.TicksPerSecond * 16 / 70;
	const long TicksPerBlinkSwitch = TimeSpan.TicksPerSecond * 32 / 70;

	ManualResetEvent _scanStart = new ManualResetEvent(initialState: false);
	ManualResetEvent _scanEnd = new ManualResetEvent(initialState: false);

	public Adapter(GraphicsArray array)
	{
		_array = array;
	}

	public bool UpdateResolution(ref int width, ref int height, ref int physicalWidth, ref int physicalHeight)
	{
		int widthScale = _array.Sequencer.DotDoubling ? 2 : 1;
		int heightScale = _array.CRTController.ScanDoubling ? 2 : 1;

		_width = _array.MiscellaneousOutput.BasePixelWidth >> (_array.Sequencer.DotDoubling ? 1 : 0);
		_height = _array.CRTController.NumScanLines * heightScale;

		_physicalWidth = _width * widthScale;
		_physicalHeight = _height;

		if (_height == 350)
			_physicalHeight = _physicalHeight * 480 / 350;

		if (_array.Graphics.DisableText)
		{
			int repeatCount = _array.CRTController.ScanRepeatCount + 1;

			_height /= repeatCount;
		}

		if ((width != _width) || (height != _height) || (physicalWidth != _physicalWidth) || (physicalHeight != _physicalHeight))
		{
			(width, height, physicalWidth, physicalHeight) = (_width, _height, _physicalWidth, _physicalHeight);
			return true;
		}

		return false;
	}

	public unsafe void Render(IntPtr texture)
	{
		_array.EndVerticalRetrace();

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

			_array.BeginVerticalRetrace();
		}
	}

	public void VerticalSync()
	{
		_scanStart.WaitOne();
		_scanEnd.WaitOne();
	}

	public void Render(Span<byte> target, int targetPitch)
	{
		try
		{
			_scanEnd.Reset();
			_scanStart.Set();

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

			bool promoteBit0ToBit13 = _array.CRTController.InterleaveOnBit0;
			bool promoteBit1ToBit14 = _array.CRTController.InterleaveOnBit1;

			bool enableText = !_array.Graphics.DisableText;

			byte dacMask = _array.DAC.Mask;
			var palette = MemoryMarshal.Cast<byte, int>(_array.DAC.PaletteBGRA);
			int fontStartA = 0x20000 + _array.Sequencer.CharacterSetAOffset;
			int fontStartB = 0x20000 + _array.Sequencer.CharacterSetBOffset;

			bool graphicsMode = _array.Graphics.DisableText;
			bool lineGraphics = _array.AttributeController.LineGraphics;
			bool use256Colours = _array.AttributeController.Use256Colours;
			bool shift256 = _array.Graphics.Shift256;
			bool chainOddEven = _array.Graphics.ChainOddEven;

			int plane0VisibleSize = use256Colours ? vram.Length : planeSize;
			int plane1VisibleSize = use256Colours ? plane0VisibleSize - planeSize : planeSize;
			int plane2VisibleSize = use256Colours ? plane1VisibleSize - planeSize : planeSize;
			int plane3VisibleSize = use256Colours ? plane2VisibleSize - planeSize : planeSize;

			var plane0 = vram.AsSpan().Slice(startAddress + 0 * planeSize, plane0VisibleSize - startAddress);
			var plane1 = vram.AsSpan().Slice(startAddress + 1 * planeSize, plane1VisibleSize - startAddress);
			var plane2 = vram.AsSpan().Slice(startAddress + 2 * planeSize, plane2VisibleSize - startAddress);
			var plane3 = vram.AsSpan().Slice(startAddress + 3 * planeSize, plane3VisibleSize - startAddress);

			long tick = ElapsedTicks;

			bool enableCursor = _array.CRTController.CursorVisible;
			bool cursorState = false;
			int cursorOffset = _array.CRTController.CursorAddress;

			if (chainOddEven)
				cursorOffset *= 2;

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
			int stride = _array.CRTController.Stride;

			bool scanDoubling = _array.CRTController.ScanDoubling;
			bool dotDoubling = _array.Sequencer.DotDoubling;

			int activeScans = _array.CRTController.NumScanLines * (scanDoubling ? 2 : 1);

			if (_array.Graphics.DisableText)
			{
				int repeatCount = _array.CRTController.ScanRepeatCount + 1;

				activeScans /= repeatCount;
				characterHeight /= repeatCount;

				if (_array.Graphics.Shift256)
					stride *= 2;
			}

			int dotsPerLoopIteration = dotDoubling ? 2 : 1;

			bool paletteAddressSource = _array.AttributeController.PaletteAddressSource;

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

			int planeOffset = _array.CRTController.SkipScans * stride + _array.CRTController.SkipBytes;

			int resetAddressAtScan = _array.CRTController.ResetAddressAtScan;

			int endHorizontalDisplay = _array.CRTController.Registers.EndHorizontalDisplay + 1;

			if (chainOddEven)
				endHorizontalDisplay *= 2;
			else if (shift256)
				endHorizontalDisplay *= 8;

			int overscanBGRA = palette[
				_array.AttributeController.Registers.OverscanPaletteIndex];

			Span<byte> wrapSpan0 = stackalloc byte[stride];
			Span<byte> wrapSpan1 = stackalloc byte[stride];
			Span<byte> wrapSpan2 = stackalloc byte[stride];
			Span<byte> wrapSpan3 = stackalloc byte[stride];

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			static Span<byte> BuildWrapSpan(Span<byte> plane, int planeOffset, int stride, Span<byte> wrapSpan)
			{
				while (planeOffset >= plane.Length)
					planeOffset -= plane.Length;

				int bytesBeforeWrap = plane.Length - planeOffset;

				if (bytesBeforeWrap >= stride)
					return plane.Slice(planeOffset, stride);

				int bytesAfterWrap = stride - bytesBeforeWrap;

				plane.Slice(planeOffset, bytesBeforeWrap).CopyTo(wrapSpan);
				plane.Slice(0, bytesAfterWrap).CopyTo(wrapSpan.Slice(bytesBeforeWrap));

				return wrapSpan;
			}

			int y = 0;
			bool scanDoubled = false;

			for (int scan = 0; scan < activeScans; scan++)
			{
				if (y == resetAddressAtScan)
					planeOffset = 0;

				var scanIn0 = BuildWrapSpan(plane0, planeOffset, stride, wrapSpan0);
				var scanIn1 = BuildWrapSpan(plane1, planeOffset, stride, wrapSpan1);
				var scanIn2 = BuildWrapSpan(plane2, planeOffset, stride, wrapSpan2);
				var scanIn3 = BuildWrapSpan(plane3, planeOffset, stride, wrapSpan3);
				var scanOut = MemoryMarshal.Cast<byte, int>(target.Slice(scan * targetPitch, rowWidthOut));

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

					if (!paletteAddressSource)
						paletteIndex = 0;

					scanOut[x] = palette[paletteIndex & dacMask];

					if (shift256)
						characterX += 2;
					else
						characterX++;

					columnBit >>= 1;

					if (shift256)
						offset++;

					if (characterX >= characterWidth)
					{
						characterX = 0;
						columnBit = 128;

						if ((offset == endHorizontalDisplay) || (offset >= scanIn0.Length))
						{
							x++;

							int remainingPixels = scanOut.Length - x;

							if (remainingPixels > 0)
								scanOut.Slice(x).Fill(overscanBGRA);

							break;
						}

						if (!shift256)
						{
							if (chainOddEven)
								offset += 2;
							else
								offset++;
						}
					}
				}

				if (scanDoubled != scanDoubling)
					scanDoubled = true;
				else
				{
					scanDoubled = false;

					y++;
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

			int remainingScans = _height - activeScans;

			if (remainingScans > 0)
				target.Slice(activeScans * targetPitch, remainingScans * targetPitch).Clear();
		}
		catch { }
		finally
		{
			_scanEnd.Set();
			_scanStart.Reset();
		}
	}
}
