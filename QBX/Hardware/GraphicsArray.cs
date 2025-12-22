using System.Runtime.InteropServices;

namespace QBX.Hardware;

public class GraphicsArray
{
	public byte[] VRAM = new byte[256 * 1024];
	public int[] Palette = new int[256];

	public VideoMode VideoMode = 0;

	public class Graphics
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

		public const int SetReset_Plane0 = 1;
		public const int SetReset_Plane1 = 2;
		public const int SetReset_Plane2 = 4;
		public const int SetReset_Plane3 = 8;

		public const int EnableSetReset_Plane0 = 1;
		public const int EnableSetReset_Plane1 = 2;
		public const int EnableSetReset_Plane2 = 4;
		public const int EnableSetReset_Plane3 = 8;

		public const int DataRotate_OperationMask = 24;
		public const int DataRotate_RotateMask = 7;

		public const int DataRotate_Operation_Copy = 0;
		public const int DataRotate_Operation_AND = 8;
		public const int DataRotate_Operation_OR = 16;
		public const int DataRotate_Operation_XOR = 24;

		public const int GraphicsMode_Shift256 = 64;
		public const int GraphicsMode_ShiftInterleave = 32;
		public const int GraphicsMode_HostOddEvenRead = 16;
		public const int GraphicsMode_ReadCompare = 8;
		public const int GraphicsMode_WriteModeMask = 3;

		public const int MiscGraphics_MemoryMapMask = 12;
		public const int MiscGraphics_MemoryMap_Flat128K = 0;
		public const int MiscGraphics_MemoryMap_Graphics64K = 1;
		public const int MiscGraphics_MemoryMap_MonoText32K = 2;
		public const int MiscGraphics_MemoryMap_ColourText32K = 3;

		public int MemoryMapBaseAddress;
		public int MemoryMapOffset;
		public int MemoryMapSize;

		public RegisterSet Registers;

		public Graphics() { Registers = new(this); }

		public class RegisterSet(Graphics owner)
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
						case Graphics.SetReset: return SetReset;
						case Graphics.EnableSetReset: return EnableSetReset;
						case Graphics.ColourCompare: return ColourCompare;
						case Graphics.DataRotate: return DataRotate;
						case Graphics.ReadMapSelect: return ReadMapSelect;
						case Graphics.GraphicsMode: return GraphicsMode;
						case Graphics.MiscGraphics: return MiscGraphics;
						case Graphics.ColourDoNotCare: return ColourDoNotCare;
						case Graphics.BitMask: return BitMask;

						default: return 0;
					}
				}
				set
				{
					switch (index)
					{
						case Graphics.SetReset: SetReset = value; break;
						case Graphics.EnableSetReset: EnableSetReset = value; break;
						case Graphics.ColourCompare: ColourCompare = value; break;
						case Graphics.DataRotate: DataRotate = value; break;
						case Graphics.ReadMapSelect: ReadMapSelect = value; break;
						case Graphics.GraphicsMode: GraphicsMode = value; break;
						case Graphics.MiscGraphics: MiscGraphics = value; break;
						case Graphics.ColourDoNotCare: ColourDoNotCare = value; break;
						case Graphics.BitMask: BitMask = value; break;
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

					owner.MemoryMapOffset = ReadMapSelect * 65536;

					owner.MemoryMapSize =
						(MiscGraphics & MiscGraphics_MemoryMapMask) switch
						{
							MiscGraphics_MemoryMap_Flat128K => 131072,
							MiscGraphics_MemoryMap_Graphics64K => 65536,
							MiscGraphics_MemoryMap_MonoText32K => 32768,
							MiscGraphics_MemoryMap_ColourText32K => 32768,

							_ => throw new Exception("Internal error")
						};
				}
			}
		}
	}

	public class Sequencer
	{
		public const int IndexPort = 0x3C4;
		public const int DataPort = IndexPort + 1;

		public const int Reset = 0;
		public const int ClockingMode = 1;
		public const int MapMask = 2;
		public const int CharacterSet = 3;
		public const int SequencerMode = 4;

		public int Index = 0;

		public const byte Reset_SynchronousWhenCleared = 2;
		public const byte Reset_AsynchronousWhenCleared = 1;

		public const byte ClockingMode_ScreenDisable = 32;
		public const byte ClockingMode_Shift4 = 16;
		public const byte ClockingMode_DotClockHalfRate = 8;
		public const byte ClockingMode_ShiftLoadRate = 4;
		public const byte ClockingMode_CharacterWidth = 1;

		public const byte MapMask_Plane0 = 1;
		public const byte MapMask_Plane1 = 2;
		public const byte MapMask_Plane2 = 4;
		public const byte MapMask_Plane3 = 8;

		public const byte CharacterSet_MaskA = 3 | 16;
		public const byte CharacterSet_MaskB = 12 | 32;

		public const byte SequencerMode_Chain4 = 8;
		public const byte SequencerMode_OddEvenDisable = 4;
		public const byte SequencerMode_ExtendedMemory = 2;

		public RegisterSet Registers = new RegisterSet();

		public class RegisterSet
		{
			public byte Reset = Reset_SynchronousWhenCleared | Reset_AsynchronousWhenCleared;
			public byte ClockingMode = 0;
			public byte MapMask = MapMask_Plane0 | MapMask_Plane1;
			public byte CharacterSet = 0;
			public byte SequencerMode = SequencerMode_Chain4;

			public byte this[int index]
			{
				get
				{
					switch (index)
					{
						case Sequencer.Reset: return Reset;
						case Sequencer.ClockingMode: return ClockingMode;
						case Sequencer.MapMask: return MapMask;
						case Sequencer.CharacterSet: return CharacterSet;
						case Sequencer.SequencerMode: return SequencerMode;

						default: return 0;
					}
				}
				set
				{
					switch (index)
					{
						case Sequencer.Reset: Reset = value; break;
						case Sequencer.ClockingMode: ClockingMode = value; break;
						case Sequencer.MapMask: MapMask = value; break;
						case Sequencer.CharacterSet: CharacterSet = value; break;
						case Sequencer.SequencerMode: SequencerMode = value; break;
					}
				}
			}
		}
	}

	const int PaletteReadIndexPort = 0x3C7;
	const int PaletteWriteIndexPort = 0x3C8;
	const int PaletteDataPort = 0x3C9;

	public class MiscellaneousOutput
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

	public class CRTController
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

		public RegisterSet Registers;

		public CRTController() { Registers = new RegisterSet(this); }

		public class RegisterSet(CRTController owner)
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

			// TODO: these registers
			// TODO: OPEN/CLOSE statements
			// TODO: GET/PUT statements
			// TODO: CIRCLE statement
		}
	}

	Graphics _graphics = new Graphics();
	int _graphicsIndex;

	Sequencer _sequencer = new Sequencer();
	int _sequencerIndex;

	MiscellaneousOutput _miscellaneousOutput = new MiscellaneousOutput();

	CRTController _controller = new CRTController();
	int _controllerIndex;

	bool _3C0DataMode;
	int _3C0Index;
	byte _3C2Value;
	int _3D4Index;
	int _paletteReadIndex;
	int _paletteWriteIndex;

	public void OutPort(int portNumber, byte data)
	{
		if ((portNumber >= 0x3D4) && (portNumber <= 0x3DA) && _miscellaneousOutput.UseMonoIOPorts)
			return;

		if ((portNumber >= 0x3B4) && (portNumber <= 0x3BA))
		{
			if (_miscellaneousOutput.UseMonoIOPorts)
				portNumber += 0x20;
			else
				return;
		}

		switch (portNumber)
		{
			case 0x3C0:
			{
				if (!_3C0DataMode)
					_3C0Index = data;
				else
				{
					// TODO: write data to _3C0Index
				}

				_3C0DataMode = !_3C0DataMode;

				break;
			}
			case MiscellaneousOutput.WritePort:
			{
				_miscellaneousOutput.Register = data;
				break;
			}
			case Sequencer.IndexPort:
			{
				_sequencerIndex = data;
				break;
			}
			case Sequencer.DataPort:
			{
				_sequencer.Registers[_sequencerIndex] = data;
				break;
			}
			case Graphics.IndexPort:
			{
				_graphicsIndex = data;
				break;
			}
			case Graphics.DataPort:
			{
				_graphics.Registers[_graphicsIndex] = data;
				break;
			}
			case CRTController.IndexPort:
			{
				_controllerIndex = data;
				break;
			}
			case CRTController.DataPort:
			{
				_controller.Registers[_controllerIndex] = data;
				break;
			}
			case PaletteReadIndexPort:
			{
				_paletteReadIndex = data * 4;
				break;
			}
			case PaletteWriteIndexPort:
			{
				_paletteWriteIndex = data * 4;
				break;
			}
			case PaletteDataPort:
			{
				var paletteBytes = MemoryMarshal.Cast<int, byte>(Palette.AsSpan());

				paletteBytes[_paletteWriteIndex] = data;

				_paletteWriteIndex++;
				if ((_paletteWriteIndex & 3) == 3)
					_paletteWriteIndex++;

				if (_paletteWriteIndex >= paletteBytes.Length)
					_paletteWriteIndex = 0;

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
	{
		switch (portNumber)
		{
			case Graphics.DataPort:
				return _graphics.Registers[_graphicsIndex];
			case Sequencer.DataPort:
				return _sequencer.Registers[_sequencerIndex];

			case PaletteDataPort:
			{
				var paletteBytes = MemoryMarshal.Cast<int, byte>(Palette.AsSpan());

				byte data = paletteBytes[_paletteReadIndex];

				_paletteReadIndex++;
				if ((_paletteReadIndex & 3) == 3)
					_paletteReadIndex++;

				if (_paletteReadIndex >= paletteBytes.Length)
					_paletteReadIndex = 0;

				return data;
			}
		}

		return 0;
	}

	public byte InPort2(int basePortNumber, byte index)
	{
		OutPort(basePortNumber, index);
		return InPort(basePortNumber + 1);
	}

	public byte this[int address]
	{
		get
		{
			int offset = address - _graphics.MemoryMapBaseAddress;

			int linearOffset = offset + _graphics.MemoryMapOffset;

			if ((offset < _graphics.MemoryMapSize)
			 && (linearOffset < VRAM.Length))
				return VRAM[linearOffset];

			return 0;
		}
		set
		{
			int offset = address - _graphics.MemoryMapBaseAddress;

			int linearOffset = offset + _graphics.MemoryMapOffset;

			if ((offset < _graphics.MemoryMapSize)
			 && (linearOffset < VRAM.Length))
				VRAM[offset] = value;
		}
	}
}
