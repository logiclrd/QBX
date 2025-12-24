using SDL3;
using System;
using System.Runtime.InteropServices;

namespace QBX.Hardware;

public unsafe class Adapter
{
	GraphicsArray _array;
	int _width, _height;

	public Adapter(GraphicsArray array)
	{
		_array = array;
	}

	public bool UpdateResolution(ref int width, ref int height)
	{
		_width = _array.Sequencer.CharacterWidth * _array.CRTController.NumColumns;
		_height = _array.CRTController.NumScanLines;

		if ((width != _width) || (height != _height))
		{
			(width, height) = (_width, _height);
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
			var textureBuffer = new Span<byte>((void *)texture, _height * pitch);
			int rowWidth = _width * 4;

			byte[] vram = _array.VRAM;
			int vramSize = _array.VRAM.Length;

			var palette = _array.DAC.Palette;
			int fontStartA = _array.Sequencer.CharacterSetAOffset;
			int fontStartB = _array.Sequencer.CharacterSetBOffset;

			int address = _array.CRTController.StartAddress;
			int stride = _array.CRTController.Stride;

			bool graphicsMode = _array.Graphics.DisableText;
			bool lineGraphics = _array.AttributeController.LineGraphics;
			bool use256Colours = _array.AttributeController.Use256Colours;

			int characterWidth = graphicsMode ? 1 : _array.Sequencer.CharacterWidth;
			int characterHeight = _array.CRTController.ScanRepeatCount;

			bool dotDoubling = _array.Sequencer.DotDoubling;
			bool scanDoubling = _array.CRTController.ScanDoubling;

			int characterY = 0;

			const byte Zero = (byte)0;

			for (int y = 0; y < _width; y++)
			{
				var scan = textureBuffer.Slice(y * pitch, rowWidth);

				int characterX = 0;
				int characterBit = 128;

				for (int x = 0; x < _width; x++)
				{
					bool inRange = (address >= 0) && (address + 1 < vramSize);

					byte ch = inRange ? vram[address] : Zero;

					if ((ch >= 0xC0) && (ch <= 0xDF) && (characterBit == 0))
						characterBit = 1;

					int colourIndex;

					if (graphicsMode)
					{
						if (use256Colours)
							colourIndex = ch;
						else
						{
							// TODO: extract thePixelValue from the byte

							byte attr = thePixelValue;

							colourIndex = _array.AttributeController.Registers.Attribute[attr]
								| (_array.AttributeController.Registers.ColourSelect << 4); // TODO: when do these apply
						}
					}
					else
					{
						byte attrByte = inRange ? _array.VRAM[address + 1] : Zero;

						int fontOffset = (attrByte & 4) == 0
							? fontStartA
							: fontStartB;

						byte fontByte = vram[2 * 65536 + fontOffset + ch * 32 + characterY];

						bool fontPixelIsSet = (fontByte & characterBit) != 0;

						byte attr = fontPixelIsSet
							? unchecked((byte)(attrByte & 15)) // foreground
							: unchecked((byte)(attrByte >> 4)); // background

						colourIndex = _array.AttributeController.Registers.Attribute[attr]
							| (_array.AttributeController.Registers.ColourSelect << 4); // TODO: when do these apply
					}

					int colour = palette[colourIndex];

					if (dotDoubling)
						characterX += (x & 1);
					else
						characterX++;

					characterBit >>= 1;

					if (characterX >= characterWidth)
					{
						characterX = 0;
						characterBit = 128;
						address += readSize;
					}
				}

				if (scanDoubling)
					characterY += (y & 1);
				else
					characterY++;

				if (characterY >= characterHeight)
				{
					characterY = 0;
					address += stride;
				}
			}

			switch (_array.VideoMode)
			{
				case VideoMode.TextMode:
					//RenderTextMode();
					break;
				case VideoMode.CGA_320x200_2bpp:
					RenderCGA2bpp(pixelsPtr, pitch);
					break;
				case VideoMode.CGA_640x200_1bpp:
					RenderCGA1bpp(pixelsPtr, pitch);
					break;
				case VideoMode.VGA_640x480_1bpp:
				case VideoMode.EGA_640x350_2bpp_Monochrome:
				case VideoMode.EGA_320x200_4bpp:
				case VideoMode.EGA_640x200_4bpp:
				case VideoMode.EGA_640x350_4bpp:
				case VideoMode.VGA_640x480_4bpp:
					//RenderPlanarGraphics4bpp();
					break;
				case VideoMode.VGA_320x200_8bpp:
					//RenderContiguousGraphics8bpp();
					break;
			}
		}
		catch { }
		finally
		{
			SDL.UnlockTexture(texture);
		}
	}
}
