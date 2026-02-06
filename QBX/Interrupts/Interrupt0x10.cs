using System;
using System.IO;
using QBX.CodeModel.Statements;
using QBX.Firmware;
using QBX.Hardware;

using static QBX.Hardware.GraphicsArray;

namespace QBX.Interrupts;

// Based on Ralf Brown's Interrupt List: https://www.ctyme.com/intr/int-10.htm
//
// Sigh. Ralf, why are your names for functions so inconsistent? :-)

public class Interrupt0x10(Machine machine) : InterruptHandler
{
	public enum Function : byte
	{
		SetVideoMode                               = 0x00,
		SetTextModeCursorShape                     = 0x01,
		SetCursorPosition                          = 0x02,
		GetCursorPositionAndSize                   = 0x03,
		ReadLightPenPosition                       = 0x04,
		SelectActiveDisplayPage                    = 0x05,
		ScrollUpWindow                             = 0x06,
		ScrollDownWindow                           = 0x07,
		ReadCharacterAndAttributeAtCursorPosition  = 0x08,
		WriteCharacterAndAttributeAtCursorPosition = 0x09,
		WriteCharacterOnlyAtCursorPosition         = 0x0A,
		SetBackgroundAndBorderColour               = 0x0B,
		WriteGraphicsPixel                         = 0x0C,
		ReadGraphicsPixel                          = 0x0D,
		TeletypeOutput                             = 0x0E,
		GetCurrentVideoMode                        = 0x0F,
		ColourFunction                             = 0x10,
		FontFunction                               = 0x11,
		AlternateFunction                          = 0x12,
		WriteString                                = 0x13,
		DisplayCombinationCode                     = 0x1A,
		FunctionalityStateInformation              = 0x1B, // not supported
		SaveRestoreVideoState                      = 0x1C,
	}

	enum ColourFunction : byte
	{
		SetSinglePaletteRegister                   = 0x00,
		SetOverscanColour                          = 0x01,
		SetAllPaletteRegisters                     = 0x02,
		ToggleIntensityBlinkingBit                 = 0x03,
		GetIndividualPaletteRegister               = 0x07,
		ReadOverscanRegister                       = 0x08,
		ReadAllPaletteRegistersAndOverscanRegister = 0x09,
		SetIndividualDACRegister                   = 0x10,
		SetBlockOfDACRegisters                     = 0x12,
		SelectVideoDACColourPage                   = 0x13,
		ReadIndividualDACRegister                  = 0x15,
		ReadBlockOfDACRegisters                    = 0x17,
		SetPixelMask                               = 0x18,
		ReadPixelMask                              = 0x19,
		GetVideoDACColourPageState                 = 0x1A,
		PerformGrayscaleSumming                    = 0x1B,
	}

	enum DACColourPageSubfunction : byte
	{
		SelectPagingMode = 0x00,
		SelectPage       = 0x01,
	}

	enum FontFunction : byte
	{
		LoadUserSpecifiedPatterns   = 0x00,
		LoadROMMonochromePatterns   = 0x01, // Ralf Brown says this just means the 8x14 font
		LoadROM8x8DoubleDotPatterns = 0x02,
		SetBlockSpecifier           = 0x03,
		LoadROM8x16CharacterSet     = 0x04,

		AndUpdateMode = 0x10,

		LoadUserSpecifiedPatternsAndUpdateMode = LoadUserSpecifiedPatterns + AndUpdateMode,
		LoadROMMonochromePatternsAndUpdateMode = LoadROMMonochromePatterns + AndUpdateMode,
		LoadROM8x8DoubleDotPatternsAndUpdateMode = LoadROM8x8DoubleDotPatterns + AndUpdateMode,
		LoadROM8x16CharacterSetAndUpdateMode = LoadROM8x16CharacterSet + AndUpdateMode,

		SetUser8x8GraphicsChars     = 0x20,
		SetUserGraphicsCharacters   = 0x21,
		SetROM8x14GraphicsChars     = 0x22,
		SetROM8x8DoubleDotChars     = 0x23,
		Load8x16GraphicsChars       = 0x24,
		GetFontInformation          = 0x30,
	}

	public enum AlternateFunction : byte
	{
		GetEGAInfo               = 0x10,
		AlternatePrintScreen     = 0x20,
		SelectVerticalResolution = 0x30, // for text modes
		PaletteLoading           = 0x31,
		VideoAddressing          = 0x32,
		GrayscaleSumming         = 0x33,
		CursorEmulation          = 0x34,
		DisplaySwitchInterface   = 0x35,
		VideoRefreshControl      = 0x36,
	}

	public enum DisplayCombinationCodeFunction : byte
	{
		Get = 0x00,
		Set = 0x01,
	}

	public enum StateFunction : byte
	{
		ReturnStateBufferSize = 0x00,
		SaveVideoState        = 0x01,
		RestoreVideoState     = 0x02,
	}

	bool _enableGrayscaleSumming = false;
	ushort _displayCombinationCodes = 0x0008; // VGA with colour display, no secondary

	readonly int StateBufferSize = machine.VideoFirmware.GetStateBufferLength();

	public override Registers Execute(Registers input)
	{
		var firmware = machine.VideoFirmware;
		var visual = firmware.VisualLibrary;

		byte ah = unchecked((byte)(input.AX >> 8));
		byte al = unchecked((byte)input.AX);

		byte bh = unchecked((byte)(input.BX >> 8));
		byte bl = unchecked((byte)input.BX);

		var function = (Function)ah;

		var result = input.AsRegistersEx();

		switch (function)
		{
			case Function.SetVideoMode:
			{
				if (firmware.SetMode(al))
				{
					result.AX &= 0xFF00;

					if (al > 7)
						result.AX |= 0x20;
					else if (al == 6)
						result.AX |= 0x3F;
					else
						result.AX |= 0x30;
				}

				break;
			}
			case Function.SetTextModeCursorShape:
			{
				int cursorStart = result.CX >> 8; // CH
				int cursorEnd = result.CX & 0xFF; // CL

				bool cursorVisible = (cursorStart & 96) == 0;

				cursorStart &= 31;

				firmware.SetCursorScans(cursorStart, cursorEnd, cursorVisible);

				break;
			}
			case Function.SetCursorPosition:
			{
				int pageNumber = result.BX >> 8; // BH;

				if (!firmware.IsTextMode
				 && (pageNumber > 0))
					break;

				int row = result.DX >> 8; // DH
				int column = result.DX & 0xFF; // DL

				firmware.MoveCursor(column, row, pageNumber);

				break;
			}
			case Function.GetCursorPositionAndSize:
			{
				int pageNumber = result.BX >> 8; // BH;

				if (!firmware.IsTextMode
				 && (pageNumber > 0))
					break;

				(int startScan, int endScan) = firmware.GetCursorScans();
				(int cursorX, int cursorY) = firmware.GetCursorPosition();

				result.CX = unchecked((ushort)((startScan << 8) | endScan));
				result.DX = unchecked((ushort)((cursorY << 8) | cursorX));

				break;
			}
			case Function.ReadLightPenPosition:
			{
				result.AX = machine.MouseDriver.LightPenIsDown ? (ushort)0x100 : (ushort)0x000;

				var lightPenPosition = machine.MouseDriver.LightPenEndPosition;

				int lightPenCharacterX = lightPenPosition.X / 8;
				int lightPenCharacterY = lightPenPosition.Y / 8;

				result.BX = unchecked((ushort)lightPenPosition.X);

				if (machine.MouseDriver.PointerMaximumY < 256)
					result.CX = unchecked((ushort)(lightPenPosition.Y << 8)); // weirdos using only CH instead of only CL
				else
					result.CX = unchecked((ushort)lightPenPosition.Y);

				result.DX = unchecked((ushort)((lightPenCharacterY << 8) | lightPenCharacterX));

				break;
			}
			case Function.SelectActiveDisplayPage:
			{
				visual.SetActivePage(al);

				break;
			}
			case Function.ScrollUpWindow:
			case Function.ScrollDownWindow:
			{
				int numLines = al;

				if (function == Function.ScrollUpWindow)
					numLines = -numLines;

				if (numLines == 0)
					numLines = visual.CharacterHeight;

				byte fillAttribute = unchecked((byte)(input.BX >> 8));

				int x1 = input.CX & 0xFF;
				int y1 = input.CX >> 8;

				int x2 = input.DX & 0xFF;
				int y2 = input.DX >> 8;

				visual.ScrollTextWindow(x1, y1, x2, y2, numLines, fillAttribute);

				break;
			}
			case Function.ReadCharacterAndAttributeAtCursorPosition:
			{
				int savedPage = visual.ActivePageNumber;

				try
				{
					visual.SetActivePage(bh);

					int x = visual.CursorX;
					int y = visual.CursorY;

					byte attribute = visual.GetAttribute(x, y);
					byte character = visual.GetCharacter(x, y);

					result.AX = unchecked((byte)((attribute << 8) | character));
				}
				finally
				{
					visual.SetActivePage(savedPage);
				}

				break;
			}
			case Function.WriteCharacterAndAttributeAtCursorPosition:
			{
				var textLibrary = visual as TextLibrary;
				var graphicsLibrary = visual as GraphicsLibrary;

				int savedPage = visual.ActivePageNumber;
				bool savedControlCharactersFlag = visual.ProcessControlCharacters;
				byte savedAttributeByte = visual.CurrentAttributeByte;

				try
				{
					visual.SetActivePage(bh);

					int x = visual.CursorX;
					int y = visual.CursorY;

					byte attribute = unchecked((byte)(input.BX & 0xFF));
					byte character = al;

					visual.CurrentAttributeByte = attribute;

					for (int i = 0; i < input.CX; i++)
						visual.WriteText(character);
				}
				finally
				{
					visual.CurrentAttributeByte = savedAttributeByte;
					visual.ProcessControlCharacters = savedControlCharactersFlag;
					visual.SetActivePage(savedPage);
				}

				break;
			}
			case Function.WriteCharacterOnlyAtCursorPosition:
			{
				var textLibrary = visual as TextLibrary;
				var graphicsLibrary = visual as GraphicsLibrary;

				int savedPage = visual.ActivePageNumber;
				bool savedControlCharactersFlag = visual.ProcessControlCharacters;

				try
				{
					visual.SetActivePage(bh);

					int x = visual.CursorX;
					int y = visual.CursorY;

					byte character = al;

					for (int i = 0; i < input.CX; i++)
						visual.WriteText(character);
				}
				finally
				{
					visual.ProcessControlCharacters = savedControlCharactersFlag;
					visual.SetActivePage(savedPage);
				}

				break;
			}
			case Function.SetBackgroundAndBorderColour:
			{
				// CGA functions
				if (machine.GraphicsArray.Graphics.ShiftInterleave)
				{
					switch (bh)
					{
						case 0:
						{
							int newBackgroundAttribute = bl & 15;

							machine.InPort(InputStatusRegisters.InputStatus1Port);
							machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, 0);
							machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, unchecked((byte)newBackgroundAttribute));
							machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, AttributeControllerRegisters.Index_PaletteAddressSourceBit);

							break;
						}
						case 1:
						{
							int newCGAPalette = bl;

							if ((newCGAPalette >= 0) && (newCGAPalette <= 1))
								firmware.LoadCGAPalette(newCGAPalette, intensity: false);

							break;
						}
					}
				}

				break;
			}
			case Function.WriteGraphicsPixel:
			{
				if (visual is GraphicsLibrary graphics)
				{
					int savedPage = graphics.ActivePageNumber;

					try
					{
						graphics.SetActivePage(bh);

						int x = input.CX;
						int y = input.DX;

						int attribute = al;

						if (((attribute & 0x80) == 0) || (graphics.MaximumAttribute >= 0x80))
							graphics.PixelSet(x, y, attribute);
						else
							graphics.PixelSet(x, y, graphics.PixelGet(x, y) ^ attribute);
					}
					finally
					{
						graphics.SetActivePage(savedPage);
					}
				}

				break;
			}
			case Function.ReadGraphicsPixel:
			{
				if (visual is GraphicsLibrary graphics)
				{
					int savedPage = graphics.ActivePageNumber;

					try
					{
						graphics.SetActivePage(bh);

						int x = input.CX;
						int y = input.DX;

						result.AX &= 0xFF00;
						result.AX |= unchecked((byte)graphics.PixelGet(x, y));
					}
					finally
					{
						graphics.SetActivePage(savedPage);
					}
				}

				break;
			}
			case Function.TeletypeOutput:
			{
				int savedPage = visual.ActivePageNumber;
				bool savedControlCharactersFlag = visual.ProcessControlCharacters;
				byte savedAttribute = visual.CurrentAttributeByte;

				try
				{
					visual.SetActivePage(bh);

					visual.ProcessControlCharacters = true;

					if (visual is GraphicsLibrary graphics)
						graphics.DrawingAttribute = bl;

					visual.WriteText(al);
				}
				finally
				{
					visual.CurrentAttributeByte = savedAttribute;
					visual.ProcessControlCharacters = savedControlCharactersFlag;
					visual.SetActivePage(savedPage);
				}
				break;
			}
			case Function.GetCurrentVideoMode:
			{
				result.AX = unchecked((ushort)(
					((visual.CharacterWidth & 0xFF) << 8) |
					(firmware.LastModeNumber & 0xFF)));
				result.BX = unchecked((ushort)(
					(visual.ActivePageNumber & 0xFF) << 8));
				break;
			}
			case Function.ColourFunction:
			{
				var subfunction = (ColourFunction)al;

				switch (subfunction)
				{
					case ColourFunction.SetSinglePaletteRegister:
					{
						machine.InPort(InputStatusRegisters.InputStatus1Port);
						machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, bl);
						machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, bh);
						machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, AttributeControllerRegisters.Index_PaletteAddressSourceBit);
						break;
					}
					case ColourFunction.SetOverscanColour:
					{
						// The original IBM VGA BIOS is documented as having a bug in this function causing it to
						// update register 0x11 instead of register 0x10. This is not emulated here.
						machine.InPort(InputStatusRegisters.InputStatus1Port);
						machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, 0x10);
						machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, bh);
						machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, AttributeControllerRegisters.Index_PaletteAddressSourceBit);
						break;
					}
					case ColourFunction.SetAllPaletteRegisters:
					{
						int offset = (input.AsRegistersEx().ES << 4) + input.DX;

						machine.InPort(InputStatusRegisters.InputStatus1Port);

						for (int i = 0; i <= 16; i++) // include the overscan at index 0x10
						{
							byte paletteListEntry = machine.SystemMemory[offset + i];

							machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, unchecked((byte)i));
							machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, paletteListEntry);
						}

						machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, AttributeControllerRegisters.Index_PaletteAddressSourceBit);

						break;
					}
					case ColourFunction.ToggleIntensityBlinkingBit:
					{
						machine.InPort(InputStatusRegisters.InputStatus1Port);

						machine.OutPort(
							AttributeControllerRegisters.IndexAndDataWritePort,
							AttributeControllerRegisters.ModeControl | AttributeControllerRegisters.Index_PaletteAddressSourceBit);

						byte modeControl = machine.InPort(AttributeControllerRegisters.DataReadPort);

						if (bl == 0)
							modeControl &= unchecked((byte)~AttributeControllerRegisters.ModeControl_BlinkEnable);
						else
							modeControl |= AttributeControllerRegisters.ModeControl_BlinkEnable;

						machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, modeControl);

						break;
					}
					case ColourFunction.GetIndividualPaletteRegister:
					{
						machine.InPort(InputStatusRegisters.InputStatus1Port);
						machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, bl);

						byte registerValue = machine.InPort(AttributeControllerRegisters.DataReadPort);

						machine.InPort(InputStatusRegisters.InputStatus1Port);
						machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, AttributeControllerRegisters.Index_PaletteAddressSourceBit);

						result.BX = unchecked((ushort)(registerValue << 8));
						break;
					}
					case ColourFunction.ReadOverscanRegister:
					{
						machine.InPort(InputStatusRegisters.InputStatus1Port);
						machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, 0x10);

						byte registerValue = machine.InPort(AttributeControllerRegisters.DataReadPort);

						machine.InPort(InputStatusRegisters.InputStatus1Port);
						machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, AttributeControllerRegisters.Index_PaletteAddressSourceBit);

						result.BX = unchecked((ushort)(registerValue << 8));
						break;
					}
					case ColourFunction.ReadAllPaletteRegistersAndOverscanRegister:
					{
						int offset = (input.AsRegistersEx().ES << 4) + input.DX;

						for (int i = 0; i <= 16; i++) // include the overscan at index 0x10
						{
							machine.InPort(InputStatusRegisters.InputStatus1Port);
							machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, unchecked((byte)i));

							byte paletteListEntry = machine.InPort(AttributeControllerRegisters.DataReadPort);

							machine.SystemMemory[offset + i] = paletteListEntry;
						}

						machine.InPort(InputStatusRegisters.InputStatus1Port);
						machine.OutPort(AttributeControllerRegisters.IndexAndDataWritePort, AttributeControllerRegisters.Index_PaletteAddressSourceBit);

						break;
					}
					case ColourFunction.SetIndividualDACRegister:
					{
						byte r = unchecked((byte)(input.CX >> 8));
						byte g = unchecked((byte)input.CX);
						byte b = unchecked((byte)(input.DX >> 8));

						if (_enableGrayscaleSumming)
							Grayscale(ref r, ref g, ref b);

						machine.OutPort(DACRegisters.WriteIndexPort, bl);
						machine.OutPort(DACRegisters.DataPort, r);
						machine.OutPort(DACRegisters.DataPort, g);
						machine.OutPort(DACRegisters.DataPort, b);

						break;
					}
					case ColourFunction.SetBlockOfDACRegisters:
					{
						machine.OutPort(DACRegisters.WriteIndexPort, bl);

						int offset = (input.AsRegistersEx().ES << 4) + input.BX;

						int numBytes = input.CX * 3;

						for (int i = 0; i < numBytes; i += 3)
						{
							byte r = machine.SystemMemory[offset + i + 0];
							byte g = machine.SystemMemory[offset + i + 0];
							byte b = machine.SystemMemory[offset + i + 0];

							if (_enableGrayscaleSumming)
								Grayscale(ref r, ref g, ref b);

							machine.OutPort(DACRegisters.DataPort, r);
							machine.OutPort(DACRegisters.DataPort, g);
							machine.OutPort(DACRegisters.DataPort, b);
						}

						break;
					}
					case ColourFunction.SelectVideoDACColourPage:
					{
						var dacColourSubfunction = (DACColourPageSubfunction)bl;

						machine.InPort(InputStatusRegisters.InputStatus1Port);

						machine.OutPort(
							AttributeControllerRegisters.IndexAndDataWritePort,
							AttributeControllerRegisters.ModeControl | AttributeControllerRegisters.Index_PaletteAddressSourceBit);

						byte modeControlValue = machine.InPort(AttributeControllerRegisters.DataReadPort);

						switch (dacColourSubfunction)
						{
							case DACColourPageSubfunction.SelectPagingMode:
							{
								modeControlValue = unchecked((byte)(
									(modeControlValue & ~AttributeControllerRegisters.ModeControl_PaletteBits54Select) |
									((bh == 0) ? 0 : AttributeControllerRegisters.ModeControl_PaletteBits54Select)));

								machine.OutPort(
									AttributeControllerRegisters.IndexAndDataWritePort,
									modeControlValue);

								break;
							}
							case DACColourPageSubfunction.SelectPage:
							{
								int shift =
									((modeControlValue & AttributeControllerRegisters.ModeControl_PaletteBits54Select) != 0)
									? AttributeControllerRegisters.ColourSelect_Bits54Shift
									: AttributeControllerRegisters.ColourSelect_Bits76Shift;

								machine.InPort(InputStatusRegisters.InputStatus1Port);

								machine.OutPort(
									AttributeControllerRegisters.IndexAndDataWritePort,
									AttributeControllerRegisters.ColourSelect | AttributeControllerRegisters.Index_PaletteAddressSourceBit);
								machine.OutPort(
									AttributeControllerRegisters.IndexAndDataWritePort,
									unchecked((byte)(bh << shift)));

								break;
							}
						}

						break;
					}
					case ColourFunction.ReadIndividualDACRegister:
					{
						machine.OutPort(DACRegisters.ReadIndexPort, bl);

						byte r = machine.InPort(DACRegisters.DataPort);
						byte g = machine.InPort(DACRegisters.DataPort);
						byte b = machine.InPort(DACRegisters.DataPort);

						result.DX = unchecked((ushort)(r << 8));
						result.CX = unchecked((ushort)((g << 8) | b));

						break;
					}
					case ColourFunction.ReadBlockOfDACRegisters:
					{
						machine.OutPort(DACRegisters.ReadIndexPort, bl);

						int offset = (input.AsRegistersEx().ES << 4) + input.BX;

						int numBytes = input.CX * 3;

						for (int i = 0; i < numBytes; i++)
							machine.SystemMemory[offset + i] = machine.InPort(DACRegisters.DataPort);

						break;
					}
					case ColourFunction.SetPixelMask:
					{
						machine.OutPort(DACRegisters.MaskPort, bl);
						break;
					}
					case ColourFunction.ReadPixelMask:
					{
						result.BX = machine.InPort(DACRegisters.MaskPort);
						break;
					}
					case ColourFunction.GetVideoDACColourPageState:
					{
						machine.InPort(InputStatusRegisters.InputStatus1Port);

						machine.OutPort(
							AttributeControllerRegisters.IndexAndDataWritePort,
							AttributeControllerRegisters.ModeControl | AttributeControllerRegisters.Index_PaletteAddressSourceBit);

						byte modeControlValue = machine.InPort(AttributeControllerRegisters.DataReadPort);

						bool enableBits54 = ((modeControlValue & AttributeControllerRegisters.ModeControl_PaletteBits54Select) != 0);

						int shift = enableBits54
							? AttributeControllerRegisters.ColourSelect_Bits54Shift
							: AttributeControllerRegisters.ColourSelect_Bits76Shift;

						machine.InPort(InputStatusRegisters.InputStatus1Port);

						machine.OutPort(
							AttributeControllerRegisters.IndexAndDataWritePort,
							AttributeControllerRegisters.ColourSelect | AttributeControllerRegisters.Index_PaletteAddressSourceBit);

						int dacColourPage = machine.InPort(AttributeControllerRegisters.DataReadPort) >> shift;

						bl = enableBits54 ? (byte)1 : (byte)0;
						bh = unchecked((byte)dacColourPage);

						result.BX = unchecked((ushort)((bh << 8) | bl));

						break;
					}
					case ColourFunction.PerformGrayscaleSumming:
					{
						for (int i = 0; i < input.CX; i++)
						{
							machine.OutPort(DACRegisters.ReadIndexPort, unchecked((byte)(input.BX + i)));

							byte r = machine.InPort(DACRegisters.DataPort);
							byte g = machine.InPort(DACRegisters.DataPort);
							byte b = machine.InPort(DACRegisters.DataPort);

							Grayscale(ref r, ref g, ref b);

							machine.OutPort(DACRegisters.WriteIndexPort, unchecked((byte)(input.BX + i)));

							machine.OutPort(DACRegisters.DataPort, r);
							machine.OutPort(DACRegisters.DataPort, g);
							machine.OutPort(DACRegisters.DataPort, b);
						}

						break;
					}
				}

				break;
			}
			case Function.FontFunction:
			{
				var subfunction = (FontFunction)al;

				switch (subfunction)
				{
					case FontFunction.LoadUserSpecifiedPatterns:
					case FontFunction.LoadUserSpecifiedPatternsAndUpdateMode:
					{
						int scansPerCharacter = bh;
						int blockIndex = bl;
						int firstCharacter = input.DX;
						int numCharacters = input.CX;

						byte[][] fontData = new byte[numCharacters][];

						int offset = (input.AsRegistersEx().ES << 4) | input.BP;

						for (int i = 0; i < numCharacters; i++)
						{
							fontData[i] = new byte[scansPerCharacter];

							for (int j = 0; j < scansPerCharacter; j++)
								fontData[i][j] = machine.SystemMemory[offset++];
						}

						firmware.LoadFontIntoCharacterGenerator(fontData, firstCharacter, blockIndex);

						if (subfunction >= FontFunction.AndUpdateMode)
							UpdateModeForFont(scansPerCharacter);

						break;
					}
					case FontFunction.LoadROMMonochromePatterns:
					case FontFunction.LoadROMMonochromePatternsAndUpdateMode:
					{
						// Ralf Brown says this just means the 8x14 font
						firmware.LoadFontIntoCharacterGenerator(
							firmware.GetFont(characterScans: 14),
							firstCharacter: 0,
							fontBlockIndex: bl);

						if (subfunction >= FontFunction.AndUpdateMode)
							UpdateModeForFont(scansPerCharacter: 14);

						break;
					}
					case FontFunction.LoadROM8x8DoubleDotPatterns:
					case FontFunction.LoadROM8x8DoubleDotPatternsAndUpdateMode:
					{
						firmware.LoadFontIntoCharacterGenerator(
							firmware.GetFont(characterScans: 8),
							firstCharacter: 0,
							fontBlockIndex: bl);

						if (subfunction >= FontFunction.AndUpdateMode)
							UpdateModeForFont(scansPerCharacter: 8);

						break;
					}
					case FontFunction.SetBlockSpecifier:
					{
						machine.OutPort(SequencerRegisters.IndexPort, SequencerRegisters.CharacterSet);
						machine.OutPort(SequencerRegisters.DataPort, bl);
						break;
					}
					case FontFunction.LoadROM8x16CharacterSet:
					case FontFunction.LoadROM8x16CharacterSetAndUpdateMode:
					{
						firmware.LoadFontIntoCharacterGenerator(
							firmware.GetFont(16));

						if (subfunction >= FontFunction.AndUpdateMode)
							UpdateModeForFont(scansPerCharacter: 16);

						break;
					}
					case FontFunction.SetUser8x8GraphicsChars:
					{
						LoadUserFontData(input, scansPerCharacter: 8, visual);
						break;
					}
					case FontFunction.SetUserGraphicsCharacters:
					{
						// Ralf Brown documents BL and DL as specifying something to do with
						// the number of rows, but it isn't clear what it actually means.
						LoadUserFontData(input, scansPerCharacter: input.CX, visual);
						break;
					}
					case FontFunction.SetROM8x14GraphicsChars:
					{
						// Ralf Brown documents BL and DL as specifying something to do with
						// the number of rows, but it isn't clear what it actually means.
						if (visual is GraphicsLibrary graphics)
						{
							graphics.Font = firmware.GetFont(14);
							graphics.CharacterScans = 14;
						}

						break;
					}
					case FontFunction.SetROM8x8DoubleDotChars:
					{
						// Ralf Brown documents BL and DL as specifying something to do with
						// the number of rows, but it isn't clear what it actually means.
						if (visual is GraphicsLibrary graphics)
						{
							graphics.Font = firmware.GetFont(8);
							graphics.CharacterScans = 8;
						}

						break;
					}
					case FontFunction.Load8x16GraphicsChars:
					{
						// Ralf Brown documents BL and DL as specifying something to do with
						// the number of rows, but it isn't clear what it actually means.
						if (visual is GraphicsLibrary graphics)
						{
							graphics.Font = firmware.GetFont(16);
							graphics.CharacterScans = 16;
						}

						break;
					}
					case FontFunction.GetFontInformation:
					{
						if (visual is GraphicsLibrary graphics)
						{
							// I have arbitrarily decided to store the font information at 8KB intervals in segment 0x1000.
							// At the time of writing this, there is not (yet?) any memory allocator or allocation scheme. :-)

							int fontSpecifier = (result.BX >> 8);

							int offset = 0x10000 + fontSpecifier * 8192;

							byte[][] font;
							int firstCharacter = 0;
							int length = 256;

							switch (fontSpecifier)
							{
								case 0: // Upper plane of 8x8 font.
									if (graphics.CharacterHeight == 8)
										font = graphics.Font;
									else
										font = firmware.GetFont(8);
									firstCharacter = 0x80;
									length = 0x80;
									break;
								case 1: // Lower plane of 8x8 font, or the entire font for other character heights.
									font = graphics.Font;
									if (graphics.CharacterHeight == 8)
										length = 0x80;
									break;
								case 2: // ROM 8x14
								case 5: // ROM 9x16? Still 8 bits per scan, though?
									font = firmware.GetFont(14);
									break;
								case 3: // ROM 8x8 (lower plane)
									font = firmware.GetFont(8);
									length = 0x80;
									break;
								case 4: // ROM 8x8 (upper plane)
									font = firmware.GetFont(8);
									firstCharacter = 0x80;
									length = 0x80;
									break;
								case 6: // ROM 8x16
								case 7: // ROM 9x16? Still 8 bits per scan, though?
									font = firmware.GetFont(16);
									break;
								default:
									goto UnknownFontSpecifier; // I wish C# had "break break" to exit 2 level of nesting :-)
							}

							for (int i = 0, o = 0; i < length; i++)
							{
								int ch = firstCharacter + i;

								for (int j = 0; j < font[ch].Length; j++)
									machine.SystemMemory[offset + o++] = font[ch][j];
							}

							result.ES = 0x1000;
							result.BP = unchecked((ushort)(offset - 0x10000));

							// Some misc extra data:
							// - Scans/character of on-screen font (not the requested font!)
							result.CX = unchecked((ushort)graphics.CharacterScans);
							// - Highest character row on the screen
							result.DX = unchecked((ushort)graphics.CharacterHeight);
						}

					UnknownFontSpecifier:
						break;
					}
				}

				break;
			}
			case Function.AlternateFunction:
			{
				var subfunction = (AlternateFunction)bl;

				switch (subfunction)
				{
					case AlternateFunction.GetEGAInfo:
					{
						byte miscellaneousOutput = machine.InPort(MiscellaneousOutputRegisters.ReadPort);

						bool useMonoIOPorts = ((miscellaneousOutput & MiscellaneousOutputRegisters.IOAddress) == 0);

						// BH = mono I/O ports, BL = installed memory planes - 1
						result.BX = unchecked((ushort)((useMonoIOPorts ? 0x100 : 0) | 3 /*256KB*/));
						// Feature connectors and switch settings
						result.CX = 0x0000;

						break;
					}
					case AlternateFunction.AlternatePrintScreen:
					{
						// Nothing to do here. Apparently, system BIOS PrintScreen routines
						// often did not understand text modes with more than 25 rows of text,
						// and some video cards provided a more capable replacement.

						break;
					}
					case AlternateFunction.SelectVerticalResolution:
					{
						switch (al)
						{
							case 0: firmware.DesiredTextModeScanLines = 200; break;
							case 1: firmware.DesiredTextModeScanLines = 350; break;
							case 2: firmware.DesiredTextModeScanLines = 400; break;
						}

						break;
					}
					case AlternateFunction.PaletteLoading:
					{
						firmware.LoadPaletteOnModeChange = (al == 0x00);
						break;
					}
					case AlternateFunction.GrayscaleSumming:
					{
						_enableGrayscaleSumming = (al == 0);
						break;
;					}
					case AlternateFunction.VideoRefreshControl:
					{
						bool enableRefresh = (al == 0x00);

						machine.InPort(InputStatusRegisters.InputStatus1Port);

						machine.OutPort(
							AttributeControllerRegisters.IndexAndDataWritePort,
							enableRefresh
							? AttributeControllerRegisters.Index_PaletteAddressSourceBit
							: (byte)0);

						break;
					}
					case AlternateFunction.VideoAddressing:
					case AlternateFunction.CursorEmulation:
					case AlternateFunction.DisplaySwitchInterface:
					{
						// Not supported. If supported, AL = 0x12 on return, so let's not do that.
						result.AX = 0;
						break;
					}
				}

				break;
			}
			case Function.WriteString:
			{
				int savedPage = visual.ActivePageNumber;
				bool savedControlCharactersFlag = visual.ProcessControlCharacters;
				byte savedAttributeByte = visual.CurrentAttributeByte;

				try
				{
					visual.SetActivePage(bh);

					visual.ProcessControlCharacters = true;

					visual.CurrentAttributeByte = bl;

					int offset = (input.AsRegistersEx().ES << 4) + input.BP;

					bool advanceCursor = (al & 1) != 0;
					bool perCharacterAttribute = (al & 2) != 0;

					int numChars = input.CX;

					(int savedCursorX, int savedCursorY) = (visual.CursorX, visual.CursorY);

					for (int i = 0; i < numChars; i++)
					{
						byte ch = machine.SystemMemory[offset++];

						if (perCharacterAttribute)
							visual.CurrentAttributeByte = machine.SystemMemory[offset++];

						visual.WriteText(ch);
					}

					if (!advanceCursor)
						visual.MoveCursor(savedCursorX, savedCursorY);
				}
				finally
				{
					visual.CurrentAttributeByte = savedAttributeByte;
					visual.ProcessControlCharacters = savedControlCharactersFlag;
					visual.SetActivePage(savedPage);
				}

				break;
			}
			case Function.DisplayCombinationCode:
			{
				var subfunction = (DisplayCombinationCodeFunction)al;

				switch (subfunction)
				{
					case DisplayCombinationCodeFunction.Get:
					{
						result.AX = 0x1A1A; // supported
						result.BX = _displayCombinationCodes;
						break;
					}
					case DisplayCombinationCodeFunction.Set:
					{
						result.AX = 0x1A1A; // supported
						_displayCombinationCodes = input.BX;
						break;
					}
				}

				break;
			}
			case Function.SaveRestoreVideoState:
			{
				var subfunction = (StateFunction)al;

				switch (subfunction)
				{
					case StateFunction.ReturnStateBufferSize:
					{
						int blocksNeeded = (StateBufferSize + 63) / 64;

						result.BX = unchecked((ushort)blocksNeeded);

						break;
					}
					case StateFunction.SaveVideoState:
					{
						int memoryOffset = (input.AsRegistersEx().ES * 0x10) + input.BX;

						var stream = new SystemMemoryStream(machine.SystemMemory, memoryOffset, StateBufferSize);

						var writer = new BinaryWriter(stream);

						firmware.SaveState(writer);

						writer.Flush();

						break;
					}
					case StateFunction.RestoreVideoState:
					{
						int memoryOffset = (input.AsRegistersEx().ES * 0x10) + input.BX;

						var stream = new SystemMemoryStream(machine.SystemMemory, memoryOffset, StateBufferSize);

						var reader = new BinaryReader(stream);

						firmware.RestoreState(reader);

						break;
					}
				}

				break;
			}
		}

		throw new NotImplementedException();
	}

	static void Grayscale(ref byte r, ref byte g, ref byte b)
	{
		byte intensity = unchecked((byte)((r * 30 + g * 59 + b * 11) / 100));

		r = g = b = intensity;
	}

	private void UpdateModeForFont(int scansPerCharacter)
	{
		int rows = machine.GraphicsArray.CRTController.NumScanLines / scansPerCharacter;

		int verticalDisplayEnd = rows * scansPerCharacter - 1;

		machine.OutPort(CRTControllerRegisters.IndexPort, CRTControllerRegisters.Overflow);

		byte overflowRegister = machine.InPort(CRTControllerRegisters.DataPort);

		overflowRegister = unchecked((byte)(
			(overflowRegister & ~(CRTControllerRegisters.Overflow_VerticalDisplayEnd8 | CRTControllerRegisters.Overflow_VerticalDisplayEnd9)) |
			(((overflowRegister & 0x100) != 0) ? CRTControllerRegisters.Overflow_VerticalDisplayEnd8 : 0) |
			(((overflowRegister & 0x200) != 0) ? CRTControllerRegisters.Overflow_VerticalDisplayEnd9 : 0)));

		machine.OutPort(CRTControllerRegisters.IndexPort, CRTControllerRegisters.MaximumScanLine);
		machine.OutPort(CRTControllerRegisters.DataPort, unchecked((byte)(scansPerCharacter - 1)));

		machine.OutPort(CRTControllerRegisters.IndexPort, CRTControllerRegisters.CursorStart);
		machine.OutPort(CRTControllerRegisters.DataPort, unchecked((byte)(scansPerCharacter - 2)));

		machine.OutPort(CRTControllerRegisters.IndexPort, CRTControllerRegisters.CursorEnd);
		machine.OutPort(CRTControllerRegisters.DataPort, 0);

		machine.OutPort(CRTControllerRegisters.IndexPort, CRTControllerRegisters.VerticalDisplayEnd);
		machine.OutPort(CRTControllerRegisters.DataPort, unchecked((byte)verticalDisplayEnd));

		machine.OutPort(CRTControllerRegisters.IndexPort, CRTControllerRegisters.Overflow);
		machine.OutPort(CRTControllerRegisters.DataPort, overflowRegister);

		machine.OutPort(CRTControllerRegisters.IndexPort, CRTControllerRegisters.UnderlineLocation);
		machine.OutPort(CRTControllerRegisters.DataPort, unchecked((byte)scansPerCharacter));
	}

	private void LoadUserFontData(Registers input, int scansPerCharacter, VisualLibrary visual)
	{
		if (visual is GraphicsLibrary graphics)
		{
			byte[][] newFont = new byte[256][];

			int offset = (input.AsRegistersEx().ES << 4) + input.BP;

			for (int ch = 0; ch < 256; ch++)
			{
				newFont[ch] = new byte[scansPerCharacter];

				for (int scan = 0; scan < scansPerCharacter; scan++)
					newFont[ch][scan] = machine.SystemMemory[offset++];
			}

			graphics.Font = newFont;
			graphics.CharacterScans = scansPerCharacter;
		}
	}
}
