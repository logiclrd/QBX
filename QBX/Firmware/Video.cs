using System;

using QBX.Hardware;

using static QBX.Hardware.GraphicsArray;

namespace QBX.Firmware;

public class Video(Machine machine)
{
	// 1: 40x25 text mode rendered to 360x400 dots
	static readonly ModeParameters Mode1 =
		new ModeParameters()
		{
			ScreenNumber = 0,
			Characters = (40, 25),
			IsGraphicsMode = false,
			PaletteType = PaletteType.VGA,
			BaseAddress = BaseAddress.B800,
			CharacterWidth = CharacterWidth._9,
			CharacterHeight = 16,
			MemoryAddressSize = 1,
			ScanDoubling = false,
			OddEvenAddressing = true,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b0011,
			HalfDotClockRate = true,
			Chain4Mode = false,
		};

	// 3: 80x25 text mode rendered to 720x400 dots
	static readonly ModeParameters Mode3 =
		new ModeParameters()
		{
			ScreenNumber = 0,
			Characters = (80, 25),
			IsGraphicsMode = false,
			PaletteType = PaletteType.VGA,
			BaseAddress = BaseAddress.B800,
			CharacterWidth = CharacterWidth._9,
			CharacterHeight = 16,
			MemoryAddressSize = 1,
			ScanDoubling = false,
			OddEvenAddressing = true,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b0011,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// 5: 320x200 graphics using planes 0 and 1 for pixels
	static readonly ModeParameters Mode5 =
		new ModeParameters()
		{
			ScreenNumber = 1,
			Pixels = (320, 200),
			Characters = (40, 25),
			IsGraphicsMode = true,
			PaletteType = PaletteType.CGA,
			BaseAddress = BaseAddress.B800,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 8,
			PixelsPerAddress = 4,
			MemoryAddressSize = 2,
			ScanDoubling = true,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = true,
			Use256Colours = false,
			PlaneMask = 0,
			HalfDotClockRate = true,
			Chain4Mode = false,
		};

	// 6: 640x200 graphics emulating monochrome by aliasing all the planes together
	static readonly ModeParameters Mode6 =
		new ModeParameters()
		{
			ScreenNumber = 2,
			Pixels = (640, 200),
			Characters = (80, 25),
			IsGraphicsMode = true,
			IsMonochrome = true,
			PaletteType = PaletteType.VGA,
			BaseAddress = BaseAddress.B800,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 8,
			PixelsPerAddress = 8,
			MemoryAddressSize = 2,
			ScanDoubling = true,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b1111,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// 7: 80x25 text rendered to 720x400 dots
	static readonly ModeParameters Mode7 =
		new ModeParameters()
		{
			Characters = (80, 25),
			IsGraphicsMode = false,
			PaletteType = PaletteType.VGA,
			BaseAddress = BaseAddress.B800,
			CharacterWidth = CharacterWidth._9,
			CharacterHeight = 16,
			MemoryAddressSize = 1,
			ScanDoubling = false,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b0011,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// D: 320x200 graphics with 4bpp planar colour
	static readonly ModeParameters ModeD =
		new ModeParameters()
		{
			ScreenNumber = 7,
			Pixels = (320, 200),
			Characters = (40, 25),
			IsGraphicsMode = true,
			PaletteType = PaletteType.VGA,
			BaseAddress = BaseAddress.A000,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 8,
			PixelsPerAddress = 8,
			MemoryAddressSize = 1,
			ScanDoubling = true,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b1111,
			HalfDotClockRate = true,
			Chain4Mode = false,
		};

	// E: 640x200 graphics with 4bpp planar colour
	static readonly ModeParameters ModeE =
		new ModeParameters()
		{
			ScreenNumber = 8,
			Pixels = (640, 200),
			Characters = (80, 25),
			IsGraphicsMode = true,
			PaletteType = PaletteType.VGA,
			BaseAddress = BaseAddress.A000,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 8,
			PixelsPerAddress = 8,
			MemoryAddressSize = 1,
			ScanDoubling = true,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b1111,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// F: 640x350 graphics emulating monochrome by aliasing all the planes together
	static readonly ModeParameters ModeF =
		new ModeParameters()
		{
			ScreenNumber = 10,
			Pixels = (640, 350),
			Characters = (80, 25),
			IsGraphicsMode = true,
			IsMonochrome = true,
			PaletteType = PaletteType.VGA,
			BaseAddress = BaseAddress.A000,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 14,
			PixelsPerAddress = 8,
			MemoryAddressSize = 2,
			ScanDoubling = false,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b1111,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// 10: 640x350 graphics with 4bbp planar colour
	static readonly ModeParameters Mode10 =
		new ModeParameters()
		{
			ScreenNumber = 9,
			Pixels = (640, 350),
			Characters = (80, 25),
			IsGraphicsMode = true,
			PaletteType = PaletteType.VGA,
			BaseAddress = BaseAddress.A000,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 14,
			PixelsPerAddress = 8,
			MemoryAddressSize = 2,
			ScanDoubling = false,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b1111,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// 11: 640x480 emulating monochrome by aliasing all the planes together
	static readonly ModeParameters Mode11 =
		new ModeParameters()
		{
			ScreenNumber = 11,
			Pixels = (640, 480),
			Characters = (80, 30),
			IsGraphicsMode = true,
			IsMonochrome = true,
			PaletteType = PaletteType.VGA,
			BaseAddress = BaseAddress.A000,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 16,
			PixelsPerAddress = 8,
			MemoryAddressSize = 1,
			ScanDoubling = false,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0b1111,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// 12: 640x480 with 4bpp planar colour
	static readonly ModeParameters Mode12 =
		new ModeParameters()
		{
			ScreenNumber = 12,
			Pixels = (640, 480),
			Characters = (80, 30),
			IsGraphicsMode = true,
			PaletteType = PaletteType.VGA,
			BaseAddress = BaseAddress.A000,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 16,
			PixelsPerAddress = 8,
			MemoryAddressSize = 2,
			ScanDoubling = false,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0,
			HalfDotClockRate = false,
			Chain4Mode = false,
		};

	// 13: 320x200 with 8bpp linear colour
	static readonly ModeParameters Mode13 =
		new ModeParameters()
		{
			ScreenNumber = 13,
			Pixels = (320, 200),
			Characters = (40, 25),
			IsGraphicsMode = true,
			PaletteType = PaletteType.VGA,
			BaseAddress = BaseAddress.A000,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 8,
			PixelsPerAddress = 1,
			MemoryAddressSize = 4,
			ScanDoubling = true,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = true,
			PlaneMask = 0,
			HalfDotClockRate = true,
			Chain4Mode = true,
		};

	public static readonly ModeParameters?[] Modes =
		new[]
		{
			Mode1, // 0: a monochrome version of mode 1 on CGA, but the same as mode 1 on VGA
			Mode1,
			Mode3, // 2: a monochrome version of mode 3 on CGA, but the same as mode 3 on VGA
			Mode3,
			Mode5, // 4: a monochrome version of mode 5 on CGA, but the same as mode 5 on VGA
			Mode5,
			Mode6,
			Mode7,
			null, // no mode 8
			null, // no mode 9
			null, // no mode A
			null, // mode B is reserved for EGA internal use
			null, // mode C is reserved for EGA internal use
			ModeD,
			ModeE,
			ModeF,
			Mode10,
			Mode11,
			Mode12,
			Mode13,
		};

	public bool SetMode(int modeNumber)
	{
		if ((modeNumber < 0) && (modeNumber >= Modes.Length))
			return false;

		var mode = Modes[modeNumber];

		if (mode == null)
			return false;

		var array = machine.GraphicsArray;

		array.VRAM.AsSpan().Clear();

		array.OutPort2(
			SequencerRegisters.IndexPort,
			SequencerRegisters.Reset,
			0b00);

		array.OutPort2(
			GraphicsRegisters.IndexPort,
			GraphicsRegisters.EnableSetReset,
			0);

		array.OutPort2(
			GraphicsRegisters.IndexPort,
			GraphicsRegisters.ColourCompare,
			0);

		array.OutPort2(
			GraphicsRegisters.IndexPort,
			GraphicsRegisters.DataRotate,
			0);

		array.OutPort2(
			GraphicsRegisters.IndexPort,
			GraphicsRegisters.ReadMapSelect,
			0);

		array.OutPort2(
			GraphicsRegisters.IndexPort,
			GraphicsRegisters.GraphicsMode,
			unchecked((byte)(
			(mode.OddEvenAddressing ? GraphicsRegisters.GraphicsMode_HostOddEvenRead : 0) |
			(mode.ShiftRegisterInterleave ? GraphicsRegisters.GraphicsMode_ShiftInterleave : 0) |
			(mode.Use256Colours ? GraphicsRegisters.GraphicsMode_Shift256 : 0))));

		array.OutPort2(
			GraphicsRegisters.IndexPort,
			GraphicsRegisters.MiscGraphics,
			unchecked((byte)(
			(mode.IsGraphicsMode ? GraphicsRegisters.MiscGraphics_AlphanumericModeDisable : 0) |
			(mode.OddEvenAddressing ? GraphicsRegisters.MiscGraphics_ChainOddEven : 0) |
			mode.BaseAddress switch
			{
				BaseAddress.B800 => GraphicsRegisters.MiscGraphics_MemoryMap_ColourText32K,
				BaseAddress.A000 => GraphicsRegisters.MiscGraphics_MemoryMap_Graphics64K,
				_ => default,
			})));

		array.OutPort2(
			GraphicsRegisters.IndexPort,
			GraphicsRegisters.ColourDoNotCare,
			0);

		array.OutPort2(
			GraphicsRegisters.IndexPort,
			GraphicsRegisters.BitMask,
			mode.PlaneMask);

		array.OutPort2(
			SequencerRegisters.IndexPort,
			SequencerRegisters.ClockingMode,
			unchecked((byte)(
			mode.CharacterWidth switch
			{
				CharacterWidth._8 => SequencerRegisters.ClockingMode_CharacterWidth_8,
				CharacterWidth._9 => SequencerRegisters.ClockingMode_CharacterWidth_9,
				_ => default
			} |
			(mode.HalfDotClockRate ? SequencerRegisters.ClockingMode_DotClockHalfRate : 0))));

		array.OutPort2(
			SequencerRegisters.IndexPort,
			SequencerRegisters.CharacterSet,
			0);

		array.OutPort2(
			SequencerRegisters.IndexPort,
			SequencerRegisters.SequencerMemoryMode,
			unchecked((byte)(
			SequencerRegisters.SequencerMemoryMode_ExtendedMemory |
			(mode.OddEvenAddressing ? 0 : SequencerRegisters.SequencerMemoryMode_OddEvenDisable) |
			(mode.Chain4Mode ? SequencerRegisters.SequencerMemoryMode_Chain4 : 0))));

		array.InPort(0x3DA);

		array.OutPort(
			MiscellaneousOutputRegisters.WritePort,
			unchecked((byte)(
			MiscellaneousOutputRegisters.IOAddress |
			MiscellaneousOutputRegisters.RAMEnable |
			mode.CharacterWidth switch
			{
				CharacterWidth._9 => MiscellaneousOutputRegisters.Clock_28MHz,
				CharacterWidth._8 => MiscellaneousOutputRegisters.Clock_25MHz,
				_ => default
			})));

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.HorizontalTotal,
			unchecked((byte)(mode.Characters.Width * (int)mode.CharacterWidth / 8 + 5)));

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.EndHorizontalDisplay,
			unchecked((byte)(mode.Characters.Width - 1)));

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.StartHorizontalBlanking,
			unchecked((byte)(mode.Characters.Width)));

		int numScanLines =
			(mode.IsGraphicsMode
			? mode.Pixels.Height
			: (mode.Characters.Height * mode.CharacterHeight));

		int displayEnd = numScanLines - 1;

		int verticalTotal = 46 + displayEnd;

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.VerticalTotal,
			unchecked((byte)verticalTotal));

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.VerticalDisplayEnd,
			unchecked((byte)displayEnd));

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.Overflow,
			unchecked((byte)(
			((verticalTotal & 256) >> 8) |
			((displayEnd & 256) >> 7) |
			((verticalTotal & 512) >> 4) |
			((displayEnd & 512) >> 3))));

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.PresetRowScan,
			0);

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.MaximumScanLine,
			unchecked((byte)(
			(mode.IsGraphicsMode ? 0 : (mode.CharacterHeight - 1)) |
			(mode.ScanDoubling ? CRTControllerRegisters.MaximumScanLine_ScanDoubling : 0))));

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.CursorStart,
			unchecked((byte)(
			(mode.IsGraphicsMode
			? CRTControllerRegisters.CursorStart_Disable
			: mode.CharacterHeight - 2))));

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.CursorEnd,
			unchecked((byte)(mode.CharacterHeight - 1)));

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.StartAddressHigh,
			0);

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.StartAddressLow,
			0);

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.CursorLocationHigh,
			0);

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.CursorLocationLow,
			0);

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.Offset,
			unchecked((byte)(
			(mode.IsGraphicsMode ? mode.Pixels.Width : mode.Characters.Width)
			/ (mode.PixelsPerAddress * mode.MemoryAddressSize * 2))));

		byte crtcMode = unchecked((byte)(
			(mode.MemoryAddressSize == 1 ? CRTControllerRegisters.ModeControl_ByteAddressing : 0) |
			(mode.ShiftRegisterInterleave
			? CRTControllerRegisters.ModeControl_MapDisplayAddress13_RowScan0
			: CRTControllerRegisters.ModeControl_MapDisplayAddress13_Address13) |
			CRTControllerRegisters.ModeControl_MapDisplayAddress14_Address14));

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.ModeControl,
			crtcMode);

		byte attributeMode = unchecked((byte)(
			(mode.Use256Colours ? AttributeControllerRegisters.ModeControl_8bitColour : 0) |
			(mode.IsGraphicsMode ? 0 : AttributeControllerRegisters.ModeControl_BlinkEnable) |
			AttributeControllerRegisters.ModeControl_LineGraphicsEnable |
			(mode.IsMonochrome ? AttributeControllerRegisters.ModeControl_MonochromeEmulation : 0) |
			(mode.IsGraphicsMode ? AttributeControllerRegisters.ModeControl_GraphicsMode : 0)));

		// reset the AttributeController port mode
		array.InPort(InputStatusRegisters.InputStatus1Port);

		array.OutPort(
			AttributeControllerRegisters.IndexAndDataWritePort,
			AttributeControllerRegisters.ModeControl);
		array.OutPort(
			AttributeControllerRegisters.IndexAndDataWritePort,
			attributeMode);

		if (!mode.IsGraphicsMode)
		{
			for (int i = 0, o = mode.Characters.Width * mode.Characters.Height; i < o; i++)
				array.VRAM[0x10000 + i] = 7;

			var font = GetFontForCurrentMode();

			LoadFontIntoCharacterGenerator(font);
		}

		switch (mode.PaletteType)
		{
			case PaletteType.CGA: LoadCGAPalette(); break;
			case PaletteType.VGA: LoadVGAPalette(); break;
		}

		array.OutPort2(
			SequencerRegisters.IndexPort,
			SequencerRegisters.Reset,
			0b11);

		return true;
	}

	public void SetCharacterRows(int rows)
	{
		var array = machine.GraphicsArray;

		if (array.Graphics.DisableText)
			return;

		byte maximumScanLineValue;
		int forceNumScanLines = 0;

		switch (rows)
		{
			case 25:
				if ((array.CRTController.NumScanLines >= 344)
				 && (array.CRTController.NumScanLines <= 350))
				{
					forceNumScanLines = 350;
					maximumScanLineValue = 13; // 8x14 font
				}
				else
					maximumScanLineValue = 15; // 8x16 font
				break;
			case 43:
				maximumScanLineValue = 7;
				forceNumScanLines = 344;
				break;
			case 50:
				maximumScanLineValue = 7;
				forceNumScanLines = 400;
				break;

			default:
				throw new Exception("Invalid row count " + rows);
		}

		// TODO: should instead offer direct set of visible scan lines and character maximum scan line here,
		// let the caller figure out the combos and remember state that is meta to the video hardware

		if (forceNumScanLines != 0)
		{
			array.OutPort(CRTControllerRegisters.IndexPort, CRTControllerRegisters.Overflow);

			byte overflow = array.InPort(CRTControllerRegisters.DataPort);

			overflow = unchecked((byte)(
				(overflow
					& ~CRTControllerRegisters.Overflow_VerticalDisplayEnd8
					& ~CRTControllerRegisters.Overflow_VerticalDisplayEnd9) |
				((forceNumScanLines >> 8) & 1) * CRTControllerRegisters.Overflow_VerticalDisplayEnd8 |
				((forceNumScanLines >> 9) & 1) * CRTControllerRegisters.Overflow_VerticalDisplayEnd9));

			array.OutPort2(
				CRTControllerRegisters.IndexPort,
				CRTControllerRegisters.VerticalDisplayEnd,
				unchecked((byte)forceNumScanLines));

			array.OutPort2(
				CRTControllerRegisters.IndexPort,
				CRTControllerRegisters.Overflow,
				overflow);
		}

		array.OutPort(CRTControllerRegisters.IndexPort, CRTControllerRegisters.MaximumScanLine);

		byte maximumScanLine = array.InPort(CRTControllerRegisters.DataPort);

		maximumScanLine = unchecked((byte)(
			(maximumScanLine & ~CRTControllerRegisters.MaximumScanLine_ScanMask) |
			maximumScanLineValue));

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.MaximumScanLine,
			maximumScanLine);

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.CursorStart,
			(byte)(maximumScanLine - 1));

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.CursorEnd,
			maximumScanLine);

		var font = GetFont(maximumScanLineValue + 1);

		LoadFontIntoCharacterGenerator(font);
	}

	public void SetCharacterWidth(int width)
	{
		byte clock;
		byte characterWidth;

		switch (width)
		{
			case 8:
				clock = MiscellaneousOutputRegisters.Clock_25MHz;
				characterWidth = SequencerRegisters.ClockingMode_CharacterWidth_8;
				break;
			case 9:
				clock = MiscellaneousOutputRegisters.Clock_28MHz;
				characterWidth = SequencerRegisters.ClockingMode_CharacterWidth_9;
				break;

			default:
				throw new InvalidOperationException();
		}

		var array = machine.GraphicsArray;

		array.MiscellaneousOutput.Register = unchecked((byte)(
			(array.MiscellaneousOutput.Register & ~MiscellaneousOutputRegisters.ClockMask) |
			clock));

		array.Sequencer.Registers[SequencerRegisters.ClockingMode] = unchecked((byte)(
			(array.Sequencer.Registers[SequencerRegisters.ClockingMode]
				& ~SequencerRegisters.ClockingMode_CharacterWidthMask) |
			characterWidth));
	}

	public int ComputePageSize() => ComputePageSize(machine.GraphicsArray);

	public static int ComputePageSize(GraphicsArray array)
	{
		if (array.Graphics.DisableText == false)
		{
			int width = array.CRTController.Registers.EndHorizontalDisplay + 1;
			int height = array.CRTController.NumScanLines / array.CRTController.CharacterHeight;

			return width * height;
		}
		else
		{
			int scans = array.CRTController.NumScanLines;

			if (array.Graphics.ShiftInterleave)
				scans /= 2;

			int stride = array.CRTController.Stride;

			return stride * scans;
		}
	}

	public bool SetVisiblePage(int pageNumber)
	{
		int pageSize = ComputePageSize();
		int pageCount = 16384 / pageSize;

		if ((pageNumber >= 0) && (pageNumber < pageCount))
		{
			int startAddress = pageNumber * pageSize / 4;

			machine.GraphicsArray.CRTController.Registers[CRTControllerRegisters.StartAddressHigh] =
				unchecked((byte)(startAddress >> 8));
			machine.GraphicsArray.CRTController.Registers[CRTControllerRegisters.StartAddressLow] =
				unchecked((byte)(startAddress & 255));

			return true;
		}

		return false;
	}

	public void LoadCGAPalette(int cgaPalette = 1, bool intensity = false)
	{
		LoadVGAPalette();

		switch (cgaPalette)
		{
			case 0:
				machine.GraphicsArray.AttributeController.Registers[0] = 0;
				machine.GraphicsArray.AttributeController.Registers[1] = unchecked((byte)(intensity ? 10 : 2));
				machine.GraphicsArray.AttributeController.Registers[2] = unchecked((byte)(intensity ? 12 : 4));
				machine.GraphicsArray.AttributeController.Registers[3] = unchecked((byte)(intensity ? 14 : 6));
				break;
			case 1:
				machine.GraphicsArray.AttributeController.Registers[0] = 0;
				machine.GraphicsArray.AttributeController.Registers[1] = unchecked((byte)(intensity ? 11 : 3));
				machine.GraphicsArray.AttributeController.Registers[2] = unchecked((byte)(intensity ? 13 : 5));
				machine.GraphicsArray.AttributeController.Registers[3] = unchecked((byte)(intensity ? 15 : 7));
				break;
			// Secret CGA palette 2 -- as I understand it, this is what happens if you
			// plug an actual CGA into a colour composite monitor but disable the Color
			// Burst signal. Trivial to emulate on VGA with attribute mapping.
			case 2:
				machine.GraphicsArray.AttributeController.Registers[0] = 0;
				machine.GraphicsArray.AttributeController.Registers[1] = unchecked((byte)(intensity ? 11 : 3));
				machine.GraphicsArray.AttributeController.Registers[2] = unchecked((byte)(intensity ? 12 : 4));
				machine.GraphicsArray.AttributeController.Registers[3] = unchecked((byte)(intensity ? 15 : 7));
				break;

			default: goto case 1;
		}
	}

	public void LoadEGAPalette()
	{
		var paletteBytes = machine.GraphicsArray.DAC.Palette.AsSpan();

		for (int i = 0; i < 64; i++)
		{
			int r = (i >> 0) & 0b11;
			int g = (i >> 2) & 0b11;
			int b = (i >> 4) & 0b11;

			r *= 0b10101;
			g *= 0b10101;
			b *= 0b10101;

			paletteBytes[0] = (byte)((b << 2) | (b >> 4));
			paletteBytes[1] = (byte)((g << 2) | (g >> 4));
			paletteBytes[2] = (byte)((r << 2) | (r >> 4));

			paletteBytes = paletteBytes.Slice(3);
		}

		machine.GraphicsArray.DAC.RebuildBGRA();

		machine.GraphicsArray.AttributeController.Registers[0x0] = 0x00;
		machine.GraphicsArray.AttributeController.Registers[0x1] = 0x01;
		machine.GraphicsArray.AttributeController.Registers[0x2] = 0x02;
		machine.GraphicsArray.AttributeController.Registers[0x3] = 0x03;
		machine.GraphicsArray.AttributeController.Registers[0x4] = 0x04;
		machine.GraphicsArray.AttributeController.Registers[0x5] = 0x05;
		machine.GraphicsArray.AttributeController.Registers[0x6] = 0x14; // Hello!
		machine.GraphicsArray.AttributeController.Registers[0x7] = 0x07;
		machine.GraphicsArray.AttributeController.Registers[0x8] = 0x38;
		machine.GraphicsArray.AttributeController.Registers[0x9] = 0x39;
		machine.GraphicsArray.AttributeController.Registers[0xA] = 0x3A;
		machine.GraphicsArray.AttributeController.Registers[0xB] = 0x3B;
		machine.GraphicsArray.AttributeController.Registers[0xC] = 0x3C;
		machine.GraphicsArray.AttributeController.Registers[0xD] = 0x3D;
		machine.GraphicsArray.AttributeController.Registers[0xE] = 0x3E;
		machine.GraphicsArray.AttributeController.Registers[0xF] = 0x3F;
	}

	public void LoadVGAPalette()
	{
		using (var stream = typeof(Video).Assembly.GetManifestResourceStream("QBX.Firmware.DefaultPalette.bin"))
		{
			if (stream == null)
				return; // ?

			stream.ReadExactly(machine.GraphicsArray.DAC.Palette);

			machine.GraphicsArray.DAC.RebuildBGRA();

			for (int i = 0; i < 16; i++)
				machine.GraphicsArray.AttributeController.Registers[i] = (byte)i;
		}
	}

	public byte[][] GetFontForCurrentMode()
		=> GetFont(machine.GraphicsArray.CRTController.CharacterHeight);

	public byte[][] GetFont(int characterScans)
	{

		string fontFileName = $"8x{characterScans}.bin";

		byte[][] fontData = new byte[256][];

		for (int i = 0; i < 256; i++)
			fontData[i] = new byte[characterScans];

		using (var stream = typeof(GraphicsArray).Assembly.GetManifestResourceStream("QBX.Firmware.Fonts." + fontFileName))
		{
			if (stream != null)
			{
				for (int ch = 0; ch < 256; ch++)
				{
					int baseOffset = ch * 32;

					byte[] glyph = fontData[ch];

					for (int y = 0; y < characterScans; y++)
						glyph[y] = unchecked((byte)stream.ReadByte());
				}
			}
		}

		return fontData;
	}

	public void LoadFontIntoCharacterGenerator(byte[][] font)
	{
		const int FontPlane = 0x20000;

		for (int ch = 0; ch < 256; ch++)
		{
			int baseOffset = ch * 32;

			Span<byte> glyph = font[ch];

			if (glyph.Length > 32)
				glyph = glyph.Slice(0, 32);

			for (int y = 0; y < glyph.Length; y++)
				machine.GraphicsArray.VRAM[FontPlane + baseOffset + y] = glyph[y];
		}
	}
}
