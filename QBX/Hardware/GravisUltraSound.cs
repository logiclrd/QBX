using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using QBX.Utility;

namespace QBX.Hardware;

public class GravisUltraSound
{
	public bool IsEnabled;

	public const int DefaultBasePort = 0x240;

	const int MixControlPortOffset = 0x000;
	const int IRQStatusPortOffset = 0x006;
	const int TimerControlPortOffset = 0x008;
	const int ControlRegisterOffset = 0x00B;
	const int PageRegisterPortOffset = 0x102;
	const int RegisterSelectPortOffset = 0x103;
	const int DataLowPortOffset = 0x104;
	const int DataHighPortOffset = 0x105;
	const int RAMIOPortOffset = 0x107;

	public const int VoiceCount = 32;
	public const int DRAMSize = 1048576;

	// Real GUS varies the sample rate based on number of active channels. We're feeding
	// into a common output buffer that will always be 44,100 samples/sec.
	public const int SampleRate = 44100;

	const int SampleOffsetScaleBits = 12;
	const int SampleOffsetScale = 1 << SampleOffsetScaleBits;

	const int ClearLowMask = unchecked((int)0xFFFF0000);
	const int ClearHighMask = 0x0000FFFF;

	int _basePort;

	public int BasePort
	{
		get => _basePort;
		set
		{
			_basePort = value;
		}
	}

	public int MixControlPort => _basePort + MixControlPortOffset;
	public int PageRegisterPort => _basePort + PageRegisterPortOffset;
	public int RegisterSelectPort => _basePort + RegisterSelectPortOffset;
	public int DataLowPort => _basePort + DataLowPortOffset;
	public int DataHighPort => _basePort + DataHighPortOffset;

	public GravisUltraSound()
	{
		BasePort = DefaultBasePort;

		_dram = new byte[DRAMSize];

		_liveRegisters = new Registers(_dram);
		_playbackRegisters = new Registers(_dram);

		_firstSampleEmittedTime = DateTime.UtcNow;
	}

	static int[] s_volumeTable;

	static GravisUltraSound()
	{
		// The GF1 chip treats volumes as floating-point values. The layout is carefully-
		// chosen so that the translation to a linear volume value preserves order.
		// The formula for the translation is:
		//
		//   exponent = volume >> 8
		//   mantissa_bits = volume & 0b11111111
		//
		//   mantissa = 1 + mantissa_bits / 256
		//
		//   linear_volume = mantissa * 2^exponent
		//
		// As such, the largest possible value of the mantissa bits produces a mantissa
		// of just under 2, and adding one to the volume wraps the mantissa back to 1
		// but increases the exponent, effectively multiplying the value by 2.
		//
		// We precalculate a table for this translation, rounded to the nearest integer.
		// Our audio data is 16-bit, which means that preserving a greater level of
		// detail in the linear volume values isn't going to translate to more detailed
		// sample data anyway. Using a 16-bit value here means that we can multiply the
		// volume by a 16-bit sample and the result will still like within a 32-bit
		// integer range. The result is then simply the top 16 bits.
		//
		// A volume value of 0 is special-cased to turn the voice off entirely. We
		// simply leave slot 0 of the table at its initial value of 0 by starting the
		// translation at a volume value of 1.

		s_volumeTable = new int[4096];

		for (int volume = 1; volume < 4096; volume++)
		{
			// Using fixed-point with a scale of 256, we can eliminate fractional
			// values from the intermediate calculation.
			int mantissa = 256 + (volume & 255);
			int exponent = (volume >> 8);

			s_volumeTable[volume] = (mantissa << exponent + 128) >> 8;
		}
	}

	enum Register : byte
	{
		Voice_VoiceControl = 0x00,
		Voice_FrequencyControl = 0x01,
		Voice_StartAddressHigh = 0x02,
		Voice_StartAddressLow = 0x03,
		Voice_EndAddressHigh = 0x04,
		Voice_EndAddressLow = 0x05,
		Voice_RampRate = 0x06,
		Voice_RampLowEnd = 0x07, // Ramp direction is controlled by Volume Control
		Voice_RampHighEnd = 0x08,
		Voice_CurrentVolume = 0x09,
		Voice_CurrentAddressHigh = 0x0A,
		Voice_CurrentAddressLow = 0x0B,
		Voice_Panning = 0x0C,
		Voice_VolumeRampControl = 0x0D,

		LastVoiceRegister = 0x0D,

		Global_MaxActiveVoice = 0x0E,
		Global_DMAControl = 0x41,
		Global_DMAAddress = 0x42,
		Global_RAMIOAddressLow = 0x43,
		Global_RAMIOAddressHigh = 0x44,
		Global_TimerControl = 0x45,
		Global_Timer1Count = 0x46,
		Global_Timer2Count = 0x47,
		Global_DMASamplingFrequency = 0x48,
		Global_DMASamplingControl = 0x49,
		Global_Reset = 0x4C,

		ReadFlag = 0x80,

		None = 0xFF,
	}

	[Flags]
	enum MixControlFlags : byte
	{
		DisableLineIn = 1,
		DisableLineOut = 2,
		EnableMicIn = 4,
		EnableLatches = 8,
		CombineChannel1And2IRQs = 16,
		EnableMIDILoopBack = 32,
		ControlRegisterSelect = 64,

		ControlRegister_DMALatches = 0,
		ControlRegister_IRQControlLatches = 64,
	}

	[Flags]
	enum VoiceControlFlags : byte
	{
		VoiceStopped = 1,
		StopVoice = 2,
		_16bit = 4,
		Loop = 8,
		PingPongLoop = 16,
		EnableIRQ = 32,
		ReverseWaveData = 64,
		PendingIRQ = 128,
	}

	[Flags]
	enum VolumeRampControlFlags : byte
	{
		RampStopped = 1,
		StopRamp = 2,
		EnableRollOverIRQ = 4,
		Loop = 8,
		PingPongLoop = 16,
		EnableIRQ = 32,
		ReverseRamp = 64,
		PendingIRQ = 128,
	}

	[Flags]
	enum VolumeRampFlags : byte
	{
		IncrementMask = 0b00111111,
		RateMask = 0b11000000,

		Rate_Every1Frame = 0,
		Rate_Every8Frames = 64,
		Rate_Every64Frames = 128,
		Rate_Every512Frames = 192,
	}

	enum PanValues : byte
	{
		Mask = 0x0F,

		FullLeft = 0,
		// center?? 7.5 lul
		FullRight = 15,
	}

	[Flags]
	enum IRQStatusFlags : byte
	{
		MIDITransmit = 1,
		MIDIReceive = 2,
		Timer1 = 4,
		Timer2 = 8,
		WaveTable = 32,
		VolumeRamp = 64,
		DMA = 128,
	}

	[Flags]
	enum ResetFlags : byte
	{
		MasterReset = 1,
		EnableDAC = 2,
		EnableIRQ = 4,
	}

	struct StereoSample
	{
		public int Left, Right;
	}

	struct Registers
	{
		struct Voice
		{
			public Voice(MutableBox<int> activeVoices)
			{
				ActiveVoices = activeVoices;
			}

			public MutableBox<int> ActiveVoices;

			public VoiceControlFlags VoiceControl;
			public ushort FrequencyControl;
			public int StartAddress;
			public int EndAddress;
			public VolumeRampFlags VolumeRamp;
			public ushort VolumeRampLowEndScaled; // Register value in the top 8 bits
			public ushort VolumeRampHighEndScaled; // Register value in the top 8 bits
			public ushort CurrentVolumeScaled; // 4 extra bits for fractional detail during ramps
			public int CurrentLinearVolume;
			public PanValues Panning;
			public VolumeRampControlFlags VolumeRampControl;

			int _startAddressWhole;
			int _endAddressWhole;

			// On each sample, delta/fraction is added to offset/fraction, and fraction overflow, if any, is transferred to offset.
			public int SampleOffset;
			public int SampleOffsetFraction;
			public int SampleOffsetDelta;
			public int SampleOffsetDeltaFraction;

			public int VolumeRampFrameInterval;
			public int VolumeRampFrameCount;
			public ushort VolumeRampDelta;

			bool _enableWaveTableIRQ;
			bool _enableVolumeRampIRQ;

			void UpdateEnableWaveTableIRQ()
			{
				_enableWaveTableIRQ =
					((VoiceControl & VoiceControlFlags.EnableIRQ) != 0) ||
					((VolumeRampControl & VolumeRampControlFlags.EnableRollOverIRQ) != 0);
			}

			void UpdateEnableVolumeRampIRQ()
			{
				_enableVolumeRampIRQ = ((VolumeRampControl & VolumeRampControlFlags.EnableIRQ) != 0);
			}

			public void SetRegister(Register index, byte lowByte, byte highByte)
			{
				ushort value = unchecked((ushort)((highByte << 8) | lowByte));

				switch (index)
				{
					case Register.Voice_VoiceControl:
						VoiceControl &= VoiceControlFlags.PendingIRQ;
						VoiceControl |= unchecked((VoiceControlFlags)highByte) & ~VoiceControlFlags.PendingIRQ;

						UpdateEnableWaveTableIRQ();

						break;
					case Register.Voice_FrequencyControl:
						int divisor = ActiveVoices.Value * 512;

						FrequencyControl = unchecked((ushort)(value >> 1));

						SampleOffsetDeltaFraction = (FrequencyControl * SampleOffsetScale * 14 + (divisor >> 1)) / divisor;

						SampleOffsetDelta = SampleOffsetDeltaFraction / SampleOffsetScale;
						SampleOffsetDeltaFraction = SampleOffsetDeltaFraction % SampleOffsetScale;

						break;
					case Register.Voice_StartAddressHigh:
						StartAddress &= ClearHighMask;
						StartAddress |= (value << 16);
						_startAddressWhole = StartAddress >> 9;
						break;
					case Register.Voice_StartAddressLow:
						StartAddress &= ClearLowMask;
						StartAddress |= (value & 0xFFE0); // address is always a multiple of 32
						_startAddressWhole = StartAddress >> 9;
						break;
					case Register.Voice_EndAddressHigh:
						EndAddress &= ClearHighMask;
						EndAddress |= (value << 16);
						_endAddressWhole = EndAddress >> 9;
						break;
					case Register.Voice_EndAddressLow:
						EndAddress &= ClearLowMask;
						EndAddress |= (value & 0xFFE0); // address is always a multiple of 32
						_endAddressWhole = EndAddress >> 9;
						break;
					case Register.Voice_RampRate:
						VolumeRamp = unchecked((VolumeRampFlags)highByte);

						VolumeRampDelta = unchecked((ushort)(VolumeRamp & VolumeRampFlags.IncrementMask));
						VolumeRampDelta <<= 4;

						switch (VolumeRamp & VolumeRampFlags.RateMask)
						{
							case VolumeRampFlags.Rate_Every1Frame: VolumeRampFrameInterval = 1; break;
							case VolumeRampFlags.Rate_Every8Frames: VolumeRampFrameInterval = 8; break;
							case VolumeRampFlags.Rate_Every64Frames: VolumeRampFrameInterval = 64; break;
							case VolumeRampFlags.Rate_Every512Frames: VolumeRampFrameInterval = 512; break;
						}

						break;
					case Register.Voice_RampLowEnd:
						VolumeRampLowEndScaled = highByte;
						VolumeRampLowEndScaled <<= 8;
						break;
					case Register.Voice_RampHighEnd:
						VolumeRampHighEndScaled = highByte;
						VolumeRampHighEndScaled <<= 8;
						break;
					case Register.Voice_CurrentVolume:
						sampleCount = 0;
						rampCount = 0;
						CurrentVolumeScaled = value;
						CurrentLinearVolume = s_volumeTable[CurrentVolumeScaled >> 4];
						break;
					case Register.Voice_CurrentAddressHigh:
						// Native GUS uses 9-bit fixed point, which means that the
						// high value is 7 bits into the integer part.
						SampleOffset &= 0b11111111;
						SampleOffset |= value << 7;
						break;
					case Register.Voice_CurrentAddressLow:
						// Native GUS uses 9-bit fixed point, which means that the
						// low value contains the bottom 7 bits of the integer part
						// and the entirety of the fractional part.
						SampleOffset &= ~0b11111111;
						SampleOffset |= value >> 9;
						SampleOffsetFraction = (value & 0b111111111) * SampleOffsetScale / 512;
						break;
					case Register.Voice_Panning:
						Panning = unchecked((PanValues)highByte);
						break;
					case Register.Voice_VolumeRampControl:
						VolumeRampControl = unchecked((VolumeRampControlFlags)highByte);

						UpdateEnableWaveTableIRQ();
						UpdateEnableVolumeRampIRQ();

						break;
				}
			}

			int sampleCount;
			int rampCount;

			public ushort GetRegister(Register index)
			{
				switch (index ^ Register.ReadFlag)
				{
					case Register.Voice_VoiceControl: return (byte)VoiceControl;
					case Register.Voice_FrequencyControl: return FrequencyControl;
					case Register.Voice_StartAddressHigh: return unchecked((ushort)(StartAddress >> 16));
					case Register.Voice_StartAddressLow: return unchecked((ushort)(StartAddress));
					case Register.Voice_EndAddressHigh: return unchecked((ushort)(EndAddress >> 16));
					case Register.Voice_EndAddressLow: return unchecked((ushort)(EndAddress));
					case Register.Voice_RampRate: return unchecked((ushort)VolumeRamp);
					case Register.Voice_RampLowEnd: return unchecked((byte)(VolumeRampLowEndScaled >> 8));
					case Register.Voice_RampHighEnd: return unchecked((byte)(VolumeRampHighEndScaled >> 8));
					case Register.Voice_CurrentVolume: return unchecked((ushort)CurrentVolumeScaled);

					case Register.Voice_CurrentAddressHigh:
						// Native GUS uses 9-bit fixed point, which means that the
						// high value is 7 bits into the integer part.
						return unchecked((ushort)(SampleOffset >> 7));
					case Register.Voice_CurrentAddressLow:
						// Native GUS uses 9-bit fixed point, which means that the
						// low value contains the bottom 7 bits of the integer part
						// and the top 9 bits of the fractional part.
						return unchecked((ushort)((SampleOffset << 9) | (SampleOffsetFraction * 512 / SampleOffsetScale)));

					case Register.Voice_Panning: return unchecked((byte)Panning);
				}

				return 0; // ?
			}

			// Process one tick, move to the next sample
			public void Update(ref IRQStatusFlags irqStatus)
			{
				if ((VoiceControl & VoiceControlFlags.VoiceStopped) != 0)
					return;

				if ((VoiceControl & VoiceControlFlags.StopVoice) != 0)
				{
					VoiceControl |= VoiceControlFlags.VoiceStopped;
					return;
				}

				sampleCount++;

				bool hitBoundary;
				int overshoot;

				// Update the sample offset
				hitBoundary = false;
				overshoot = 0;

				if ((VoiceControl & VoiceControlFlags.ReverseWaveData) == 0)
				{
					SampleOffset += SampleOffsetDelta;
					SampleOffsetFraction += SampleOffsetDeltaFraction;

					if (SampleOffsetFraction >= SampleOffsetScale)
					{
						SampleOffset++;
						SampleOffsetFraction -= SampleOffsetScale;
					}

					if (SampleOffset >= _endAddressWhole)
					{
						int sampleOffsetScaled = (SampleOffset << 9) | (SampleOffsetFraction >> (SampleOffsetScale - 9));

						if (sampleOffsetScaled >= EndAddress)
						{
							hitBoundary = true;
							overshoot = sampleOffsetScaled - EndAddress;
						}
					}
				}
				else
				{
					SampleOffset -= SampleOffsetDelta;
					SampleOffsetFraction -= SampleOffsetDeltaFraction;

					if (SampleOffsetFraction < 0)
					{
						SampleOffset--;
						SampleOffsetFraction += SampleOffsetScale;
					}

					// This replicates a GF1 hardware bug, whereby if the sample offset crosses the
					// start address but also crosses 0 in the same tick, it wraps around to the top
					// of memory before the processor can detect that it has crossed over the start
					// address. Another way to interpret this is that the sample offset is an
					// intrinsically unsigned value, so if offset < delta, then subtracting delta
					// fails to make it negative. The bug, then, is that it is unable to detect that
					// it has hit a boundary should loop or terminate.
					SampleOffset &= 1048575;

					if (SampleOffset <= _startAddressWhole)
					{
						int sampleOffsetScaled = (SampleOffset << 9) | (SampleOffsetFraction >> (SampleOffsetScale - 9));

						if (sampleOffsetScaled < StartAddress)
						{
							hitBoundary = true;
							overshoot = sampleOffsetScaled - StartAddress + 1;
						}
					}
				}

				if (hitBoundary)
				{
					if (_enableWaveTableIRQ)
						irqStatus |= IRQStatusFlags.WaveTable;

					if ((VoiceControl & VoiceControlFlags.Loop) == 0)
					{
						if ((VolumeRampControl & VolumeRampControlFlags.EnableRollOverIRQ) == 0)
						{
							VoiceControl |= VoiceControlFlags.VoiceStopped;

							if ((VoiceControl & VoiceControlFlags.ReverseWaveData) == 0)
							{
								SampleOffset = StartAddress >> 9;
								SampleOffsetFraction = (StartAddress & 511) << (SampleOffsetScale - 9);
							}
							else
							{
								SampleOffset = EndAddress >> 9;
								SampleOffsetFraction = (EndAddress & 511) << (SampleOffsetScale - 9);
							}

							SampleOffsetFraction = 0;
						}
					}
					else
					{
						if ((VoiceControl & VoiceControlFlags.PingPongLoop) != 0)
						{
							VoiceControl ^= VoiceControlFlags.ReverseWaveData;

							// If we have a fractional part, invert it. To illustrate, suppose the
							// sample offset scale is 1,000. If we overshoot and end at +1.002, then
							// mirroring that across the boundary, we want to go to -1.002, which is
							// represented as -2 + .998.
							if (SampleOffsetFraction != 0)
							{
								SampleOffsetFraction = SampleOffsetScale - SampleOffsetFraction;
								overshoot++;
							}
						}

						int newSampleOffsetScaled;

						if ((VoiceControl & VoiceControlFlags.ReverseWaveData) == 0)
							newSampleOffsetScaled = StartAddress + overshoot;
						else
							newSampleOffsetScaled = EndAddress + overshoot;

						SampleOffset = newSampleOffsetScaled >> 9;
						SampleOffsetFraction = (newSampleOffsetScaled & 511) << (SampleOffsetScale - 9);
					}
				}

				// Update the volume ramp
				if ((VolumeRampControl & VolumeRampControlFlags.StopRamp) != 0)
					VolumeRampControl |= VolumeRampControlFlags.RampStopped;

				if ((VolumeRampControl & VolumeRampControlFlags.RampStopped) == 0)
				{
					VolumeRampFrameCount++;

					if (VolumeRampFrameCount >= VolumeRampFrameInterval)
					{
						VolumeRampFrameCount = 0;

						hitBoundary = false;
						overshoot = 0;

						rampCount++;

						int newVolumeScaled = CurrentVolumeScaled;

						if ((VolumeRampControl & VolumeRampControlFlags.ReverseRamp) == 0)
						{
							newVolumeScaled += VolumeRampDelta;

							hitBoundary = (newVolumeScaled >= VolumeRampHighEndScaled);
							overshoot = newVolumeScaled - VolumeRampHighEndScaled;
						}
						else
						{
							newVolumeScaled -= VolumeRampDelta;

							hitBoundary = (newVolumeScaled <= VolumeRampLowEndScaled);
							overshoot = newVolumeScaled - VolumeRampLowEndScaled;
						}

						CurrentVolumeScaled = unchecked((ushort)newVolumeScaled);
						CurrentLinearVolume = s_volumeTable[CurrentVolumeScaled >> 4];

						if (hitBoundary)
						{
							if (_enableVolumeRampIRQ)
								irqStatus |= IRQStatusFlags.VolumeRamp;

							if ((VolumeRampControl & VolumeRampControlFlags.Loop) == 0)
							{
								VolumeRampControl |= VolumeRampControlFlags.RampStopped;
								CurrentVolumeScaled = unchecked((ushort)(CurrentVolumeScaled - overshoot));
								CurrentLinearVolume = s_volumeTable[CurrentVolumeScaled >> 4];
							}
							else
							{
								if ((VolumeRampControl & VolumeRampControlFlags.PingPongLoop) != 0)
									VolumeRampControl ^= VolumeRampControlFlags.ReverseRamp;

								if ((VolumeRampControl & VolumeRampControlFlags.ReverseRamp) == 0)
									SampleOffset = VolumeRampLowEndScaled + overshoot;
								else
									SampleOffset = VolumeRampHighEndScaled + overshoot;
							}
						}
					}
				}
			}
		}

		[InlineArray(length: VoiceCount)]
		struct Voices
		{
			Voice _element0;
		}

		public long SampleNumber; // timestamp for this register state

		public int SelectedVoice;
		public MixControlFlags MixControl;
		public IRQStatusFlags IRQStatus;

		public byte LastIOByte;
		
		Register _selectedRegister;
		byte _dataLowByte;

		int _maxActiveVoice;
		// Must be 0b11000000, nobody should ever not supply that, but if they do, then the emulator pauses voice updates.
		int _clockDividerEnableBits;
		byte _irqControl = 0;
		byte _dmaControl;
		int _dmaAddress;
		byte _timerControl;
		byte _timer1Count;
		byte _timer2Count;
		byte _dmaSamplingStep;
		byte _dmaSamplingControl;
		ResetFlags _reset;

		// Easiest way to get around "not used" warnings. :-)
		public byte IRQControl => _irqControl;
		public int DMAAddress => _dmaAddress;
		public byte Timer1Count => _timer1Count;
		public byte Timer2Count => _timer2Count;
		public byte DMASamplingStep => _dmaSamplingStep;

		Voices _voices;

		public MutableBox<int> ActiveVoices;
		public int RAMIOAddress;

		// External to structure
		byte[] _dram;

		public Registers(byte[] dram)
		{
			_dram = dram;

			ActiveVoices = new MutableBox<int>();
			ActiveVoices.Value = 14;

			for (int i = 0; i < VoiceCount; i++)
				_voices[i] = new Voice(ActiveVoices);
		}

		public void CopyFrom(ref Registers other)
		{
			// Blit the entire state of the other registers, except maintain a
			// logically-independent ActiveVoices box.

			var activeVoices = ActiveVoices;

			this = other;

			ActiveVoices = activeVoices;

			for (int i = 0; i < VoiceCount; i++)
				_voices[i].ActiveVoices = activeVoices;

			ActiveVoices.Value = other.ActiveVoices.Value;
		}

		public void SetGlobalRegister(Register index, byte lowByte, byte highByte)
		{
			switch (index)
			{
				case Register.Global_MaxActiveVoice:
					// Documentation states that this value is clamped to [13,32). But, community
					// discussions suggest that they didn't actually get around to implementing
					// this, meaning that effective sample rates above 44,100 are possible (but
					// filtering on the output might still limit the output frequency envelope).
					//
					// We always generate output at 44,100, which means that if the number of
					// voices is set lower than 14, then we are implicitly resampling to a
					// lower quality level. Shrug.
					//
					// Bits 6 and 7 are documented as required to be set. It isn't clearly
					// explained what happens if they aren't. Google AI suggests that it breaks
					// logic within the clock divider circuit, so that while they aren't set,
					// the GF1 chip stops being able to cycle through the voices. So, while it
					// may or may not match hardware behaviour exactly, I'm choosing to treat
					// bits 6 and 7 as enable bits for the clock divider.

					_maxActiveVoice = highByte & 31;
					_clockDividerEnableBits = highByte & 0b11000000;
					break;
				case Register.Global_DMAControl:
					_dmaControl = highByte;
					break;
				case Register.Global_DMAAddress:
					_dmaAddress = (highByte << 8) | lowByte;
					break;
				case Register.Global_RAMIOAddressLow:
					RAMIOAddress &= ~0xFFFF;
					RAMIOAddress |= (highByte << 8) | lowByte;
					break;
				case Register.Global_RAMIOAddressHigh:
					RAMIOAddress &= 0xFFFF;
					RAMIOAddress |= (highByte << 16);
					break;
				case Register.Global_TimerControl:
					_timerControl = highByte;
					break;
				case Register.Global_Timer1Count:
					_timer1Count = highByte;
					break;
				case Register.Global_Timer2Count:
					_timer2Count = highByte;
					break;
				case Register.Global_DMASamplingFrequency:
					_dmaSamplingStep = highByte;
					break;
				case Register.Global_DMASamplingControl:
					_dmaSamplingControl = highByte;
					break;
				case Register.Global_Reset:
					ResetFlags newReset = unchecked((ResetFlags)highByte);

					if (_reset.HasFlag(ResetFlags.MasterReset) && !newReset.HasFlag(ResetFlags.MasterReset))
						newReset &= ~(ResetFlags.EnableDAC | ResetFlags.EnableIRQ);

					_reset = newReset;

					break;
			}
		}

		public ushort GetGlobalRegister(Register index)
		{
			switch (index ^ Register.ReadFlag)
			{
				case Register.Global_MaxActiveVoice: return unchecked((byte)(_maxActiveVoice | _clockDividerEnableBits));
				case Register.Global_DMAControl: return _dmaControl;
				case Register.Global_TimerControl: return _timerControl;
				case Register.Global_DMASamplingControl: return _dmaSamplingControl;
				case Register.Global_Reset: return unchecked((byte)_reset);
			}

			return 0;
		}

		public void SetRegister(Register index, byte lowByte, byte highByte)
		{
			if (index > Register.LastVoiceRegister)
				SetGlobalRegister(index, lowByte, highByte);
			else
				_voices[SelectedVoice].SetRegister(index, lowByte, highByte);
		}

		public ushort GetRegister(Register index)
		{
			if (index > Register.LastVoiceRegister)
				return GetGlobalRegister(index);
			else
				return _voices[SelectedVoice].GetRegister(index);
		}

		public ushort GetSelectedRegister() => GetRegister(_selectedRegister);

		public bool OutPort(int port, byte value, bool updateDRAM)
		{
			switch (port)
			{
				case MixControlPortOffset:
					MixControl = unchecked((MixControlFlags)value);
					return true;

				case PageRegisterPortOffset:
					SelectedVoice = unchecked((byte)(value & 31));
					LastIOByte = value;
					return true;
				case RegisterSelectPortOffset:
					_selectedRegister = (Register)value;
					LastIOByte = value;
					return true;
				case DataLowPortOffset:
					_dataLowByte = value;
					LastIOByte = value;
					return true;
				case DataHighPortOffset:
					LastIOByte = value;

					SetRegister(_selectedRegister, _dataLowByte, value);

					return true;
				case RAMIOPortOffset:
					if (updateDRAM)
						_dram[RAMIOAddress] = value;
					return true;
			}

			return false;
		}

		public void UpdateVoice(int voiceNumber)
		{
			if ((voiceNumber >= 0) && (voiceNumber <= _maxActiveVoice))
				_voices[voiceNumber].Update(ref IRQStatus);
		}

		public short UpdateVoiceAndAccumulateSample(int voiceNumber, Span<byte> dram, ref StereoSample accumulator)
		{
			if ((voiceNumber >= 0) && (voiceNumber <= _maxActiveVoice))
			{
				ref Voice voice = ref _voices[voiceNumber];

				// Update playhead and volume ramp positions.
				voice.Update(ref IRQStatus);

				// As efficiently as possible, pluck the two samples surrounding the current
				// playhead position and convert them to S16LE.
				int s1, s2;

				unchecked
				{
					uint sampleOffset1 = (uint)voice.SampleOffset;
					uint sampleOffset2 = sampleOffset1 + 1;

					if ((voice.VoiceControl & VoiceControlFlags._16bit) != 0)
					{
						var data = MemoryMarshal.Cast<byte, short>(dram);

						if (sampleOffset2 < (uint)data.Length)
						{
							s1 = data[(int)sampleOffset1];
							s2 = data[(int)sampleOffset2];
						}
						else
						{
							s1 = (sampleOffset1 < (uint)data.Length) ? data[(int)sampleOffset1] : 0;
							s2 = 0;
						}
					}
					else
					{
						if (sampleOffset2 < (uint)dram.Length)
						{
							ushort ss1 = dram[(int)sampleOffset1];
							ushort ss2 = dram[(int)sampleOffset2];

							s1 = unchecked((short)(ss1 * 0x101));
							s2 = unchecked((short)(ss2 * 0x101));
						}
						else
						{
							ushort ss1 = (sampleOffset1 < (uint)dram.Length) ? dram[(int)sampleOffset1] : (ushort)0;

							s1 = unchecked((short)(ss1 * 0x101));
							s2 = 0;
						}
					}
				}

				// Interpolate between the two samples.
				int s = ((s1 << SampleOffsetScaleBits)
					+ voice.SampleOffsetFraction * (s2 - s1)
					+ (1 << (SampleOffsetScaleBits - 1))) >> SampleOffsetScaleBits;

				// Apply volume & panning.
				int leftVolume = voice.CurrentLinearVolume;
				int rightVolume = leftVolume;

				int panning = 2 * unchecked((int)voice.Panning);

				if (panning < 16)
					rightVolume = rightVolume * panning / 15;
				if (panning > 15)
					leftVolume = leftVolume * (30 - panning) / 15;

				accumulator.Left += unchecked((short)((s * leftVolume) >> 16));
				accumulator.Right += unchecked((short)((s * rightVolume) >> 16));
			}

			return 0;
		}

		const int RegisterSetCopyThreshold = 5;

		public void BringUpToDate(DateTime sampleEpoch, ref Registers playbackSet)
		{
			long nowSample = (DateTime.UtcNow - sampleEpoch).Ticks * SampleRate / TimeSpan.TicksPerSecond;

			if ((playbackSet.SampleNumber > SampleNumber) && (nowSample > SampleNumber + RegisterSetCopyThreshold))
				CopyFrom(ref playbackSet);

			int activeVoices = ActiveVoices.Value;

			while (SampleNumber < nowSample)
			{
				for (int i = 0; i < activeVoices; i++)
					_voices[i].Update(ref IRQStatus);

				SampleNumber++;
			}
		}
	}

	Registers _liveRegisters;
	Registers _playbackRegisters;

	byte[] _dram;

	public byte InPort(int port, out bool handled)
	{
		if (!IsEnabled)
		{
			handled = false;
			return default;
		}

		_liveRegisters.BringUpToDate(_firstSampleEmittedTime, ref _playbackRegisters);

		handled = true;

		switch (port - BasePort)
		{
			case IRQStatusPortOffset:
				byte irqStatus = unchecked((byte)_liveRegisters.IRQStatus);

				_liveRegisters.IRQStatus = 0;

				return irqStatus;

			case TimerControlPortOffset: return 0; // TODO

			// GUS bug: Reads on the page register and register select ports do not return
			//          the selected page and selected register, but rather the last byte
			//          in or out on any port in the range base + 0x102 .. base + 0x105.
			//          The value is latched far too aggressively.

			case PageRegisterPortOffset: return _liveRegisters.LastIOByte;
			case RegisterSelectPortOffset: return _liveRegisters.LastIOByte;

			case DataLowPortOffset: return unchecked((byte)_liveRegisters.GetSelectedRegister());
			case DataHighPortOffset: return unchecked((byte)(_liveRegisters.GetSelectedRegister() >> 8));

			case RAMIOPortOffset:
				if (_liveRegisters.RAMIOAddress < _dram.Length)
					return _dram[_liveRegisters.RAMIOAddress];
				else
					return 0;
		}

		handled = false;

		return 0;
	}

	public void OutPort(int port, byte value)
	{
		if (!IsEnabled)
			return;

		_liveRegisters.BringUpToDate(_firstSampleEmittedTime, ref _playbackRegisters);

		port -= BasePort;

		if (_liveRegisters.OutPort(port, value, updateDRAM: false))
		{
			var op = new PortIOOperation();

			op.Sample = _liveRegisters.SampleNumber;
			op.PortNumber = port;
			op.Value = value;

			_portIOOperations.Enqueue(op);
		}
	}

	struct PortIOOperation
	{
		public long Sample;

		public int PortNumber;
		public byte Value;
	}

	DateTime _firstSampleEmittedTime;

	ConcurrentQueue<PortIOOperation> _portIOOperations = new ConcurrentQueue<PortIOOperation>();

	short _leftOverRightSample;
	bool _haveLeftOverRightSample;

	public void GetMoreSound(Span<short> samples)
	{
		if (!IsEnabled)
			return;

		long lastSampleEmitted = _playbackRegisters.SampleNumber;

		if (_firstSampleEmittedTime == default)
			_firstSampleEmittedTime = DateTime.UtcNow - TimeSpan.FromSeconds(samples.Length / SampleRate);

		var thisBufferStartTime = _firstSampleEmittedTime
			.AddTicks(lastSampleEmitted * 10_000_000L / SampleRate);

		var slippage = DateTime.UtcNow - thisBufferStartTime;

		if (slippage.TotalSeconds > 0.5)
			_firstSampleEmittedTime += slippage;

		if (_haveLeftOverRightSample)
		{
			samples[0] = _leftOverRightSample;
			samples = samples.Slice(1);

			_haveLeftOverRightSample = false;
		}

		bool enableLineOut = !_playbackRegisters.MixControl.HasFlag(MixControlFlags.DisableLineOut);

		int i = 0;

		while (i < samples.Length)
		{
			while (_portIOOperations.TryPeek(out var op))
			{
				if (op.Sample > lastSampleEmitted)
					break;

				_playbackRegisters.OutPort(op.PortNumber, op.Value, updateDRAM: true);

				_portIOOperations.TryDequeue(out _);
			}

			int activeVoices = _playbackRegisters.ActiveVoices.Value;

			var accumulator = new StereoSample();

			Span<byte> dramSpan = _dram;

			for (int ch = 0; ch < activeVoices; ch++)
			{
				_playbackRegisters.UpdateVoiceAndAccumulateSample(ch, dramSpan, ref accumulator);
				lastSampleEmitted++;
			}

			if (!enableLineOut)
				i += 2;
			else
			{
				samples[i++] = unchecked((short)Math.Clamp(accumulator.Left >> 1, short.MinValue, short.MaxValue));

				if (i >= samples.Length)
				{
					_haveLeftOverRightSample = true;
					_leftOverRightSample = unchecked((short)Math.Clamp(accumulator.Right >> 1, short.MinValue, short.MaxValue));
					break;
				}

				samples[i++] = unchecked((short)Math.Clamp(accumulator.Right >> 1, short.MinValue, short.MaxValue));
			}
		}

		_playbackRegisters.SampleNumber = lastSampleEmitted;
	}
}
