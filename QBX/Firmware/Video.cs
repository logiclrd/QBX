using System;

using QBX.Hardware;

using static QBX.Hardware.GraphicsArray;

namespace QBX.Firmware;

public class Video(Machine machine)
{
	enum BaseAddress
	{
		A000,
		B800,
	}

	enum CharacterWidth
	{
		_8 = 8,
		_9 = 9,
	}

	enum PaletteType
	{
		CGA,
		VGA,
	}

	class ModeParameters
	{
		public int? ScreenNumber; // QB
		public (int Width, int Height) Pixels;
		public (int Width, int Height) Characters;
		public bool IsGraphicsMode;
		public bool IsMonochrome;
		public PaletteType PaletteType;
		public BaseAddress BaseAddress;
		public CharacterWidth CharacterWidth;
		public int CharacterHeight;
		public int PixelsPerAddress = 1;
		public int MemoryAddressSize;
		public bool ScanDoubling;
		public bool OddEvenAddressing;
		public bool ShiftRegisterInterleave;
		public bool Use256Colours;
		public byte PlaneMask;
		public bool HalfDotClockRate;
		public bool Chain4Mode;
	}

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
			BaseAddress = BaseAddress.B800,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 8,
			PixelsPerAddress = 8,
			MemoryAddressSize = 1,
			ScanDoubling = true,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0,
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
			BaseAddress = BaseAddress.B800,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 8,
			PixelsPerAddress = 8,
			MemoryAddressSize = 1,
			ScanDoubling = true,
			OddEvenAddressing = false,
			ShiftRegisterInterleave = false,
			Use256Colours = false,
			PlaneMask = 0,
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
			BaseAddress = BaseAddress.B800,
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
			BaseAddress = BaseAddress.B800,
			CharacterWidth = CharacterWidth._8,
			CharacterHeight = 14,
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

	static ModeParameters?[] Modes =
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

	public void SetMode(int modeNumber)
	{
		if ((modeNumber >= 0) && (modeNumber < Modes.Length))
		{
			var mode = Modes[modeNumber];

			if (mode == null)
				return;

			var array = machine.GraphicsArray;

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

				array.ResetFont();
			}

			switch (mode.PaletteType)
			{
				case PaletteType.CGA: array.LoadCGAPalette(); break;
				case PaletteType.VGA: array.LoadVGAPalette(); break;
			}

			array.OutPort2(
				SequencerRegisters.IndexPort,
				SequencerRegisters.Reset,
				0b11);
		}
		else
			throw new NotSupportedException("Not supported: video mode 0x" + modeNumber.ToString("X"));
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

		array.ResetFont();
	}

	public void SetCharacterWidth(int width)
	{
		byte clock;
		byte characterWidth;

		switch (width)
		{
			case 8:
				clock = GraphicsArray.MiscellaneousOutputRegisters.Clock_25MHz;
				characterWidth = GraphicsArray.SequencerRegisters.ClockingMode_CharacterWidth_8;
				break;
			case 9:
				clock = GraphicsArray.MiscellaneousOutputRegisters.Clock_28MHz;
				characterWidth = GraphicsArray.SequencerRegisters.ClockingMode_CharacterWidth_9;
				break;

			default:
				throw new InvalidOperationException();
		}

		var array = machine.GraphicsArray;

		array.MiscellaneousOutput.Register = unchecked((byte)(
			(array.MiscellaneousOutput.Register & ~GraphicsArray.MiscellaneousOutputRegisters.ClockMask) |
			clock));

		array.Sequencer.Registers[GraphicsArray.SequencerRegisters.ClockingMode] = unchecked((byte)(
			(array.Sequencer.Registers[GraphicsArray.SequencerRegisters.ClockingMode]
				& ~GraphicsArray.SequencerRegisters.ClockingMode_CharacterWidthMask) |
			characterWidth));
	}
}
