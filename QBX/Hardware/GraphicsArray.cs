using System;
using System.Runtime.CompilerServices;

namespace QBX.Hardware;

public class GraphicsArray : IMemory
{
	public byte[] VRAM = new byte[256 * 1024];

	public int Length => VRAM.Length;

	public class GraphicsRegisters
	{
		public const int IndexPort = 0x3CE;
		public const int DataPort = IndexPort + 1;

		public const int SetReset = 0;
		public const int EnableSetReset = 1;
		public const int ColourCompare = 2;
		public const int DataRotate = 3;
		public const int ReadMapSelect = 4;
		public const int GraphicsMode = 5;
		public const int MiscGraphics = 6;
		public const int ColourDoNotCare = 7;
		public const int BitMask = 8;

		public const byte SetReset_Plane0 = 1;
		public const byte SetReset_Plane1 = 2;
		public const byte SetReset_Plane2 = 4;
		public const byte SetReset_Plane3 = 8;

		public const byte EnableSetReset_Plane0 = 1;
		public const byte EnableSetReset_Plane1 = 2;
		public const byte EnableSetReset_Plane2 = 4;
		public const byte EnableSetReset_Plane3 = 8;

		public const byte DataRotate_OperationMask = 24;
		public const byte DataRotate_RotateMask = 7;

		public const byte DataRotate_Operation_Copy = 0;
		public const byte DataRotate_Operation_AND = 8;
		public const byte DataRotate_Operation_OR = 16;
		public const byte DataRotate_Operation_XOR = 24;

		public const byte GraphicsMode_Shift256 = 64;
		public const byte GraphicsMode_ShiftInterleave = 32;
		public const byte GraphicsMode_HostOddEvenRead = 16;
		public const byte GraphicsMode_ReadCompare = 8;
		public const byte GraphicsMode_WriteModeMask = 3;

		public const byte MiscGraphics_MemoryMapMask = 12;
		public const byte MiscGraphics_MemoryMap_Flat128K = 0;
		public const byte MiscGraphics_MemoryMap_Graphics64K = 4;
		public const byte MiscGraphics_MemoryMap_MonoText32K = 8;
		public const byte MiscGraphics_MemoryMap_ColourText32K = 12;
		public const byte MiscGraphics_ChainOddEven = 2;
		public const byte MiscGraphics_AlphanumericModeDisable = 1;

		public int MemoryMapBaseAddress;
		public int MemoryMapReadOffset;
		public int MemoryMapSize;
		public bool DisableText;
		public bool Shift256;
		public bool ShiftInterleave;
		public bool HostOddEvenRead;

		public readonly RegisterSet Registers;

		public GraphicsRegisters() { Registers = new(this); }

		public class RegisterSet(GraphicsRegisters owner)
		{
			public byte SetReset = 0;
			public byte EnableSetReset = 0;
			public byte ColourCompare = 0;
			public byte DataRotate = 0;
			public byte ReadMapSelect = 0;
			public byte GraphicsMode = 0;
			public byte MiscGraphics = MiscGraphics_MemoryMap_ColourText32K;
			public byte ColourDoNotCare = 0;
			public byte BitMask = 0;

			public byte this[int index]
			{
				get
				{
					switch (index)
					{
						case GraphicsRegisters.SetReset: return SetReset;
						case GraphicsRegisters.EnableSetReset: return EnableSetReset;
						case GraphicsRegisters.ColourCompare: return ColourCompare;
						case GraphicsRegisters.DataRotate: return DataRotate;
						case GraphicsRegisters.ReadMapSelect: return ReadMapSelect;
						case GraphicsRegisters.GraphicsMode: return GraphicsMode;
						case GraphicsRegisters.MiscGraphics: return MiscGraphics;
						case GraphicsRegisters.ColourDoNotCare: return ColourDoNotCare;
						case GraphicsRegisters.BitMask: return BitMask;

						default: return 0;
					}
				}
				set
				{
					switch (index)
					{
						case GraphicsRegisters.SetReset: SetReset = value; break;
						case GraphicsRegisters.EnableSetReset: EnableSetReset = value; break;
						case GraphicsRegisters.ColourCompare: ColourCompare = value; break;
						case GraphicsRegisters.DataRotate: DataRotate = value; break;
						case GraphicsRegisters.ReadMapSelect: ReadMapSelect = value; break;
						case GraphicsRegisters.GraphicsMode: GraphicsMode = value; break;
						case GraphicsRegisters.MiscGraphics: MiscGraphics = value; break;
						case GraphicsRegisters.ColourDoNotCare: ColourDoNotCare = value; break;
						case GraphicsRegisters.BitMask: BitMask = value; break;
					}

					owner.MemoryMapBaseAddress =
						(MiscGraphics & MiscGraphics_MemoryMapMask) switch
						{
							MiscGraphics_MemoryMap_Flat128K => 0xA0000,
							MiscGraphics_MemoryMap_Graphics64K => 0xA0000,
							MiscGraphics_MemoryMap_MonoText32K => 0xB0000,
							MiscGraphics_MemoryMap_ColourText32K => 0xB8000,

							_ => throw new Exception("Internal error")
						};

					owner.MemoryMapReadOffset = ReadMapSelect * 65536;

					owner.MemoryMapSize =
						(MiscGraphics & MiscGraphics_MemoryMapMask) switch
						{
							MiscGraphics_MemoryMap_Flat128K => 131072,
							MiscGraphics_MemoryMap_Graphics64K => 65536,
							MiscGraphics_MemoryMap_MonoText32K => 32768,
							MiscGraphics_MemoryMap_ColourText32K => 32768,

							_ => throw new Exception("Internal error")
						};

					owner.DisableText = ((MiscGraphics & MiscGraphics_AlphanumericModeDisable) != 0);

					owner.Shift256 = ((GraphicsMode & GraphicsMode_Shift256) != 0);
					owner.ShiftInterleave = ((GraphicsMode & GraphicsMode_ShiftInterleave) != 0);
					owner.HostOddEvenRead = ((GraphicsMode & GraphicsMode_HostOddEvenRead) != 0);
				}
			}
		}
	}

	public class SequencerRegisters
	{
		public const int IndexPort = 0x3C4;
		public const int DataPort = IndexPort + 1;

		public const int Reset = 0;
		public const int ClockingMode = 1;
		public const int MapMask = 2;
		public const int CharacterSet = 3;
		public const int SequencerMemoryMode = 4;

		public const byte Reset_SynchronousWhenCleared = 2;
		public const byte Reset_AsynchronousWhenCleared = 1;

		public const byte ClockingMode_ScreenDisable = 32;
		public const byte ClockingMode_Shift4 = 16;
		public const byte ClockingMode_DotClockHalfRate = 8;
		public const byte ClockingMode_ShiftLoadRate = 4;
		public const byte ClockingMode_CharacterWidthMask = 1;
		public const byte ClockingMode_CharacterWidth_9 = 0;
		public const byte ClockingMode_CharacterWidth_8 = 1;

		public const byte MapMask_Plane0 = 1;
		public const byte MapMask_Plane1 = 2;
		public const byte MapMask_Plane2 = 4;
		public const byte MapMask_Plane3 = 8;

		public const byte CharacterSet_AMask = 3;
		public const byte CharacterSet_AShift = 0;
		public const byte CharacterSet_AMaskHigh = 16;
		public const byte CharacterSet_BMask = 12;
		public const byte CharacterSet_BShift = 2;
		public const byte CharacterSet_BMaskHigh = 32;

		public const byte SequencerMemoryMode_Chain4 = 8;
		public const byte SequencerMemoryMode_OddEvenDisable = 4;
		public const byte SequencerMemoryMode_ExtendedMemory = 2;

		public bool Plane0WriteEnable;
		public bool Plane1WriteEnable;
		public bool Plane2WriteEnable;
		public bool Plane3WriteEnable;
		public int CharacterWidth;
		public bool DotDoubling;
		public int CharacterSetAOffset;
		public int CharacterSetBOffset;
		public bool HostOddEvenWrite;

		public readonly RegisterSet Registers;

		public SequencerRegisters() { Registers = new RegisterSet(this); }

		public class RegisterSet(SequencerRegisters owner)
		{
			public byte Reset = Reset_SynchronousWhenCleared | Reset_AsynchronousWhenCleared;
			public byte ClockingMode = 0;
			public byte MapMask = MapMask_Plane0 | MapMask_Plane1;
			public byte CharacterSet = 0;
			public byte SequencerMemoryMode = SequencerMemoryMode_Chain4;

			public byte this[int index]
			{
				get
				{
					switch (index)
					{
						case SequencerRegisters.Reset: return Reset;
						case SequencerRegisters.ClockingMode: return ClockingMode;
						case SequencerRegisters.MapMask: return MapMask;
						case SequencerRegisters.CharacterSet: return CharacterSet;
						case SequencerRegisters.SequencerMemoryMode: return SequencerMemoryMode;

						default: return 0;
					}
				}
				set
				{
					switch (index)
					{
						case SequencerRegisters.Reset: Reset = value; break;
						case SequencerRegisters.ClockingMode: ClockingMode = value; break;
						case SequencerRegisters.MapMask: MapMask = value; break;
						case SequencerRegisters.CharacterSet: CharacterSet = value; break;
						case SequencerRegisters.SequencerMemoryMode: SequencerMemoryMode = value; break;
					}

					owner.Plane0WriteEnable = ((MapMask & MapMask_Plane0) != 0);
					owner.Plane1WriteEnable = ((MapMask & MapMask_Plane1) != 0);
					owner.Plane2WriteEnable = ((MapMask & MapMask_Plane2) != 0);
					owner.Plane3WriteEnable = ((MapMask & MapMask_Plane3) != 0);

					owner.CharacterWidth = ((ClockingMode & ClockingMode_CharacterWidthMask) == ClockingMode_CharacterWidth_9)
						? 9
						: 8;

					owner.DotDoubling = ((ClockingMode & ClockingMode_DotClockHalfRate) != 0);

					int characterSetA =
						((CharacterSet & CharacterSet_AMask) >> CharacterSet_AShift << 1) |
						(((CharacterSet & CharacterSet_AMaskHigh) != 0) ? 1 : 0);
					int characterSetB =
						((CharacterSet & CharacterSet_BMask) >> CharacterSet_BShift << 1) |
						(((CharacterSet & CharacterSet_BMaskHigh) != 0) ? 1 : 0);

					owner.CharacterSetAOffset = characterSetA * 0x2000;
					owner.CharacterSetBOffset = characterSetB * 0x2000;

					owner.HostOddEvenWrite = ((SequencerMemoryMode & SequencerMemoryMode_OddEvenDisable) == 0);
				}
			}
		}
	}

	public class DACRegisters
	{
		public const int ReadIndexPort = 0x3C7;
		public const int WriteIndexPort = 0x3C8;
		public const int DataPort = 0x3C9;

		public byte[] Palette = new byte[768];
		public byte[] PaletteBGRA = new byte[1024];

		public void RebuildBGRA()
		{
			for (int i = 0; i < 256; i++)
				RebuildBGRA(i);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void RebuildBGRA(int paletteIndex)
		{
			int sourcePaletteOffset = paletteIndex * 3;
			int destPaletteOffset = paletteIndex * 4;

			int r = Palette[sourcePaletteOffset + 0];
			int g = Palette[sourcePaletteOffset + 1];
			int b = Palette[sourcePaletteOffset + 2];

			r = (r << 2) | (r >> 4);
			g = (g << 2) | (g >> 4);
			b = (b << 2) | (b >> 4);

			unchecked
			{
				PaletteBGRA[destPaletteOffset + 0] = 255;
				PaletteBGRA[destPaletteOffset + 1] = (byte)r;
				PaletteBGRA[destPaletteOffset + 2] = (byte)g;
				PaletteBGRA[destPaletteOffset + 3] = (byte)b;
			}
		}
	}

	public class MiscellaneousOutputRegisters
	{
		public const int ReadPort = 0x3CC;
		public const int WritePort = 0x3C2;

		public const byte VerticalSyncPolarity = 128;
		public const byte HorizontalSyncPolarity = 64;
		public const byte SelectOddPage = 32;
		public const byte ClockMask = 12;
		public const byte Clock_25MHz = 0;
		public const byte Clock_28MHz = 4;
		public const byte RAMEnable = 2;
		public const byte IOAddress = 1;

		byte _register = Clock_28MHz | RAMEnable | IOAddress;

		public int BasePixelWidth = 720;
		public bool IsRAMEnabled = true;
		public bool UseMonoIOPorts = false;

		public byte Register
		{
			get { return _register; }
			set
			{
				_register = value;

				BasePixelWidth =
					(value & ClockMask) == Clock_28MHz
					? 720
					: 640;

				IsRAMEnabled = (value & RAMEnable) != 0;

				UseMonoIOPorts = (value & IOAddress) == 0;
			}
		}
	}

	public class InputStatusRegisters
	{
		public const int InputStatus0Port = 0x3C2;
		public const int InputStatus1Port = 0x3DA;
	}

	public class CRTControllerRegisters
	{
		public const int IndexPort = 0x3D4;
		public const int DataPort = 0x3D5;

		public const int HorizontalTotal = 0;
		public const int EndHorizontalDisplay = 1;
		public const int StartHorizontalBlanking = 2;
		public const int EndHorizontalBlanking = 3;
		public const int StartHorizontalRetrace = 4;
		public const int EndHorizontalRetrace = 5;
		public const int VerticalTotal = 6;
		public const int Overflow = 7;
		public const int PresetRowScan = 8;
		public const int MaximumScanLine = 9;
		public const int CursorStart = 10;
		public const int CursorEnd = 11;
		public const int StartAddressHigh = 12;
		public const int StartAddressLow = 13;
		public const int CursorLocationHigh = 14;
		public const int CursorLocationLow = 15;
		public const int VerticalRetraceStart = 16;
		public const int VerticalRetraceEnd = 17;
		public const int VerticalDisplayEnd = 18;
		public const int Offset = 19;
		public const int UnderlineLocation = 20;
		public const int StartVerticalBlanking = 21;
		public const int EndVerticalBlanking = 22;
		public const int ModeControl = 23;
		public const int LineCompare = 24;

		public const byte EndHorizontalBlanking_EnableVerticalRetraceAccess = 128;
		public const byte EndHorizontalBlanking_EndMask = 31;

		public const byte EndHorizontalRetrace_Blanking = 128;
		public const byte EndHorizontalRetrace_EndMask = 31;

		public const byte Overflow_VerticalRetraceStart9 = 128;
		public const byte Overflow_VerticalDisplayEnd9 = 64;
		public const byte Overflow_VerticalTotal9 = 32;
		public const byte Overflow_LineCompare8 = 16;
		public const byte Overflow_StartVerticalBlanking8 = 8;
		public const byte Overflow_VerticalRetraceStart8 = 4;
		public const byte Overflow_VerticalDisplayEnd8 = 2;
		public const byte Overflow_VerticalTotal8 = 1;

		public const byte PresetRowScan_BytePanningMask = 96;
		public const byte PresetRowScan_BytePanningShift = 5;
		public const byte PresetRowScan_ScanMask = 31;

		public const byte MaximumScanLine_ScanDoubling = 128;
		public const byte MaximumScanLine_LineCompareBit9 = 64;
		public const byte MaximumScanLine_StartVerticalBlankingBit9 = 32;
		public const byte MaximumScanLine_ScanMask = 15;

		public const byte CursorStart_Disable = 32;
		public const byte CursorStart_Mask = 31;

		public const byte CursorEnd_SkewMask = 96;
		public const byte CursorEnd_SkewShift = 5;
		public const byte CursorEnd_Mask = 31;

		public const byte VerticalRetraceEnd_Protect = 128;
		public const byte VerticalRetraceEnd_Bandwidth = 64;
		public const byte VerticalRetraceEnd_EndMask = 15;

		public const byte UnderlineLocation_DoubleWordAddress = 64;
		public const byte UnderlineLocation_DivideMemoryAddressBy4 = 32;
		public const byte UnderlineLocation_Mask = 31;

		public const byte EndVerticalBlanking_Mask = 127;

		public const byte ModeControl_SyncEnable = 128;
		public const byte ModeControl_ByteAddressing = 64;
		public const byte ModeControl_AddressWrap = 32;
		public const byte ModeControl_HalfCharacterClock = 8;
		public const byte ModeControl_HalfScanClock = 4;
		public const byte ModeControl_MapDisplayAddress14Mask = 2;
		public const byte ModeControl_MapDisplayAddress14_RowScan1 = 0;
		public const byte ModeControl_MapDisplayAddress14_Address14 = 2;
		public const byte ModeControl_MapDisplayAddress13Mask = 1;
		public const byte ModeControl_MapDisplayAddress13_RowScan0 = 0;
		public const byte ModeControl_MapDisplayAddress13_Address13 = 1;

		public int NumColumns;

		public int NumScanLines;
		public int SkipScans;
		public int ScanRepeatCount;
		public int CharacterHeight;
		public bool ScanDoubling;
		public bool CursorVisible;
		public int CursorScanStart;
		public int CursorScanEnd;
		public int StartAddress;
		public int CursorAddress;
		public int Stride;
		public int UnderlineCharacterRow;
		public bool InterleaveOnBit0;
		public bool InterleaveOnBit1;

		// In text modes, ScanRepeatCount is the height of the character box. The same scan is repeated for each
		// separate scan of the characters in the row. In graphics mode, there is no font being translated, so
		// the repeated scan is just literally repeated. If ScanDoubling is enabled, then each individual row of
		// the text font is independently repeated, if applicable (otherwise the scan is simply repeated
		// ScanRepeatCount * 2 times).

		public readonly RegisterSet Registers;

		public CRTControllerRegisters() { Registers = new RegisterSet(this); }

		public class RegisterSet(CRTControllerRegisters owner)
		{
			public byte HorizontalTotal = 75;
			public byte EndHorizontalDisplay = 0;
			public byte StartHorizontalBlanking = 0;
			public byte EndHorizontalBlanking = 0;
			public byte StartHorizontalRetrace = 0;
			public byte EndHorizontalRetrace = 0;
			public byte VerticalTotal = 0;
			public byte Overflow = 0;
			public byte PresetRowScan = 0;
			public byte MaximumScanLine = 0;
			public byte CursorStart = 0;
			public byte CursorEnd = 0;
			public byte StartAddressHigh = 0;
			public byte StartAddressLow = 0;
			public byte CursorLocationHigh = 0;
			public byte CursorLocationLow = 0;
			public byte VerticalRetraceStart = 0;
			public byte VerticalRetraceEnd = 0;
			public byte VerticalDisplayEnd = 0;
			public byte Offset = 0;
			public byte UnderlineLocation = 0;
			public byte StartVerticalBlanking = 0;
			public byte EndVerticalBlanking = 0;
			public byte ModeControl = 0;
			public byte LineCompare = 0;

			public byte this[int index]
			{
				get
				{
					switch (index)
					{
						case 0: return HorizontalTotal;
						case 1: return EndHorizontalDisplay;
						case 2: return StartHorizontalBlanking;
						case 3: return EndHorizontalBlanking;
						case 4: return StartHorizontalRetrace;
						case 5: return EndHorizontalRetrace;
						case 6: return VerticalTotal;
						case 7: return Overflow;
						case 8: return PresetRowScan;
						case 9: return MaximumScanLine;
						case 10: return CursorStart;
						case 11: return CursorEnd;
						case 12: return StartAddressHigh;
						case 13: return StartAddressLow;
						case 14: return CursorLocationHigh;
						case 15: return CursorLocationLow;
						case 16: return VerticalRetraceStart;
						case 17: return VerticalRetraceEnd;
						case 18: return VerticalDisplayEnd;
						case 19: return Offset;
						case 20: return UnderlineLocation;
						case 21: return StartVerticalBlanking;
						case 22: return EndVerticalBlanking;
						case 23: return ModeControl;
						case 24: return LineCompare;

						default: return 0;
					}
				}
				set
				{
					switch (index)
					{
						case 0: HorizontalTotal = value; break;
						case 1: EndHorizontalDisplay = value; break;
						case 2: StartHorizontalBlanking = value; break;
						case 3: EndHorizontalBlanking = value; break;
						case 4: StartHorizontalRetrace = value; break;
						case 5: EndHorizontalRetrace = value; break;
						case 6: VerticalTotal = value; break;
						case 7: Overflow = value; break;
						case 8: PresetRowScan = value; break;
						case 9: MaximumScanLine = value; break;
						case 10: CursorStart = value; break;
						case 11: CursorEnd = value; break;
						case 12: StartAddressHigh = value; break;
						case 13: StartAddressLow = value; break;
						case 14: CursorLocationHigh = value; break;
						case 15: CursorLocationLow = value; break;
						case 16: VerticalRetraceStart = value; break;
						case 17: VerticalRetraceEnd = value; break;
						case 18: VerticalDisplayEnd = value; break;
						case 19: Offset = value; break;
						case 20: UnderlineLocation = value; break;
						case 21: StartVerticalBlanking = value; break;
						case 22: EndVerticalBlanking = value; break;
						case 23: ModeControl = value; break;
						case 24: LineCompare = value; break;
					}

					int verticalDisplayEndValue = VerticalDisplayEnd | ((Overflow & 2) << 7) | ((Overflow & 64) << 3);

					owner.NumColumns = HorizontalTotal + 5;
					owner.NumScanLines = verticalDisplayEndValue + 1;
					owner.SkipScans = PresetRowScan & PresetRowScan_ScanMask;
					owner.ScanRepeatCount = MaximumScanLine & MaximumScanLine_ScanMask;
					owner.CharacterHeight = owner.ScanRepeatCount + 1;
					owner.ScanDoubling = (MaximumScanLine & MaximumScanLine_ScanDoubling) != 0;
					owner.StartAddress = (StartAddressLow << 2) | (StartAddressHigh << 10);
					owner.CursorVisible = (CursorStart & CursorStart_Disable) == 0;
					owner.CursorScanStart = CursorStart & CursorStart_Mask;
					owner.CursorScanEnd = CursorEnd & CursorEnd_Mask;
					owner.CursorAddress = CursorLocationLow | (CursorLocationHigh << 8);
					owner.Stride = Offset * 4;
					owner.UnderlineCharacterRow = UnderlineLocation & UnderlineLocation;
					owner.InterleaveOnBit0 = ((ModeControl & ModeControl_MapDisplayAddress13Mask) == ModeControl_MapDisplayAddress13_RowScan0);
					owner.InterleaveOnBit1 = ((ModeControl & ModeControl_MapDisplayAddress14Mask) == ModeControl_MapDisplayAddress14_RowScan1);
				}
			}
		}
	}

	public class AttributeControllerRegisters
	{
		public const int IndexAndDataWritePort = 0x3C0;
		public const int DataReadPort = 0x3C1;

		public const int Attribute0 = 0;
		public const int Attribute1 = 1;
		public const int Attribute2 = 2;
		public const int Attribute3 = 3;
		public const int Attribute4 = 4;
		public const int Attribute5 = 5;
		public const int Attribute6 = 6;
		public const int Attribute7 = 7;
		public const int Attribute8 = 8;
		public const int Attribute9 = 9;
		public const int Attribute10 = 10;
		public const int Attribute11 = 11;
		public const int Attribute12 = 12;
		public const int Attribute13 = 13;
		public const int Attribute14 = 14;
		public const int Attribute15 = 15;
		public const int ModeControl = 16;
		public const int OverscanPaletteIndex = 17;
		public const int ColourPlaneEnable = 18;
		public const int HorizontalPixelPanning = 19;
		public const int ColourSelect = 20;

		public const byte ModeControl_PaletteBits54Select = 128;
		public const byte ModeControl_8bitColour = 64;
		public const byte ModeControl_PixelPanningMode = 32;
		public const byte ModeControl_BlinkEnable = 8;
		public const byte ModeControl_LineGraphicsEnable = 4;
		public const byte ModeControl_MonochromeEmulation = 2;
		public const byte ModeControl_GraphicsMode = 1;

		public const byte ColourPlaneEnable_Plane3 = 8;
		public const byte ColourPlaneEnable_Plane2 = 4;
		public const byte ColourPlaneEnable_Plane1 = 2;
		public const byte ColourPlaneEnable_Plane0 = 1;

		public const byte HorizontalPixelPanningMask = 15;

		public const byte ColourSelect_Bits76 = 8 | 4;
		public const int ColourSelect_Bits76Shift = 2;
		public const byte ColourSelect_Bits54 = 2 | 1;
		public const int ColourSelect_Bits54Shift = 0;

		public bool LineGraphics;
		public bool Use256Colours;
		public bool EnableBlinking;
		public int AttributeBits76;
		public int AttributeBits54;
		public bool OverrideAttributeBits54;

		public readonly RegisterSet Registers;

		public AttributeControllerRegisters() { Registers = new(this); }

		public class RegisterSet(AttributeControllerRegisters owner)
		{
			public byte[] Attribute = new byte[16];
			public byte ModeControl;
			public byte OverscanPaletteIndex;
			public byte ColourPlaneEnable;
			public byte HorizontalPixelPanning;
			public byte ColourSelect;

			public byte this[int index]
			{
				get
				{
					switch (index)
					{
						case AttributeControllerRegisters.Attribute0: return Attribute[0];
						case AttributeControllerRegisters.Attribute1: return Attribute[1];
						case AttributeControllerRegisters.Attribute2: return Attribute[2];
						case AttributeControllerRegisters.Attribute3: return Attribute[3];
						case AttributeControllerRegisters.Attribute4: return Attribute[4];
						case AttributeControllerRegisters.Attribute5: return Attribute[5];
						case AttributeControllerRegisters.Attribute6: return Attribute[6];
						case AttributeControllerRegisters.Attribute7: return Attribute[7];
						case AttributeControllerRegisters.Attribute8: return Attribute[8];
						case AttributeControllerRegisters.Attribute9: return Attribute[9];
						case AttributeControllerRegisters.Attribute10: return Attribute[10];
						case AttributeControllerRegisters.Attribute11: return Attribute[11];
						case AttributeControllerRegisters.Attribute12: return Attribute[12];
						case AttributeControllerRegisters.Attribute13: return Attribute[13];
						case AttributeControllerRegisters.Attribute14: return Attribute[14];
						case AttributeControllerRegisters.Attribute15: return Attribute[15];
						case AttributeControllerRegisters.ModeControl: return ModeControl;
						case AttributeControllerRegisters.OverscanPaletteIndex: return OverscanPaletteIndex;
						case AttributeControllerRegisters.HorizontalPixelPanning: return HorizontalPixelPanning;
						case AttributeControllerRegisters.ColourSelect: return ColourSelect;
					}

					return 0;
				}
				set
				{
					switch (index)
					{
						case AttributeControllerRegisters.Attribute0: Attribute[0] = value; break;
						case AttributeControllerRegisters.Attribute1: Attribute[1] = value; break;
						case AttributeControllerRegisters.Attribute2: Attribute[2] = value; break;
						case AttributeControllerRegisters.Attribute3: Attribute[3] = value; break;
						case AttributeControllerRegisters.Attribute4: Attribute[4] = value; break;
						case AttributeControllerRegisters.Attribute5: Attribute[5] = value; break;
						case AttributeControllerRegisters.Attribute6: Attribute[6] = value; break;
						case AttributeControllerRegisters.Attribute7: Attribute[7] = value; break;
						case AttributeControllerRegisters.Attribute8: Attribute[8] = value; break;
						case AttributeControllerRegisters.Attribute9: Attribute[9] = value; break;
						case AttributeControllerRegisters.Attribute10: Attribute[10] = value; break;
						case AttributeControllerRegisters.Attribute11: Attribute[11] = value; break;
						case AttributeControllerRegisters.Attribute12: Attribute[12] = value; break;
						case AttributeControllerRegisters.Attribute13: Attribute[13] = value; break;
						case AttributeControllerRegisters.Attribute14: Attribute[14] = value; break;
						case AttributeControllerRegisters.Attribute15: Attribute[15] = value; break;
						case AttributeControllerRegisters.ModeControl: ModeControl = value; break;
						case AttributeControllerRegisters.OverscanPaletteIndex: OverscanPaletteIndex = value; break;
						case AttributeControllerRegisters.HorizontalPixelPanning: HorizontalPixelPanning = value; break;
						case AttributeControllerRegisters.ColourSelect: ColourSelect = value; break;
					}

					owner.LineGraphics = ((ModeControl & ModeControl_LineGraphicsEnable) != 0);
					owner.Use256Colours = ((ModeControl & ModeControl_8bitColour) != 0);
					owner.EnableBlinking = ((ModeControl & ModeControl_BlinkEnable) != 0);
					owner.OverrideAttributeBits54 = ((ModeControl & ModeControl_PaletteBits54Select) != 0);
					owner.AttributeBits76 = ((ColourSelect & ColourSelect_Bits76) >> ColourSelect_Bits76Shift) << 6;
					owner.AttributeBits54 = ((ColourSelect & ColourSelect_Bits54) >> ColourSelect_Bits54Shift) << 6;
				}
			}
		}
	}

	public readonly GraphicsRegisters Graphics = new GraphicsRegisters();
	public readonly SequencerRegisters Sequencer = new SequencerRegisters();
	public readonly MiscellaneousOutputRegisters MiscellaneousOutput = new MiscellaneousOutputRegisters();
	public readonly CRTControllerRegisters CRTController = new CRTControllerRegisters();
	public readonly AttributeControllerRegisters AttributeController = new AttributeControllerRegisters();
	public readonly DACRegisters DAC = new DACRegisters();

	int _graphicsIndex;
	int _sequencerIndex;
	int _crtControllerIndex;
	int _attributeControllerIndex;
	int _dacReadIndex;
	int _dacWriteIndex;
	AddressAndDataPortMode _attributeControllerPortMode;

	enum AddressAndDataPortMode
	{
		Address = 0,
		Data = 1,
	}

	public void OutPort(int portNumber, byte data)
	{
		if ((portNumber >= 0x3D4) && (portNumber <= 0x3DA) && MiscellaneousOutput.UseMonoIOPorts)
			return;

		if ((portNumber >= 0x3B4) && (portNumber <= 0x3BA))
		{
			if (MiscellaneousOutput.UseMonoIOPorts)
				portNumber += 0x20;
			else
				return;
		}

		switch (portNumber)
		{
			case MiscellaneousOutputRegisters.WritePort:
			{
				MiscellaneousOutput.Register = data;
				break;
			}
			case SequencerRegisters.IndexPort:
			{
				_sequencerIndex = data;
				break;
			}
			case SequencerRegisters.DataPort:
			{
				Sequencer.Registers[_sequencerIndex] = data;
				break;
			}
			case GraphicsRegisters.IndexPort:
			{
				_graphicsIndex = data;
				break;
			}
			case GraphicsRegisters.DataPort:
			{
				Graphics.Registers[_graphicsIndex] = data;
				break;
			}
			case CRTControllerRegisters.IndexPort:
			{
				_crtControllerIndex = data;
				break;
			}
			case CRTControllerRegisters.DataPort:
			{
				CRTController.Registers[_crtControllerIndex] = data;
				break;
			}
			case AttributeControllerRegisters.IndexAndDataWritePort:
			{
				switch (_attributeControllerPortMode)
				{
					case AddressAndDataPortMode.Address:
						_attributeControllerIndex = data;
						_attributeControllerPortMode = AddressAndDataPortMode.Data;
						break;
					case AddressAndDataPortMode.Data:
						AttributeController.Registers[_attributeControllerIndex] = data;
						_attributeControllerPortMode = AddressAndDataPortMode.Address;
						break;
				}

				break;
			}
			case DACRegisters.ReadIndexPort:
			{
				_dacReadIndex = data * 4;
				break;
			}
			case DACRegisters.WriteIndexPort:
			{
				_dacWriteIndex = data * 3;
				break;
			}
			case DACRegisters.DataPort:
			{
				DAC.Palette[_dacWriteIndex] = data;

				(int paletteIndex, int channelIndex) = int.DivRem(_dacWriteIndex, 3);

				if (channelIndex == 2)
					DAC.RebuildBGRA(paletteIndex);

				_dacWriteIndex++;

				if (_dacWriteIndex >= DAC.Palette.Length)
					_dacWriteIndex = 0;

				break;
			}
		}
	}

	public void OutPort2(int basePortNumber, byte index, byte data)
	{
		OutPort(basePortNumber, index);
		OutPort(basePortNumber + 1, data);
	}

	public byte InPort(int portNumber)
		=> InPort(portNumber, out _);

	public byte InPort(int portNumber, out bool handled)
	{
		switch (portNumber)
		{
			case GraphicsRegisters.DataPort:
				handled = true;
				return Graphics.Registers[_graphicsIndex];
			case SequencerRegisters.DataPort:
				handled = true;
				return Sequencer.Registers[_sequencerIndex];
			case CRTControllerRegisters.DataPort:
				handled = true;
				return CRTController.Registers[_crtControllerIndex];
			case AttributeControllerRegisters.DataReadPort:
				handled = true;
				return AttributeController.Registers[_attributeControllerIndex];
			case InputStatusRegisters.InputStatus1Port:
				handled = true;
				_attributeControllerPortMode = AddressAndDataPortMode.Address;
				break;
			case MiscellaneousOutputRegisters.ReadPort:
				handled = true;
				return MiscellaneousOutput.Register;

			case DACRegisters.DataPort:
			{
				handled = true;

				byte data = DAC.Palette[_dacReadIndex];

				_dacReadIndex++;

				if (_dacReadIndex >= DAC.Palette.Length)
					_dacReadIndex = 0;

				return data;
			}
		}

		handled = false;
		return 0;
	}

	public byte InPort2(int basePortNumber, byte index, out bool handled)
	{
		OutPort(basePortNumber, index);
		return InPort(basePortNumber + 1, out handled);
	}

	public byte this[int address]
	{
		get
		{
			int offset = address - Graphics.MemoryMapBaseAddress;

			if (Graphics.HostOddEvenRead)
				offset = (offset >> 1) | ((offset & 1) << 16);

			int linearOffset = offset + Graphics.MemoryMapReadOffset;

			if ((offset < Graphics.MemoryMapSize)
			 && (linearOffset < VRAM.Length))
				return VRAM[linearOffset];

			return 0;
		}
		set
		{
			int offset = address - Graphics.MemoryMapBaseAddress;

			if (Sequencer.HostOddEvenWrite)
			{
				offset = (offset >> 1) | ((offset & 1) << 16);

				if (offset < Graphics.MemoryMapSize)
					VRAM[offset] = value;
			}
			else
			{
				if (offset < Graphics.MemoryMapSize)
				{
					if (Sequencer.Plane0WriteEnable)
						VRAM[offset] = value;
					if (Sequencer.Plane1WriteEnable)
						VRAM[offset + 0x10000] = value;
					if (Sequencer.Plane2WriteEnable)
						VRAM[offset + 0x20000] = value;
					if (Sequencer.Plane3WriteEnable)
						VRAM[offset + 0x30000] = value;
				}
			}
		}
	}
}
