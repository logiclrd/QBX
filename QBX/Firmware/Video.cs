using QBX.Hardware;
using System;
using static QBX.Hardware.GraphicsArray;

namespace QBX.Firmware;

public partial class Video(Machine machine)
{
	public event Action<ModeParameters>? ModeChanged;


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

		array.OutPort(DACRegisters.MaskPort, 0xFF);

		switch (mode.PaletteType)
		{
			case PaletteType.CGA: LoadCGAPalette(intensity: true, reloadDAC: true); break;
			case PaletteType.EGA: LoadEGAPalette(); break;
			case PaletteType.VGA: LoadVGAPalette(); break;
		}

		array.OutPort2(
			SequencerRegisters.IndexPort,
			SequencerRegisters.Reset,
			0b11);

		ModeChanged?.Invoke(mode);

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

		int cursorStart = array.InPort2(CRTControllerRegisters.IndexPort, CRTControllerRegisters.CursorStart, out _);

		int cursorDisableBit = (cursorStart & CRTControllerRegisters.CursorStart_Disable);

		array.OutPort2(
			CRTControllerRegisters.IndexPort,
			CRTControllerRegisters.CursorStart,
			(byte)((maximumScanLine - 1) | cursorDisableBit));

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

	public void LoadCGAPalette(int cgaPalette = 1, bool intensity = false, bool reloadDAC = false)
	{
		if (reloadDAC)
		{
			var cgaColours = new byte[768];

			var cgaBytes = cgaColours.AsSpan();

			int o = 0;

			void RGB(int r, int g, int b)
			{
				cgaColours[o++] = unchecked((byte)r);
				cgaColours[o++] = unchecked((byte)g);
				cgaColours[o++] = unchecked((byte)b);
			}

			// Copied from DOSBox, it's probably doing the right thing.

			// The first 32 entries are copied straight out of the standard
			// EGA attribute mappings, repeated.
			RGB(0x00, 0x00, 0x00); RGB(0x00, 0x00, 0x2A); RGB(0x00, 0x2A, 0x00); RGB(0x00, 0x2A, 0x2A);
			RGB(0x2A, 0x00, 0x00); RGB(0x2A, 0x00, 0x2A); RGB(0x2A, 0x15, 0x00); RGB(0x2A, 0x2A, 0x2A);
			RGB(0x15, 0x15, 0x15); RGB(0x15, 0x15, 0x3F); RGB(0x15, 0x3F, 0x15); RGB(0x15, 0x3F, 0x3F);
			RGB(0x3F, 0x15, 0x15); RGB(0x3F, 0x15, 0x3F); RGB(0x3F, 0x3F, 0x15); RGB(0x3F, 0x3F, 0x3F);

			RGB(0x00, 0x00, 0x00); RGB(0x00, 0x00, 0x2A); RGB(0x00, 0x2A, 0x00); RGB(0x00, 0x2A, 0x2A);
			RGB(0x2A, 0x00, 0x00); RGB(0x2A, 0x00, 0x2A); RGB(0x2A, 0x15, 0x00); RGB(0x2A, 0x2A, 0x2A);
			RGB(0x15, 0x15, 0x15); RGB(0x15, 0x15, 0x3F); RGB(0x15, 0x3F, 0x15); RGB(0x15, 0x3F, 0x3F);
			RGB(0x3F, 0x15, 0x15); RGB(0x3F, 0x15, 0x3F); RGB(0x3F, 0x3F, 0x15); RGB(0x3F, 0x3F, 0x3F);

			// I couldn't discern a clear pattern for the remaining colours.
			RGB(0x3F, 0x1F, 0x1F); RGB(0x3F, 0x27, 0x1F); RGB(0x3F, 0x2F, 0x1F); RGB(0x3F, 0x37, 0x1F);
			RGB(0x3F, 0x3F, 0x1F); RGB(0x37, 0x3F, 0x1F); RGB(0x2F, 0x3F, 0x1F); RGB(0x27, 0x3F, 0x1F);
			RGB(0x1F, 0x3F, 0x1F); RGB(0x1F, 0x3F, 0x27); RGB(0x1F, 0x3F, 0x2F); RGB(0x1F, 0x3F, 0x37);
			RGB(0x1F, 0x3F, 0x3F); RGB(0x1F, 0x37, 0x3F); RGB(0x1F, 0x2F, 0x3F); RGB(0x1F, 0x27, 0x3F);

			RGB(0x2D, 0x2D, 0x3F); RGB(0x31, 0x2D, 0x3F); RGB(0x36, 0x2D, 0x3F); RGB(0x3A, 0x2D, 0x3F);
			RGB(0x3F, 0x2D, 0x3F); RGB(0x3F, 0x2D, 0x3A); RGB(0x3F, 0x2D, 0x36); RGB(0x3F, 0x2D, 0x31);
			RGB(0x3F, 0x2D, 0x2D); RGB(0x3F, 0x31, 0x2D); RGB(0x3F, 0x36, 0x2D); RGB(0x3F, 0x3A, 0x2D);
			RGB(0x3F, 0x3F, 0x2D); RGB(0x3A, 0x3F, 0x2D); RGB(0x36, 0x3F, 0x2D); RGB(0x31, 0x3F, 0x2D);

			RGB(0x2D, 0x3F, 0x2D); RGB(0x2D, 0x3F, 0x31); RGB(0x2D, 0x3F, 0x36); RGB(0x2D, 0x3F, 0x3A);
			RGB(0x2D, 0x3F, 0x3F); RGB(0x2D, 0x3A, 0x3F); RGB(0x2D, 0x36, 0x3F); RGB(0x2D, 0x31, 0x3F);
			RGB(0x00, 0x00, 0x1C); RGB(0x07, 0x00, 0x1C); RGB(0x0E, 0x00, 0x1C); RGB(0x15, 0x00, 0x1C);
			RGB(0x1C, 0x00, 0x1C); RGB(0x1C, 0x00, 0x15); RGB(0x1C, 0x00, 0x0E); RGB(0x1C, 0x00, 0x07);

			RGB(0x1C, 0x00, 0x00); RGB(0x1C, 0x07, 0x00); RGB(0x1C, 0x0E, 0x00); RGB(0x1C, 0x15, 0x00);
			RGB(0x1C, 0x1C, 0x00); RGB(0x15, 0x1C, 0x00); RGB(0x0E, 0x1C, 0x00); RGB(0x07, 0x1C, 0x00);
			RGB(0x00, 0x1C, 0x00); RGB(0x00, 0x1C, 0X07); RGB(0x00, 0x1C, 0x0E); RGB(0x00, 0x1C, 0x15);
			RGB(0x00, 0x1C, 0x1C); RGB(0x00, 0x15, 0x1C); RGB(0x00, 0x0E, 0x1C); RGB(0x00, 0x07, 0x1C);

			RGB(0x0E, 0x0E, 0x1C); RGB(0x11, 0x0E, 0x1C); RGB(0x15, 0x0E, 0x1C); RGB(0x18, 0x0E, 0x1C);
			RGB(0x1C, 0x0E, 0x1C); RGB(0x1C, 0x0E, 0x18); RGB(0x1C, 0x0E, 0x15); RGB(0x1C, 0x0E, 0x11);
			RGB(0x1C, 0x0E, 0x0E); RGB(0x1C, 0x11, 0x0E); RGB(0x1C, 0x15, 0x0E); RGB(0x1C, 0x18, 0x0E);
			RGB(0x1C, 0x1C, 0x0E); RGB(0x18, 0x1C, 0x0E); RGB(0x15, 0x1C, 0x0E); RGB(0x11, 0x1C, 0x0E);

			RGB(0x0E, 0x1C, 0x0E); RGB(0x0E, 0x1C, 0x11); RGB(0x0E, 0x1C, 0x15); RGB(0x0E, 0x1C, 0x18);
			RGB(0x0E, 0x1C, 0x1C); RGB(0x0E, 0x18, 0x1C); RGB(0x0E, 0x15, 0x1C); RGB(0x0E, 0x11, 0x1C);
			RGB(0x14, 0x14, 0x1C); RGB(0x16, 0x14, 0x1C); RGB(0x18, 0x14, 0x1C); RGB(0x1A, 0x14, 0x1C);
			RGB(0x1C, 0x14, 0x1C); RGB(0x1C, 0x14, 0x1A); RGB(0x1C, 0x14, 0x18); RGB(0x1C, 0x14, 0x16);

			RGB(0x1C, 0x14, 0x14); RGB(0x1C, 0x16, 0x14); RGB(0x1C, 0x18, 0x14); RGB(0x1C, 0x1A, 0x14);
			RGB(0x1C, 0x1C, 0x14); RGB(0x1A, 0x1C, 0x14); RGB(0x18, 0x1C, 0x14); RGB(0x16, 0x1C, 0x14);
			RGB(0x14, 0x1C, 0x14); RGB(0x14, 0x1C, 0x16); RGB(0x14, 0x1C, 0x18); RGB(0x14, 0x1C, 0x1A);
			RGB(0x14, 0x1C, 0x1C); RGB(0x14, 0x1A, 0x1C); RGB(0x14, 0x18, 0x1C); RGB(0x14, 0x16, 0x1C);

			RGB(0x00, 0x00, 0x10); RGB(0x04, 0x00, 0x10); RGB(0x08, 0x00, 0x10); RGB(0x0C, 0x00, 0x10);
			RGB(0x10, 0x00, 0x10); RGB(0x10, 0x00, 0x0C); RGB(0x10, 0x00, 0x08); RGB(0x10, 0x00, 0x04);
			RGB(0x10, 0x00, 0x00); RGB(0x10, 0x04, 0x00); RGB(0x10, 0x08, 0x00); RGB(0x10, 0x0C, 0x00);
			RGB(0x10, 0x10, 0x00); RGB(0x0C, 0x10, 0x00); RGB(0x08, 0x10, 0x00); RGB(0x04, 0x10, 0x00);

			RGB(0x00, 0x10, 0x00); RGB(0x00, 0x10, 0x04); RGB(0x00, 0x10, 0x08); RGB(0x00, 0x10, 0x0C);
			RGB(0x00, 0x10, 0x10); RGB(0x00, 0x0C, 0x10); RGB(0x00, 0x08, 0x10); RGB(0x00, 0x04, 0x10);
			RGB(0x08, 0x08, 0x10); RGB(0x0A, 0x08, 0x10); RGB(0x0C, 0x08, 0x10); RGB(0x0E, 0x08, 0x10);
			RGB(0x10, 0x08, 0x10); RGB(0x10, 0x08, 0x0E); RGB(0x10, 0x08, 0x0C); RGB(0x10, 0x08, 0x0A);

			RGB(0x10, 0x08, 0x08); RGB(0x10, 0x0A, 0x08); RGB(0x10, 0x0C, 0x08); RGB(0x10, 0x0E, 0x08);
			RGB(0x10, 0x10, 0x08); RGB(0x0E, 0x10, 0x08); RGB(0x0C, 0x10, 0x08); RGB(0x0A, 0x10, 0x08);
			RGB(0x08, 0x10, 0x08); RGB(0x08, 0x10, 0x0A); RGB(0x08, 0x10, 0x0C); RGB(0x08, 0x10, 0x0E);
			RGB(0x08, 0x10, 0x10); RGB(0x08, 0x0E, 0x10); RGB(0x08, 0x0C, 0x10); RGB(0x08, 0x0A, 0x10);

			RGB(0x0B, 0x0B, 0x10); RGB(0x0C, 0x0B, 0x10); RGB(0x0D, 0x0B, 0x10); RGB(0x0F, 0x0B, 0x10);
			RGB(0x10, 0x0B, 0x10); RGB(0x10, 0x0B, 0x0F); RGB(0x10, 0x0B, 0x0D); RGB(0x10, 0x0B, 0x0C);
			RGB(0x10, 0x0B, 0x0B); RGB(0x10, 0x0C, 0x0B); RGB(0x10, 0x0D, 0x0B); RGB(0x10, 0x0F, 0x3B);
			RGB(0x10, 0x10, 0x0B); RGB(0x0F, 0x10, 0x0B); RGB(0x0D, 0x10, 0x0B); RGB(0x0C, 0x10, 0x3B);

			RGB(0x0B, 0x10, 0x0B); RGB(0x0B, 0x10, 0x0C); RGB(0x0B, 0x10, 0x0D); RGB(0x0B, 0x10, 0x0F);
			RGB(0x0B, 0x10, 0x10); RGB(0x0B, 0x0F, 0x10); RGB(0x0B, 0x0D, 0x10); RGB(0x0B, 0x0C, 0x10);
			RGB(0x00, 0x00, 0x00); RGB(0x00, 0x00, 0x00); RGB(0x00, 0x00, 0x00); RGB(0x00, 0x00, 0x00);
			RGB(0x00, 0x00, 0x00); RGB(0x00, 0x00, 0x00); RGB(0x00, 0x00, 0x00); RGB(0x00, 0x00, 0x00);

			cgaBytes.CopyTo(machine.GraphicsArray.DAC.Palette);

			machine.GraphicsArray.DAC.RebuildBGRA();
		}

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

	void BuildEGAPalette(Span<byte> paletteBytes)
	{
		for (int i = 0; i < 64; i++)
		{
			int r = (i & 0b100100) >> 2;
			int g = (i & 0b010010) >> 1;
			int b = (i & 0b001001) >> 0;

			// 0bB00A => 0bAB
			r = (r >> 3) | ((r & 1) << 1);
			g = (g >> 3) | ((g & 1) << 1);
			b = (b >> 3) | ((b & 1) << 1);

			// 0b0000AB => 0bABABAB
			r *= 0b10101;
			g *= 0b10101;
			b *= 0b10101;

			paletteBytes[0] = (byte)r;
			paletteBytes[1] = (byte)g;
			paletteBytes[2] = (byte)b;

			paletteBytes = paletteBytes.Slice(3);
		}
	}

	public void LoadEGAPalette()
	{
		BuildEGAPalette(machine.GraphicsArray.DAC.Palette.AsSpan());

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
		=> TryGetFont(characterScans) ?? throw new ArgumentException("No font found for character scan count " + characterScans);

	public byte[][]? TryGetFont(int characterScans)
	{
		string fontFileName = $"8x{characterScans}.bin";

		using (var stream = typeof(GraphicsArray).Assembly.GetManifestResourceStream("QBX.Firmware.Fonts." + fontFileName))
		{
			if (stream == null)
				return null;

			byte[][] fontData = new byte[256][];

			for (int i = 0; i < 256; i++)
				fontData[i] = new byte[characterScans];

			for (int ch = 0; ch < 256; ch++)
			{
				int baseOffset = ch * 32;

				byte[] glyph = fontData[ch];

				for (int y = 0; y < characterScans; y++)
					glyph[y] = unchecked((byte)stream.ReadByte());
			}

			return fontData;
		}
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
