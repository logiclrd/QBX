using System;
using System.Runtime.CompilerServices;

namespace QBX.Hardware.AdLib;

using QBX.Utility;

// This source code is adapted from ymf262.c by Jarek Burczynski, which is licensed under GPL-2.0+.

public class YMF262Chip
{
	public const int OPLBankSize = 9;
	public const int OPLChannels = 18;

	public const uint RateBase = 49716u; // It's not a good idea to deviate from this.

	public const int BasePort = 0x388;

	public const uint RateDivisor = 288u;

	const int FrequencyShift         = 16;  /* 16.16 fixed point (frequency calculations) */
	const int EnvelopeGeneratorShift = 16;  /* 16.16 fixed point (EG timing)              */
	const int LFOShift               = 24;  /*  8.24 fixed point (LFO calculations)       */

	const int FrequencyMask = (1 << FrequencyShift) - 1;

	/* envelope output entries */
	const int EnvelopeBits      = 10;
	const int EnvelopeLength    = 1 << EnvelopeBits;
	internal const double EnvelopeStep   = 128.0 / EnvelopeLength;

	const int MaxAttenuationIndex = (1 << (EnvelopeBits - 1)) - 1; /* 511 */
	const int MinAttenuationIndex = 0;

	/* register number to channel number , slot offset */
	const int Slot1 = 0;
	const int Slot2 = 1;

	/* save output as raw 16-bit sample */

	struct EnvelopeGeneratorRate
	{
		public uint Rate; // Scaled by 2 bits
		public uint Mask;
		public byte Shift;
		public byte Select;
	}

	enum EnvelopeGeneratorType
	{
		NonPercussive,
		Percussive,
	}

	struct EnvelopeGeneratorFields
	{
		public EnvelopeGeneratorType Type;   /* percussive/non-percussive mode */
		public EnvelopeGeneratorPhase Phase; /* phase type                   */
		public uint TotalLevel;              /* total level: TL << 2         */
		public int  TotalLevelAdjusted;      /* adjusted now TL              */
		public int  Volume;                  /* envelope counter             */
		public uint SustainLevel;            /* sustain level: sl_tab[SL]    */
	}

	struct OPL3Slot
	{
		public byte  KeyScaleRateShift;   /* key scale rate               */
		public byte  KeyScaleLevel;       /* key scale level               */
		public byte  KeyScaleRate;        /* key scale rate: kcode>>KSR   */
		public byte  Multiple;            /* multiple: mul_tab[ML]        */

		/* Phase Generator */
		public uint  FrequencyTick;       /* frequency counter            */
		public uint  FrequencyStep;       /* frequency counter step       */
		public byte  FeedbackShift;       /* feedback shift value         */
		public MutableBox<short> OutputSlot;    /* slot output pointer          */
		public OPLSlotOutArray Operator1Output; /* slot1 output for feedback    */
		public byte  ConnectionType;      /* connection (algorithm) type  */

		/* Envelope Generator */
		public EnvelopeGeneratorFields EnvelopeGenerator;

		public EnvelopeGeneratorRate AttackRate;
		public EnvelopeGeneratorRate DecayRate;
		public EnvelopeGeneratorRate ReleaseRate;

		public uint Key;        /* 0 = KEY OFF, >0 = KEY ON     */

		/* LFO */
		public uint  AmplitudeModulationMask;     /* LFO Amplitude Modulation enable mask */
		public bool  Vibrate;        /* LFO Phase Modulation enable flag (active high)*/

		/* waveform select */
		public byte WaveformNumber;
		public uint WaveTable;

		[InlineArray(128 - 112)] //speedup: pump up the struct size to power of 2
		struct Padding { byte _element0; }
		Padding _reserved;
	}

	[InlineArray(2)]
	struct OPL3ChannelSlotArray
	{
		OPL3Slot _element0;
	}

	struct OPL3Channel
	{
		public OPL3ChannelSlotArray Slot;

		public uint  BlockAndFunctionNumber; /* block+fnum                   */
		public uint  FrequencyStepBase;      /* Freq. Increment base         */
		public uint  KeyScaleLevelBase;      /* KeyScaleLevel Base step      */
		public byte  KeyCode;                /* key code (for key scaling)   */

		/*
			there are 12 2-operator channels which can be combined in pairs
			to form six 4-operator channel, they are:
				0 and 3,
				1 and 4,
				2 and 5,
				9 and 12,
				10 and 13,
				11 and 14
		*/
		public bool IsExtended;   /* set to 1 if this channel forms up a 4op channel with another channel(only used by first of pair of channels, ie 0,1,2 and 9,10,11) */

		[InlineArray(512 - 272)] //speedup: pump up the struct size to power of 2
		struct Padding { byte _element0; }
		Padding _reserved;
	}

	/* OPL3 state */
	OPL3Channel[] _channels = new OPL3Channel[18];               /* OPL3 chips have 18 channels  */

	short[] _outputMasks = new short[18*4];          /* channels output masks (0xffff = enable); 4 masks per one channel */
	uint[]  _outputControlValues = new uint[18];     /* output control values 1 per one channel (1 value contains 4 masks) */

	MutableBox<short>[] _channelOutputSlots =
		new MutableBox<short>[18]
		{
			new MutableBox<short>(), new MutableBox<short>(), new MutableBox<short>(),
			new MutableBox<short>(), new MutableBox<short>(), new MutableBox<short>(),
			new MutableBox<short>(), new MutableBox<short>(), new MutableBox<short>(),

			new MutableBox<short>(), new MutableBox<short>(), new MutableBox<short>(),
			new MutableBox<short>(), new MutableBox<short>(), new MutableBox<short>(),
			new MutableBox<short>(), new MutableBox<short>(), new MutableBox<short>(),
		};

	MutableBox<short> _phaseModulation = new MutableBox<short>();   /* phase modulation input (SLOT 2) */
	MutableBox<short> _phaseModulation2 = new MutableBox<short>();  /* phase modulation input (SLOT 3 in 4 operator channels) */

	internal const int RateSteps = 8;

	struct GlobalEnvelopeGeneratorState
	{
		public uint Counter;
		public uint TimerTick; /* counter works at frequency = chipclock/288 (288=8*36) */
		public uint TimerStep;
		public uint TimerOverflow; /* envelope generator timer overflows every 1 sample (on real chip) */
	}

	GlobalEnvelopeGeneratorState _globalEnvelopeGenerator = new GlobalEnvelopeGeneratorState();

	uint[] _fnumberTable = new uint[1024];           /* fnumber->increment counter   */

	struct LowFrequencyOscillatorModulatorFields
	{
		public bool Depth
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => (DepthRange != 0);
			set => DepthRange = (value ? (byte)1 : (byte)0);
		}

		public byte DepthRange;
		public uint Tick;
		public uint Step;
	}

	struct LowFrequencyOscillatorFields
	{
		public uint CurrentAmplitude;
		public int CurrentPhase;

		public LowFrequencyOscillatorModulatorFields AmplitudeModulator;
		public LowFrequencyOscillatorModulatorFields PhaseModulator;
	}

	struct NoiseFields
	{
		public uint Value;              /* 23 bit noise shift register  */
		public uint Phase;              /* current noise 'phase'        */
		public uint Period;             /* current noise period         */
	}

	/* LFO */
	LowFrequencyOscillatorFields _lowFrequencyOscillator;

	NoiseFields _noise;

	bool   _opl3Mode;              /* OPL3 extension enable flag   */

	bool   _hasRhythmPart;                 /* Rhythm mode                  */

	struct TimerFields
	{
		public int Tick;
		public bool IsEnabled;
	}

	[InlineArray(length: 2)]
	struct Timers
	{
		TimerFields _element0;
	}

	Timers _timers;

	enum StatusFlags : byte
	{
		ST1 = 0x01,
		ST2 = 0x02,
		X = 0x04,
		BufferReady = 0x08,
		EOS = 0x10,
		TimerB = 0x20,
		TimerA = 0x40,
		IRQEnabled = 0x80,
	}

	int _selectedRegister;
	StatusFlags _status;
	StatusFlags _statusMask;

	byte _noteSelect;

	uint _clock;                   /* master clock  (Hz)           */
	uint _rate;                    /* sampling rate (Hz)           */
	double _frequencyBase;              /* frequency base               */

	internal const int EnvelopeQuiet = YMF262Tables.TLTableLength >> 4;

	/* work table */
	ref OPL3Slot Slot7_1 => ref _channels[7].Slot[Slot1];
	ref OPL3Slot Slot7_2 => ref _channels[7].Slot[Slot2];
	ref OPL3Slot Slot8_1 => ref _channels[8].Slot[Slot1];
	ref OPL3Slot Slot8_2 => ref _channels[8].Slot[Slot2];

	public YMF262Chip(int mixFrequency)
	{
		Initialize(RateBase * RateDivisor, (uint)mixFrequency);
	}

	void SetStatus(StatusFlags flag)
	{
		_status |= (flag & _statusMask);
		if ((_status & StatusFlags.IRQEnabled) != StatusFlags.IRQEnabled)
		{
			if ((_status & _statusMask) != 0)
			{
				/* IRQ on */
				_status |= StatusFlags.IRQEnabled;
				/* callback user interrupt handler (IRQ is OFF to ON) */
				//OnIRQ(true);
			}
		}
	}

	void ResetStatus(StatusFlags flag)
	{
		/* reset status flag */
		_status &= ~flag;
		if ((_status & StatusFlags.IRQEnabled) != StatusFlags.IRQEnabled)
		{
			if ((_status & _statusMask) != 0)
			{
				_status &= ~StatusFlags.IRQEnabled;
				/* callback user interrupt handler (IRQ is OFF to ON) */
				//OnIRQ(true);
			}
		}
	}

	/* IRQ mask set */
	void SetStatusMask(StatusFlags flag)
	{
		_statusMask = flag;
		/* IRQ handling check */
		SetStatus(0);
		ResetStatus(0);
	}

	void AdvanceLowFrequencyOscillator()
	{
		byte tmp;

		/* LFO */
		_lowFrequencyOscillator.AmplitudeModulator.Tick +=
			_lowFrequencyOscillator.AmplitudeModulator.Step;

		/* LFOAmplitudeModulatorTable is 210 elements long */
		if (_lowFrequencyOscillator.AmplitudeModulator.Tick >= ((uint)YMF262Tables.LFOAmplitudeModulatorTable.Length << LFOShift))
			_lowFrequencyOscillator.AmplitudeModulator.Tick -= ((uint)YMF262Tables.LFOAmplitudeModulatorTable.Length << LFOShift);

		tmp = YMF262Tables.LFOAmplitudeModulatorTable[_lowFrequencyOscillator.AmplitudeModulator.Tick >> LFOShift ];

		if (_lowFrequencyOscillator.AmplitudeModulator.Depth)
			_lowFrequencyOscillator.CurrentAmplitude = tmp;
		else
			_lowFrequencyOscillator.CurrentAmplitude = unchecked((uint)tmp) >> 2;

		_lowFrequencyOscillator.PhaseModulator.Tick += _lowFrequencyOscillator.PhaseModulator.Step;

		_lowFrequencyOscillator.CurrentPhase = unchecked((int)
			(((_lowFrequencyOscillator.PhaseModulator.Tick >> LFOShift) & 7) | _lowFrequencyOscillator.PhaseModulator.DepthRange));
	}

	/* advance to next sample */
	void AdvanceSample()
	{
		_globalEnvelopeGenerator.TimerTick += _globalEnvelopeGenerator.TimerStep;

		while (_globalEnvelopeGenerator.TimerTick >= _globalEnvelopeGenerator.TimerOverflow)
		{
			_globalEnvelopeGenerator.TimerTick -= _globalEnvelopeGenerator.TimerOverflow;
			_globalEnvelopeGenerator.Counter++;

			for (int i=0; i<9*2*2; i++)
			{
				ref var CH  = ref _channels[i / 2];
				ref var op  = ref CH.Slot[i & 1];

				/* Envelope Generator */
				switch (op.EnvelopeGenerator.Phase)
				{
					case EnvelopeGeneratorPhase.Attack:    /* attack phase */
						if ((_globalEnvelopeGenerator.Counter & op.AttackRate.Mask) == 0)
						{
							op.EnvelopeGenerator.Volume += (~op.EnvelopeGenerator.Volume *
														(YMF262Tables.EnvelopeGeneratorIncrements[op.AttackRate.Select + ((_globalEnvelopeGenerator.Counter >> op.AttackRate.Shift)&7)])
														) >> 3;

							if (op.EnvelopeGenerator.Volume <= MinAttenuationIndex)
							{
								op.EnvelopeGenerator.Volume = MinAttenuationIndex;
								op.EnvelopeGenerator.Phase = EnvelopeGeneratorPhase.Decay;
							}

						}
						break;

					case EnvelopeGeneratorPhase.Decay:    /* decay phase */
						if ((_globalEnvelopeGenerator.Counter & op.DecayRate.Mask) == 0)
						{
							op.EnvelopeGenerator.Volume += YMF262Tables.EnvelopeGeneratorIncrements[op.DecayRate.Select + ((_globalEnvelopeGenerator.Counter >> op.DecayRate.Shift)&7)];

							if (op.EnvelopeGenerator.Volume >= op.EnvelopeGenerator.SustainLevel)
								op.EnvelopeGenerator.Phase = EnvelopeGeneratorPhase.Sustain;

						}
						break;

					case EnvelopeGeneratorPhase.Sustain:    /* sustain phase */

						/* this is important behaviour:
						one can change percusive/non-percussive modes on the fly and
						the chip will remain in sustain phase - verified on real YM3812 */

						if (op.EnvelopeGenerator.Type == EnvelopeGeneratorType.Percussive)
						{
							/* during sustain phase chip adds Release Rate (in percussive mode) */
							if ((_globalEnvelopeGenerator.Counter & op.ReleaseRate.Mask) == 0)
							{
								op.EnvelopeGenerator.Volume += YMF262Tables.EnvelopeGeneratorIncrements[op.ReleaseRate.Select + ((_globalEnvelopeGenerator.Counter >> op.ReleaseRate.Shift)&7)];

								if (op.EnvelopeGenerator.Volume >= MaxAttenuationIndex)
									op.EnvelopeGenerator.Volume = MaxAttenuationIndex;
							}
							/* else do nothing in sustain phase */
						}
						break;

					case EnvelopeGeneratorPhase.Release:    /* release phase */
						if ((_globalEnvelopeGenerator.Counter & op.ReleaseRate.Mask) == 0)
						{
							op.EnvelopeGenerator.Volume += YMF262Tables.EnvelopeGeneratorIncrements[op.ReleaseRate.Select + ((_globalEnvelopeGenerator.Counter >> op.ReleaseRate.Shift)&7)];

							if ( op.EnvelopeGenerator.Volume >= MaxAttenuationIndex)
							{
								op.EnvelopeGenerator.Volume = MaxAttenuationIndex;
								op.EnvelopeGenerator.Phase = EnvelopeGeneratorPhase.Off;
							}

						}
						break;
				}
			}
		}

		for (int i=0; i<9*2*2; i++)
		{
			ref var CH  = ref _channels[i / 2];
			ref var op  = ref CH.Slot[i & 1];

			/* Phase Generator */
			if (op.Vibrate)
			{
				byte block;
				uint block_fnum = CH.BlockAndFunctionNumber;

				uint fnum_lfo   = (block_fnum&0x0380) >> 7;

				int lfo_fn_table_index_offset = YMF262Tables.LFOPhaseModulatorTable[_lowFrequencyOscillator.CurrentPhase + 16*fnum_lfo ];

				if (lfo_fn_table_index_offset != 0)  /* LFO phase modulation active */
				{
					block_fnum = unchecked((uint)(block_fnum + lfo_fn_table_index_offset));
					block = unchecked((byte)((block_fnum & 0x1c00) >> 10));
					op.FrequencyTick += (_fnumberTable[block_fnum&0x03ff] >> (7-block)) * op.Multiple;
				}
				else    /* LFO phase modulation  = zero */
				{
					op.FrequencyTick += op.FrequencyStep;
				}
			}
			else    /* LFO phase modulation disabled for this operator */
			{
				op.FrequencyTick += op.FrequencyStep;
			}
		}

		/*  The Noise Generator of the YM3812 is 23-bit shift register.
		*   Period is equal to 2^23-2 samples.
		*   Register works at sampling frequency of the chip, so output
		*   can change on every sample.
		*
		*   Output of the register and input to the bit 22 is:
		*   bit0 XOR bit14 XOR bit15 XOR bit22
		*
		*   Simply use bit 22 as the noise output.
		*/

		_noise.Phase += _noise.Period;
		uint ii = _noise.Phase >> FrequencyShift;       /* number of events (shifts of the shift register) */
		_noise.Phase &= FrequencyMask;
		while (ii != 0)
		{
			/*
			uint j;
			j = ( (noise_rng) ^ (noise_rng>>14) ^ (noise_rng>>15) ^ (noise_rng>>22) ) & 1;
			noise_rng = (j<<22) | (noise_rng>>1);
			*/

			/*
					Instead of doing all the logic operations above, we
					use a trick here (and use bit 0 as the noise output).
					The difference is only that the noise bit changes one
					step ahead. This doesn't matter since we don't know
					what is real state of the noise_rng after the reset.
			*/

			if ((_noise.Value & 1) != 0)
				_noise.Value ^= 0x800302;

			_noise.Value >>= 1;

			ii--;
		}
	}

	short CalculateOperator(uint phase, uint env, int pm, uint wave_tab)
	{
		uint p = (env<<4) + YMF262Tables.SineTable[wave_tab + ((((int)((phase & ~FrequencyMask) + (pm << 16))) >> FrequencyShift) & YMF262Tables.SineMask)];

		if (p >= YMF262Tables.TLTableLength)
			return 0;
		return YMF262Tables.TLTable[p];
	}

	short CalculateOperator1(uint phase, uint env, int pm, uint wave_tab)
	{
		uint p = (env<<4) + YMF262Tables.SineTable[wave_tab + ((((int)((phase & ~FrequencyMask) + pm)) >> FrequencyShift) & YMF262Tables.SineMask)];

		if (p >= YMF262Tables.TLTableLength)
			return 0;
		return YMF262Tables.TLTable[p];
	}

	uint CalculateVolume(ref OPL3Slot OP) => unchecked((uint)OP.EnvelopeGenerator.TotalLevelAdjusted) + ((uint)OP.EnvelopeGenerator.Volume) + (_lowFrequencyOscillator.CurrentAmplitude & OP.AmplitudeModulationMask);

	/* calculate output of a standard 2 operator channel
	(or 1st part of a 4-op channel) */
	void UpdateChannel(ref OPL3Channel CH)
	{
		_phaseModulation.Value  = 0;
		_phaseModulation2.Value = 0;

		/* SLOT 1 */
		{
			ref var SLOT = ref CH.Slot[Slot1];
			uint env  = CalculateVolume(ref SLOT);
			int @out  = SLOT.Operator1Output[0] + SLOT.Operator1Output[1];
			SLOT.Operator1Output[0] = SLOT.Operator1Output[1];
			SLOT.Operator1Output[1] = 0;
			if (env < EnvelopeQuiet)
			{
				if (SLOT.FeedbackShift == 0)
					@out = 0;
				SLOT.Operator1Output[1] = CalculateOperator1(SLOT.FrequencyTick, env, (@out<<SLOT.FeedbackShift), SLOT.WaveTable);
			}
			SLOT.OutputSlot.Value += SLOT.Operator1Output[1];
		}

		/* SLOT 2 */
		{
			ref var SLOT = ref CH.Slot[Slot2];
			uint env = CalculateVolume(ref SLOT);
			if (env < EnvelopeQuiet)
				SLOT.OutputSlot.Value += CalculateOperator(SLOT.FrequencyTick, env, _phaseModulation.Value, SLOT.WaveTable);
		}
	}

	/* calculate output of a 2nd part of 4-op channel */
	void UpdateExtendedChannel(ref OPL3Channel CH)
	{
		_phaseModulation.Value = 0;

		/* SLOT 1 */
		{
			ref var SLOT = ref CH.Slot[Slot1];
			uint env  = CalculateVolume(ref SLOT);
			if (env < EnvelopeQuiet)
				SLOT.OutputSlot.Value += CalculateOperator(SLOT.FrequencyTick, env, _phaseModulation2.Value, SLOT.WaveTable );
		}

		/* SLOT 2 */
		{
			ref var SLOT = ref CH.Slot[Slot2];
			uint env = CalculateVolume(ref SLOT);
			if (env < EnvelopeQuiet)
				SLOT.OutputSlot.Value += CalculateOperator(SLOT.FrequencyTick, env, _phaseModulation.Value, SLOT.WaveTable);
		}
	}

	/*
			operators used in the rhythm sounds generation process:

			Envelope Generator:

	channel  operator  register number   Bass  High  Snare Tom  Top
	/ slot   number    TL ARDR SLRR Wave Drum  Hat   Drum  Tom  Cymbal
	6 / 0   12        50  70   90   f0  +
	6 / 1   15        53  73   93   f3  +
	7 / 0   13        51  71   91   f1        +
	7 / 1   16        54  74   94   f4              +
	8 / 0   14        52  72   92   f2                    +
	8 / 1   17        55  75   95   f5                          +

			Phase Generator:

	channel  operator  register number   Bass  High  Snare Tom  Top
	/ slot   number    MULTIPLE          Drum  Hat   Drum  Tom  Cymbal
	6 / 0   12        30                +
	6 / 1   15        33                +
	7 / 0   13        31                      +     +           +
	7 / 1   16        34                -----  n o t  u s e d -----
	8 / 0   14        32                                  +
	8 / 1   17        35                      +                 +

	channel  operator  register number   Bass  High  Snare Tom  Top
	number   number    BLK/FNUM2 FNUM    Drum  Hat   Drum  Tom  Cymbal
		6     12,15     B6        A6      +

		7     13,16     B7        A7            +     +           +

		8     14,17     B8        A8            +           +     +

	*/

	void CalculateRhythm(OPL3Channel[] CH, bool noise)
	{
		/* Bass Drum (verified on real YM3812):
			- depends on the channel 6 'connect' register:
					when connect = 0 it works the same as in normal (non-rhythm) mode (op1->op2->out)
					when connect = 1 _only_ operator 2 is present on output (op2->out), operator 1 is ignored
			- output sample always is multiplied by 2
		*/

		int @out;

		_phaseModulation.Value = 0;

		/* SLOT 1 */
		{
			ref var SLOT = ref CH[6].Slot[Slot1];
			uint env = CalculateVolume(ref SLOT);

			@out = SLOT.Operator1Output[0] + SLOT.Operator1Output[1];
			SLOT.Operator1Output[0] = SLOT.Operator1Output[1];

			if (SLOT.ConnectionType == 0)
				_phaseModulation.Value = SLOT.Operator1Output[0];
			//else ignore output of operator 1

			SLOT.Operator1Output[1] = 0;
			if (env < EnvelopeQuiet)
			{
				if (SLOT.FeedbackShift == 0)
					@out = 0;
				SLOT.Operator1Output[1] = CalculateOperator1(SLOT.FrequencyTick, env, (@out << SLOT.FeedbackShift), SLOT.WaveTable );
			}
		}

		/* SLOT 2 */
		{
			ref var SLOT = ref CH[6].Slot[Slot2];
			uint env = CalculateVolume(ref SLOT);
			if (env < EnvelopeQuiet)
				_channelOutputSlots[6].Value += unchecked((short)(CalculateOperator(SLOT.FrequencyTick, env, _phaseModulation.Value, SLOT.WaveTable) * 2));
		}

		/* Phase generation is based on: */
		// HH  (13) channel 7->slot 1 combined with channel 8->slot 2 (same combination as TOP CYMBAL but different output phases)
		// SD  (16) channel 7->slot 1
		// TOM (14) channel 8->slot 1
		// TOP (17) channel 7->slot 1 combined with channel 8->slot 2 (same combination as HIGH HAT but different output phases)

		/* Envelope generation based on: */
		// HH  channel 7->slot1
		// SD  channel 7->slot2
		// TOM channel 8->slot1
		// TOP channel 8->slot2


		/* The following formulas can be well optimized.
			I leave them in direct form for now (in case I've missed something).
		*/

		{
			/* High Hat (verified on real YM3812) */
			uint env = CalculateVolume(ref Slot7_1);
			if (env < EnvelopeQuiet)
			{
				/* high hat phase generation:
						phase = d0 or 234 (based on frequency only)
						phase = 34 or 2d0 (based on noise)
				*/

				/* base frequency derived from operator 1 in channel 7 */
				uint bit7 = ((Slot7_1.FrequencyTick >> FrequencyShift) >> 7) & 1;
				uint bit3 = ((Slot7_1.FrequencyTick >> FrequencyShift) >> 3) & 1;
				uint bit2 = ((Slot7_1.FrequencyTick >> FrequencyShift) >> 2) & 1;

				uint res1 = (bit2 ^ bit7) | bit3;

				/* when res1 = 0 phase = 0x000 | 0xd0; */
				/* when res1 = 1 phase = 0x200 | (0xd0 >> 2); */
				uint phase = (res1 != 0) ? (0x200u | (0xd0 >> 2)) : 0xd0;

				/* enable gate based on frequency of operator 2 in channel 8 */
				uint bit5e= ((Slot8_2.FrequencyTick >> FrequencyShift) >> 5) & 1;
				uint bit3e= ((Slot8_2.FrequencyTick >> FrequencyShift) >> 3) & 1;

				uint res2 = (bit3e ^ bit5e);

				/* when res2 = 0 pass the phase from calculation above (res1); */
				/* when res2 = 1 pha != 0se = 0x200 | (0xd0>>2); */
				if (res2 != 0)
					phase = (0x200|(0xd0>>2));


				/* when phase & 0x200 is set and noise=1 then phase = 0x200|0xd0 */
				/* when phase & 0x200 is set and noise=0 then phase = 0x200|(0xd0>>2), ie no change */
				if ((phase & 0x200) != 0)
				{
					if (noise)
						phase = 0x200|0xd0;
				}
				else
				/* when phase & 0x200 is clear and noise=1 then phase = 0xd0>>2 */
				/* when phase & 0x200 is clear and noise=0 then phase = 0xd0, ie no change */
				{
					if (noise)
						phase = 0xd0>>2;
				}

				_channelOutputSlots[7].Value += unchecked((short)(CalculateOperator(phase<<FrequencyShift, env, 0, Slot7_1.WaveTable) * 2));
			}
		}

		{
			/* Snare Drum (verified on real YM3812) */
			uint env = CalculateVolume(ref Slot7_2);
			if (env < EnvelopeQuiet)
			{
				/* base frequency derived from operator 1 in channel 7 */
				bool bit8 = (((Slot7_1.FrequencyTick >> FrequencyShift) >> 8) & 1) != 0;

				/* when bit8 = 0 phase = 0x100; */
				/* when bit8 = 1 phase = 0x200; */
				uint phase = bit8 ? 0x200u : 0x100;

				/* Noise bit XOR'es phase by 0x100 */
				/* when noisebit = 0 pass the phase from calculation above */
				/* when noisebit = 1 phase ^= 0x100; */
				/* in other words: phase ^= (noisebit<<8); */
				if (noise)
					phase ^= 0x100;

				_channelOutputSlots[7].Value += unchecked((short)(CalculateOperator(phase << FrequencyShift, env, 0, Slot7_2.WaveTable) * 2));
			}
		}

		{
			/* Tom Tom (verified on real YM3812) */
			uint env = CalculateVolume(ref Slot8_1);
			if (env < EnvelopeQuiet)
				_channelOutputSlots[8].Value += unchecked((short)(CalculateOperator(Slot8_1.FrequencyTick, env, 0, Slot8_1.WaveTable) * 2));
		}

		{
			/* Top Cymbal (verified on real YM3812) */
			uint env = CalculateVolume(ref Slot8_2);
			if (env < EnvelopeQuiet)
			{
				/* base frequency derived from operator 1 in channel 7 */
				uint bit7 = ((Slot7_1.FrequencyTick>>FrequencyShift)>>7) & 1;
				uint bit3 = ((Slot7_1.FrequencyTick>>FrequencyShift)>>3) & 1;
				uint bit2 = ((Slot7_1.FrequencyTick>>FrequencyShift)>>2) & 1;

				uint res1 = (bit2 ^ bit7) | bit3;

				/* when res1 = 0 phase = 0x000 | 0x100; */
				/* when res1 = 1 phase = 0x200 | 0x100; */
				uint phase = (res1 != 0) ? 0x300u : 0x100;

				/* enable gate based on frequency of operator 2 in channel 8 */
				uint bit5e= ((Slot8_2.FrequencyTick>>FrequencyShift)>>5) & 1;
				uint bit3e= ((Slot8_2.FrequencyTick>>FrequencyShift)>>3) & 1;

				uint res2 = (bit3e ^ bit5e);
				/* when res2 = 0 pass the phase from calculation above (res1); */
				/* when res2 = 1 phase = 0x200 | 0x100; */
				if (res2 != 0)
					phase = 0x300;

				_channelOutputSlots[8].Value += unchecked((short)(CalculateOperator(phase << FrequencyShift, env, 0, Slot8_2.WaveTable) * 2));
			}
		}
	}

	static void OPLCloseTable()
	{
		// lalala
	}

	void InitializeGlobalTables()
	{
		/* frequency base */
		_frequencyBase  = (_rate != 0) ? ((double)_clock / (8.0*36)) / _rate  : 0;

		/* make fnumber -> increment counter table */
		for (int i = 0; i < 1024; i++)
		{
			/* opn phase increment counter = 20bit */
			_fnumberTable[i] = (uint)( (double)i * 64 * _frequencyBase * (1<<(FrequencyShift-10)) ); /* -10 because chip works with 10.10 fixed point, while we use 16.16 */
		}

		/* Amplitude modulation: 27 output levels (triangle waveform); 1 level takes one of: 192, 256 or 448 samples */
		/* One entry from LFO_AM_TABLE lasts for 64 samples */
		_lowFrequencyOscillator.AmplitudeModulator.Step = (uint)((1.0 / 64.0 ) * (1<<LFOShift) * _frequencyBase);

		/* Vibrato: 8 output levels (triangle waveform); 1 level takes 1024 samples */
		_lowFrequencyOscillator.PhaseModulator.Step = (uint)((1.0 / 1024.0) * (1<<LFOShift) * _frequencyBase);

		/* Noise generator: a step takes 1 sample */
		_noise.Period = (uint)((1.0 / 1.0) * (1<<FrequencyShift) * _frequencyBase);

		_globalEnvelopeGenerator.TimerStep = (uint)((1<<EnvelopeGeneratorShift)  * _frequencyBase);
		_globalEnvelopeGenerator.TimerOverflow = 1 << EnvelopeGeneratorShift;
	}

	void KeyOn(ref OPL3Slot SLOT, uint key_set)
	{
		if (SLOT.Key == 0)
		{
			/* restart Phase Generator */
			SLOT.FrequencyTick = 0;
			/* phase -> Attack */
			SLOT.EnvelopeGenerator.Phase = EnvelopeGeneratorPhase.Attack;
		}

		SLOT.Key |= key_set;
	}

	void KeyOff(ref OPL3Slot SLOT, uint key_clr)
	{
		if (SLOT.Key != 0)
		{
			SLOT.Key &= key_clr;

			if (SLOT.Key == 0)
			{
				/* phase -> Release */
				if (SLOT.EnvelopeGenerator.Phase > EnvelopeGeneratorPhase.Release)
					SLOT.EnvelopeGenerator.Phase = EnvelopeGeneratorPhase.Release;
			}
		}
	}

	/* update phase increment counter of operator (also update the EG rates if necessary) */
	void CalculateFrequencyControl(ref OPL3Channel CH, ref OPL3Slot SLOT)
	{
		/* (frequency) phase increment counter */
		SLOT.FrequencyStep = CH.FrequencyStepBase * SLOT.Multiple;

		byte ksr = unchecked((byte)(CH.KeyCode >> SLOT.KeyScaleRateShift));

		if (SLOT.KeyScaleRate != ksr)
		{
			SLOT.KeyScaleRate = ksr;

			/* calculate envelope generator rates */
			if ((SLOT.AttackRate.Rate + SLOT.KeyScaleRate) < 16+60)
			{
				SLOT.AttackRate.Shift  = YMF262Tables.EnvelopeGeneratorRateShift [SLOT.AttackRate.Rate + SLOT.KeyScaleRate ];
				SLOT.AttackRate.Mask   = (1u << SLOT.AttackRate.Shift) - 1;
				SLOT.AttackRate.Select = YMF262Tables.EnvelopeGeneratorRateSelect[SLOT.AttackRate.Rate + SLOT.KeyScaleRate ];
			}
			else
			{
				SLOT.AttackRate.Shift  = 0;
				SLOT.AttackRate.Mask   = (1u << SLOT.AttackRate.Shift) - 1;
				SLOT.AttackRate.Select = 13 * RateSteps;
			}
			SLOT.DecayRate.Shift = YMF262Tables.EnvelopeGeneratorRateShift  [SLOT.DecayRate.Rate + SLOT.KeyScaleRate];
			SLOT.DecayRate.Mask = (1u << SLOT.DecayRate.Shift) - 1;
			SLOT.DecayRate.Select = YMF262Tables.EnvelopeGeneratorRateSelect[SLOT.DecayRate.Rate + SLOT.KeyScaleRate];
			SLOT.ReleaseRate.Shift  = YMF262Tables.EnvelopeGeneratorRateShift [SLOT.ReleaseRate.Rate + SLOT.KeyScaleRate];
			SLOT.ReleaseRate.Mask   = (1u << SLOT.ReleaseRate.Shift) - 1;
			SLOT.ReleaseRate.Select = YMF262Tables.EnvelopeGeneratorRateSelect[SLOT.ReleaseRate.Rate + SLOT.KeyScaleRate];
		}
	}

	/* set multi,am,vib,EG-TYP,KSR,mul */
	void SetParameter(int slot, int v)
	{
		int CHslot = slot / 2;

		ref var CH = ref _channels[CHslot];
		ref var SLOT = ref CH.Slot[slot & 1];

		SLOT.Multiple = YMF262Tables.Multiple[v&0x0f];
		SLOT.KeyScaleRateShift = ((v & 0x10) != 0) ? (byte)0 : (byte)2;
		SLOT.EnvelopeGenerator.Type =
			(v & 0x20) switch
			{
				0x00 => EnvelopeGeneratorType.NonPercussive,
				0x20 => EnvelopeGeneratorType.Percussive,
				_ => throw new Exception("Sanity failure")
			};
		SLOT.Vibrate = ((v & 0x40) != 0);
		SLOT.AmplitudeModulationMask = ((v & 0x80) != 0) ? unchecked((byte)~0) : (byte)0;

		if (_opl3Mode)
		{
			int chan_no = slot / 2;

			/* in OPL3 mode */
			//DO THIS:
			//if this is one of the slots of 1st channel forming up a 4-op channel
			//do normal operation
			//else normal 2 operator function
			//OR THIS:
			//if this is one of the slots of 2nd channel forming up a 4-op channel
			//update it using channel data of 1st channel of a pair
			//else normal 2 operator function
			switch(chan_no)
			{
				case 0: case 1: case 2:
				case 9: case 10: case 11:
					if (CH.IsExtended)
					{
						/* normal */
						CalculateFrequencyControl(ref CH,ref SLOT);
					}
					else
					{
						/* normal */
						CalculateFrequencyControl(ref CH,ref SLOT);
					}
					break;
				case 3: case 4: case 5:
				case 12: case 13: case 14:
					if (_channels[CHslot - 3].IsExtended)
					{
						/* update this SLOT using frequency data for 1st channel of a pair */
						CalculateFrequencyControl(ref _channels[CHslot - 3],ref SLOT);
					}
					else
					{
						/* normal */
						CalculateFrequencyControl(ref CH,ref SLOT);
					}
					break;
				default:
					/* normal */
					CalculateFrequencyControl(ref CH,ref SLOT);
					break;
			}
		}
		else
		{
			/* in OPL2 mode */
			CalculateFrequencyControl(ref CH,ref SLOT);
		}
	}

	/* set ksl & tl */
	void SetKeyScaleLevelAndTotalLevel(int slot, int v)
	{
		ref var CH   = ref _channels[slot / 2];
		ref var SLOT = ref CH.Slot[slot & 1];

		SLOT.KeyScaleLevel = YMF262Tables.KeyScaleLevelShift[v >> 6];

		/* 7 bits TL (bit 6 = always 0) */
		SLOT.EnvelopeGenerator.TotalLevel = unchecked((uint)((v & 0x3f) << (EnvelopeBits - 1 - 7)));

		if (_opl3Mode)
		{
			int chan_no = slot / 2;

			/* in OPL3 mode */
			//DO THIS:
			//if this is one of the slots of 1st channel forming up a 4-op channel
			//do normal operation
			//else normal 2 operator function
			//OR THIS:
			//if this is one of the slots of 2nd channel forming up a 4-op channel
			//update it using channel data of 1st channel of a pair
			//else normal 2 operator function
			switch(chan_no)
			{
				case 0: case 1: case 2:
				case 9: case 10: case 11:
					if (CH.IsExtended)
					{
						/* normal */
						SLOT.EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(SLOT.EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase >> SLOT.KeyScaleLevel)));
					}
					else
					{
						/* normal */
						SLOT.EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(SLOT.EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase >> SLOT.KeyScaleLevel)));
					}
					break;
				case 3: case 4: case 5:
				case 12: case 13: case 14:
					if (_channels[chan_no - 3].IsExtended)
					{
						/* update this SLOT using frequency data for 1st channel of a pair */
						SLOT.EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(SLOT.EnvelopeGenerator.TotalLevel + (_channels[chan_no - 3].KeyScaleLevelBase >> SLOT.KeyScaleLevel)));
					}
					else
					{
						/* normal */
						SLOT.EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(SLOT.EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase >> SLOT.KeyScaleLevel)));
					}
					break;
				default:
					/* normal */
					SLOT.EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(SLOT.EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase >> SLOT.KeyScaleLevel)));
					break;
			}
		}
		else
		{
			/* in OPL2 mode */
			SLOT.EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(SLOT.EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase>>SLOT.KeyScaleLevel)));
		}

	}

	/* set attack rate & decay rate  */
	void SetAttackAndDecayRates(int slot, int v)
	{
		ref var CH   = ref _channels[slot / 2];
		ref var SLOT = ref CH.Slot[slot & 1];

		SLOT.AttackRate.Rate = ((v>>4) != 0) ? unchecked((uint)(16 + ((v>>4) << 2))) : 0u;

		if ((SLOT.AttackRate.Rate + SLOT.KeyScaleRate) < 16+60) /* verified on real YMF262 - all 15 x rates take "zero" time */
		{
			SLOT.AttackRate.Shift  = YMF262Tables.EnvelopeGeneratorRateShift [SLOT.AttackRate.Rate + SLOT.KeyScaleRate ];
			SLOT.AttackRate.Mask   = unchecked((uint)((1 << SLOT.AttackRate.Shift) - 1));
			SLOT.AttackRate.Select = YMF262Tables.EnvelopeGeneratorRateSelect[SLOT.AttackRate.Rate + SLOT.KeyScaleRate ];
		}
		else
		{
			SLOT.AttackRate.Shift  = 0;
			SLOT.AttackRate.Mask      = unchecked((uint)((1 << SLOT.AttackRate.Shift) - 1));
			SLOT.AttackRate.Select = 13 * RateSteps;
		}

		SLOT.DecayRate.Rate    = ((v & 0x0f) != 0) ? unchecked((uint)(16 + ((v & 0x0f) << 2))) : 0u;
		SLOT.DecayRate.Shift  = YMF262Tables.EnvelopeGeneratorRateShift [SLOT.DecayRate.Rate + SLOT.KeyScaleRate ];
		SLOT.DecayRate.Mask   = unchecked((uint)((1 << SLOT.DecayRate.Shift) - 1));
		SLOT.DecayRate.Select = YMF262Tables.EnvelopeGeneratorRateSelect[SLOT.DecayRate.Rate + SLOT.KeyScaleRate ];
	}

	/* set sustain level & release rate */
	void SetSustainLevelAndReleaseRate(int slot, int v)
	{
		ref var CH   = ref _channels[slot / 2];
		ref var SLOT = ref CH.Slot[slot & 1];

		SLOT.EnvelopeGenerator.SustainLevel  = YMF262Tables.SustainLevels[v >> 4];

		SLOT.ReleaseRate.Rate  = ((v & 0x0f) != 0) ? unchecked((uint)(16 + ((v & 0x0f) << 2))) : 0u;
		SLOT.ReleaseRate.Shift  = YMF262Tables.EnvelopeGeneratorRateShift [SLOT.ReleaseRate.Rate + SLOT.KeyScaleRate ];
		SLOT.ReleaseRate.Mask   = unchecked((uint)((1 << SLOT.ReleaseRate.Shift) - 1));
		SLOT.ReleaseRate.Select = YMF262Tables.EnvelopeGeneratorRateSelect[SLOT.ReleaseRate.Rate + SLOT.KeyScaleRate ];
	}

	/* write a value v to register r on OPL chip */
	void WriteRegister(int r, int v)
	{
		int ch_offset = 0;

		if ((r & 0x100) != 0)
		{
			switch(r)
			{
				case 0x101: /* test register */
					return;

				case 0x104: /* 6 channels enable */
				{
					bool prev;

					{
						ref var CH = ref _channels[0];    /* channel 0 */
						prev = CH.IsExtended;
						CH.IsExtended = ((v >> 0) & 1) != 0;
					}
					{
						ref var CH = ref _channels[1];          /* channel 1 */
						prev = CH.IsExtended;
						CH.IsExtended = ((v >> 1) & 1) != 0;
					}
					{
						ref var CH = ref _channels[2];          /* channel 2 */
						prev = CH.IsExtended;
						CH.IsExtended = ((v >> 2) & 1) != 0;
					}

					{
						ref var CH = ref _channels[9];    /* channel 9 */
						prev = CH.IsExtended;
						CH.IsExtended = ((v >> 3) & 1) != 0;
					}
					{
						ref var CH = ref _channels[10];            /* channel 10 */
						prev = CH.IsExtended;
						CH.IsExtended = ((v >> 4) & 1) != 0;
					}
					{
						ref var CH = ref _channels[11];            /* channel 11 */
						prev = CH.IsExtended;
						CH.IsExtended = ((v >> 5) & 1) != 0;
					}

					return;
				}

				case 0x105: /* OPL3 extensions enable register */

					_opl3Mode = (v & 0x01) != 0;   /* OPL3 mode when bit0=1 otherwise it is OPL2 mode */

					/* following behaviour was tested on real YMF262,
					switching OPL3/OPL2 modes on the fly:
					- does not change the waveform previously selected (unless when ....)
					- does not update CH.A, CH.B, CH.C and CH.D output selectors (registers c0-c8) (unless when ....)
					- does not disable channels 9-17 on OPL3->OPL2 switch
					- does not switch 4 operator channels back to 2 operator channels
					*/

					return;
			}

			ch_offset = 9;  /* register page #2 starts from channel 9 (counting from 0) */
		}

		/* adjust bus to 8 bits */
		r &= 0xff;
		v &= 0xff;


		switch(r&0xe0)
		{
			case 0x00:  /* 00-1f:control */
				switch(r&0x1f)
				{
					case 0x01:  /* test register */
						break;
					case 0x02:  /* Timer 1 */
						_timers[0].Tick = (256-v)*4;
						break;
					case 0x03:  /* Timer 2 */
						_timers[1].Tick = (256-v)*16;
						break;
					case 0x04:  /* IRQ clear / mask and Timer enable */
						if ((v & 0x80) != 0)
						{   /* IRQ flags clear */
							ResetStatus(StatusFlags.TimerA | StatusFlags.TimerB);
						}
						else
						{
							StatusFlags vf = (StatusFlags)v;

							/* set IRQ mask ,timer enable */
							bool st1 = (vf & StatusFlags.ST1) == StatusFlags.ST1;
							bool st2 = (vf & StatusFlags.ST2) == StatusFlags.ST2;

							/* IRQRST,T1MSK,t2MSK,x,x,x,ST2,ST1 */
							ResetStatus(vf & (StatusFlags.TimerA | StatusFlags.TimerB));
							SetStatusMask((~vf) & (StatusFlags.TimerA | StatusFlags.TimerB));

							/* timer 2 */
							if (_timers[1].IsEnabled != st2)
							{
								_timers[1].IsEnabled = st2;

								//double period = st2 ? TimerBase * _timers[1].Tick : 0.0;
								//OnTimer(1, period);
							}

							/* timer 1 */
							if (_timers[0].IsEnabled != st1)
							{
								_timers[0].IsEnabled = st1;

								//double period = st1 ? TimerBase * _timers[0].Tick : 0.0;
								//OnTimer(0, period);
							}
						}
						break;
					case 0x08:  /* x,NTS,x,x, x,x,x,x */
						_noteSelect = unchecked((byte)v);
						break;
				}
				break;
			case 0x20:  /* am ON, vib ON, ksr, eg_type, mul */
			{
				int slot = YMF262Tables.SlotByRegister[r&0x1f];
				if (slot < 0) return;
				SetParameter(slot + ch_offset*2, v);
				break;
			}
			case 0x40:
			{
				int slot = YMF262Tables.SlotByRegister[r&0x1f];
				if (slot < 0) return;
				SetKeyScaleLevelAndTotalLevel(slot + ch_offset*2, v);
				break;
			}
			case 0x60:
			{
				int slot = YMF262Tables.SlotByRegister[r&0x1f];
				if (slot < 0) return;
				SetAttackAndDecayRates(slot + ch_offset*2, v);
				break;
			}
			case 0x80:
			{
				int slot = YMF262Tables.SlotByRegister[r&0x1f];
				if (slot < 0) return;
				SetSustainLevelAndReleaseRate(slot + ch_offset*2, v);
				break;
			}
			case 0xa0:
			{
				if (r == 0xbd)          /* am depth, vibrato depth, r,bd,sd,tom,tc,hh */
				{
					if (ch_offset != 0) /* 0xbd register is present in set #1 only */
						return;

					_lowFrequencyOscillator.AmplitudeModulator.Depth = (v & 0x80) != 0;
					_lowFrequencyOscillator.PhaseModulator.DepthRange = ((v & 0x40) != 0) ? (byte)8 : (byte)0;

					_hasRhythmPart = (v & 0x20) != 0;

					if (_hasRhythmPart)
					{
						/* BD key on/off */
						if ((v & 0x10) != 0)
						{
							KeyOn(ref _channels[6].Slot[Slot1], 2);
							KeyOn(ref _channels[6].Slot[Slot2], 2);
						}
						else
						{
							KeyOff(ref _channels[6].Slot[Slot1], ~2u);
							KeyOff(ref _channels[6].Slot[Slot2], ~2u);
						}

						/* HH key on/off */
						if ((v & 0x01) != 0)
							KeyOn(ref _channels[7].Slot[Slot1], 2);
						else
							KeyOff(ref _channels[7].Slot[Slot1], ~2u);

						/* SD key on/off */
						if ((v & 0x08) != 0)
							KeyOn(ref _channels[7].Slot[Slot2], 2);
						else
							KeyOff(ref _channels[7].Slot[Slot2], ~2u);

						/* TOM key on/off */
						if ((v & 0x04) != 0)
							KeyOn(ref _channels[8].Slot[Slot1], 2);
						else
							KeyOff(ref _channels[8].Slot[Slot1], ~2u);

						/* TOP-CY key on/off */
						if ((v & 0x02) != 0)
							KeyOn(ref _channels[8].Slot[Slot2], 2);
						else
							KeyOff(ref _channels[8].Slot[Slot2], ~2u);
					}
					else
					{
						/* BD key off */
						KeyOff(ref _channels[6].Slot[Slot1], ~2u);
						KeyOff(ref _channels[6].Slot[Slot2], ~2u);
						/* HH key off */
						KeyOff(ref _channels[7].Slot[Slot1], ~2u);
						/* SD key off */
						KeyOff(ref _channels[7].Slot[Slot2], ~2u);
						/* TOM key off */
						KeyOff(ref _channels[8].Slot[Slot1], ~2u);
						/* TOP-CY off */
						KeyOff(ref _channels[8].Slot[Slot2], ~2u);
					}

					return;
				}

				/* keyon,block,fnum */
				if ((r&0x0f) > 8) return;
				int ch_num = (r&0x0f) + ch_offset;
				ref var CH = ref _channels[ch_num];

				uint block_fnum;

				if ((r & 0x10) == 0)
				{   /* a0-a8 */
					block_fnum  = (CH.BlockAndFunctionNumber & 0x1f00) | unchecked((uint)v);
				}
				else
				{   /* b0-b8 */
					block_fnum = ((unchecked((uint)v) & 0x1fu) << 8) | (CH.BlockAndFunctionNumber&0xff);

					if (_opl3Mode)
					{
						int chan_no = (r&0x0f) + ch_offset;

						/* in OPL3 mode */
						//DO THIS:
						//if this is 1st channel forming up a 4-op channel
						//ALSO keyon/off slots of 2nd channel forming up 4-op channel
						//else normal 2 operator function keyon/off
						//OR THIS:
						//if this is 2nd channel forming up 4-op channel just do nothing
						//else normal 2 operator function keyon/off
						switch(chan_no)
						{
						case 0: case 1: case 2:
						case 9: case 10: case 11:
							if (CH.IsExtended)
							{
								//if this is 1st channel forming up a 4-op channel
								//ALSO keyon/off slots of 2nd channel forming up 4-op channel
								if ((v & 0x20) != 0)
								{
									KeyOn(ref CH.Slot[Slot1], 1);
									KeyOn(ref CH.Slot[Slot2], 1);
									KeyOn(ref _channels[ch_num + 3].Slot[Slot1], 1);
									KeyOn(ref _channels[ch_num + 3].Slot[Slot2], 1);
								}
								else
								{
									KeyOff(ref CH.Slot[Slot1], ~1u);
									KeyOff(ref CH.Slot[Slot2], ~1u);
									KeyOff(ref _channels[ch_num + 3].Slot[Slot1], ~1u);
									KeyOff(ref _channels[ch_num + 3].Slot[Slot2], ~1u);
								}
							}
							else
							{
								//else normal 2 operator function keyon/off
								if ((v & 0x20) != 0)
								{
									KeyOn (ref CH.Slot[Slot1], 1);
									KeyOn (ref CH.Slot[Slot2], 1);
								}
								else
								{
									KeyOff(ref CH.Slot[Slot1], ~1u);
									KeyOff(ref CH.Slot[Slot2], ~1u);
								}
							}
						break;

						case 3: case 4: case 5:
						case 12: case 13: case 14:
							if (_channels[ch_num - 3].IsExtended)
							{
								//if this is 2nd channel forming up 4-op channel just do nothing
							}
							else
							{
								//else normal 2 operator function keyon/off
								if ((v & 0x20) != 0)
								{
									KeyOn (ref CH.Slot[Slot1], 1);
									KeyOn (ref CH.Slot[Slot2], 1);
								}
								else
								{
									KeyOff(ref CH.Slot[Slot1], ~1u);
									KeyOff(ref CH.Slot[Slot2], ~1u);
								}
							}
						break;

						default:
							if ((v & 0x20) != 0)
							{
								KeyOn (ref CH.Slot[Slot1], 1);
								KeyOn (ref CH.Slot[Slot2], 1);
							}
							else
							{
								KeyOff(ref CH.Slot[Slot1], ~1u);
								KeyOff(ref CH.Slot[Slot2], ~1u);
							}
						break;
						}
					}
					else
					{
						if ((v & 0x20) != 0)
						{
							KeyOn (ref CH.Slot[Slot1], 1);
							KeyOn (ref CH.Slot[Slot2], 1);
						}
						else
						{
							KeyOff(ref CH.Slot[Slot1], ~1u);
							KeyOff(ref CH.Slot[Slot2], ~1u);
						}
					}
				}
				/* update */
				if (CH.BlockAndFunctionNumber != block_fnum)
				{
					byte block  = unchecked((byte)(block_fnum >> 10));

					CH.BlockAndFunctionNumber = block_fnum;

					CH.KeyScaleLevelBase = (uint)(YMF262Tables.KeyScaleLevelTable[block_fnum>>6]);
					CH.FrequencyStepBase       = _fnumberTable[block_fnum&0x03ff] >> (7-block);

					/* BLK 2,1,0 bits -> bits 3,2,1 of kcode */
					CH.KeyCode    = unchecked((byte)((CH.BlockAndFunctionNumber & 0x1c00) >> 9));

					/* the info below is actually opposite to what is stated in the Manuals (verifed on real YMF262) */
					/* if notesel == 0 -> lsb of kcode is bit 10 (MSB) of fnum  */
					/* if notesel == 1 -> lsb of kcode is bit 9 (MSB-1) of fnum */
					if ((_noteSelect & 0x40) != 0)
						CH.KeyCode |= unchecked((byte)((CH.BlockAndFunctionNumber & 0x100) >> 8)); /* notesel == 1 */
					else
						CH.KeyCode |= unchecked((byte)((CH.BlockAndFunctionNumber & 0x200) >> 9)); /* notesel == 0 */

					if (_opl3Mode)
					{
						int chan_no = (r&0x0f) + ch_offset;
						/* in OPL3 mode */
						//DO THIS:
						//if this is 1st channel forming up a 4-op channel
						//ALSO update slots of 2nd channel forming up 4-op channel
						//else normal 2 operator function keyon/off
						//OR THIS:
						//if this is 2nd channel forming up 4-op channel just do nothing
						//else normal 2 operator function keyon/off
						switch(chan_no)
						{
						case 0: case 1: case 2:
						case 9: case 10: case 11:
							if (CH.IsExtended)
							{
								//if this is 1st channel forming up a 4-op channel
								//ALSO update slots of 2nd channel forming up 4-op channel

								/* refresh Total Level in FOUR SLOTs of this channel and channel+3 using data from THIS channel */
								CH.Slot[Slot1].EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(CH.Slot[Slot1].EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase>>CH.Slot[Slot1].KeyScaleLevel)));
								CH.Slot[Slot2].EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(CH.Slot[Slot2].EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase>>CH.Slot[Slot2].KeyScaleLevel)));
								_channels[ch_num + 3].Slot[Slot1].EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(_channels[ch_num + 3].Slot[Slot1].EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase>>_channels[ch_num + 3].Slot[Slot1].KeyScaleLevel)));
								_channels[ch_num + 3].Slot[Slot2].EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(_channels[ch_num + 3].Slot[Slot2].EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase>>_channels[ch_num + 3].Slot[Slot2].KeyScaleLevel)));

								/* refresh frequency counter in FOUR SLOTs of this channel and channel+3 using data from THIS channel */
								CalculateFrequencyControl(ref CH, ref CH.Slot[Slot1]);
								CalculateFrequencyControl(ref CH, ref CH.Slot[Slot2]);
								CalculateFrequencyControl(ref CH, ref _channels[ch_num + 3].Slot[Slot1]);
								CalculateFrequencyControl(ref CH, ref _channels[ch_num + 3].Slot[Slot2]);
							}
							else
							{
								//else normal 2 operator function
								/* refresh Total Level in both SLOTs of this channel */
								CH.Slot[Slot1].EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(CH.Slot[Slot1].EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase>>CH.Slot[Slot1].KeyScaleLevel)));
								CH.Slot[Slot2].EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(CH.Slot[Slot2].EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase>>CH.Slot[Slot2].KeyScaleLevel)));

								/* refresh frequency counter in both SLOTs of this channel */
								CalculateFrequencyControl(ref CH, ref CH.Slot[Slot1]);
								CalculateFrequencyControl(ref CH, ref CH.Slot[Slot2]);
							}
						break;

						case 3: case 4: case 5:
						case 12: case 13: case 14:
							if (_channels[ch_num - 3].IsExtended)
							{
								//if this is 2nd channel forming up 4-op channel just do nothing
							}
							else
							{
								//else normal 2 operator function
								/* refresh Total Level in both SLOTs of this channel */
								CH.Slot[Slot1].EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(CH.Slot[Slot1].EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase>>CH.Slot[Slot1].KeyScaleLevel)));
								CH.Slot[Slot2].EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(CH.Slot[Slot2].EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase>>CH.Slot[Slot2].KeyScaleLevel)));

								/* refresh frequency counter in both SLOTs of this channel */
								CalculateFrequencyControl(ref CH, ref CH.Slot[Slot1]);
								CalculateFrequencyControl(ref CH, ref CH.Slot[Slot2]);
							}
						break;

						default:
							/* refresh Total Level in both SLOTs of this channel */
							CH.Slot[Slot1].EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(CH.Slot[Slot1].EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase>>CH.Slot[Slot1].KeyScaleLevel)));
							CH.Slot[Slot2].EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(CH.Slot[Slot2].EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase>>CH.Slot[Slot2].KeyScaleLevel)));

							/* refresh frequency counter in both SLOTs of this channel */
							CalculateFrequencyControl(ref CH, ref CH.Slot[Slot1]);
							CalculateFrequencyControl(ref CH, ref CH.Slot[Slot2]);
						break;
						}
					}
					else
					{
						/* in OPL2 mode */

						/* refresh Total Level in both SLOTs of this channel */
						CH.Slot[Slot1].EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(CH.Slot[Slot1].EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase>>CH.Slot[Slot1].KeyScaleLevel)));
						CH.Slot[Slot2].EnvelopeGenerator.TotalLevelAdjusted = unchecked((int)(CH.Slot[Slot2].EnvelopeGenerator.TotalLevel + (CH.KeyScaleLevelBase>>CH.Slot[Slot2].KeyScaleLevel)));

						/* refresh frequency counter in both SLOTs of this channel */
						CalculateFrequencyControl(ref CH, ref CH.Slot[Slot1]);
						CalculateFrequencyControl(ref CH, ref CH.Slot[Slot2]);
					}
				}
				break;
			}
			case 0xc0:
			{
				/* CH.D, CH.C, CH.B, CH.A, FB(3bits), C */
				if ((r & 0xf) > 8) return;

				int ch_num = (r & 0xf) + ch_offset;

				ref var CH = ref _channels[ch_num];

				const short ON = unchecked((short)0xFFFF);
				const short OFF = 0;

				if (_opl3Mode)
				{
					int @base = ((r&0xf) + ch_offset) * 4;

					/* OPL3 mode */
					_outputMasks[@base    ] = ((v & 0x10) != 0) ? ON : OFF; /* ch.A */
					_outputMasks[@base + 1] = ((v & 0x20) != 0) ? ON : OFF; /* ch.B */
					_outputMasks[@base + 2] = ((v & 0x40) != 0) ? ON : OFF; /* ch.C */
					_outputMasks[@base + 3] = ((v & 0x80) != 0) ? ON : OFF; /* ch.D */
				}
				else
				{
					int @base = ((r&0xf) + ch_offset) * 4;

					/* OPL2 mode - always enabled */
					_outputMasks[@base    ] = ON;      /* ch.A */
					_outputMasks[@base + 1] = ON;      /* ch.B */
					_outputMasks[@base + 2] = ON;      /* ch.C */
					_outputMasks[@base + 3] = ON;      /* ch.D */
				}

				_outputControlValues[(r & 0xf) + ch_offset] = unchecked((uint)v);    /* store control value for OPL3/OPL2 mode switching on the fly */

				CH.Slot[Slot1].FeedbackShift = unchecked((byte)((((v >> 1) & 7) != 0) ? ((v >> 1) & 7) + 7 : 0));
				CH.Slot[Slot1].ConnectionType = unchecked((byte)(v & 1));

				if (_opl3Mode)
				{
					int chan_no = (r&0x0f) + ch_offset;

					switch(chan_no)
					{
						case 0: case 1: case 2:
						case 9: case 10: case 11:
							if (CH.IsExtended)
							{
								byte conn = unchecked((byte)((CH.Slot[Slot1].ConnectionType << 1) | (_channels[ch_num + 3].Slot[Slot1].ConnectionType << 0)));

								switch (conn)
								{
									case 0:
										/* 1 -> 2 -> 3 -> 4 - out */

										CH.Slot[Slot1].OutputSlot = _phaseModulation;
										CH.Slot[Slot2].OutputSlot = _phaseModulation2;
										_channels[ch_num + 3].Slot[Slot1].OutputSlot = _phaseModulation;
										_channels[ch_num + 3].Slot[Slot2].OutputSlot = _channelOutputSlots[chan_no + 3];
										break;
									case 1:
										// 1 -> 2 -.
										// 3 -> 4 -+- out */

										CH.Slot[Slot1].OutputSlot = _phaseModulation;
										CH.Slot[Slot2].OutputSlot = _channelOutputSlots[chan_no];
										_channels[ch_num + 3].Slot[Slot1].OutputSlot = _phaseModulation;
										_channels[ch_num + 3].Slot[Slot2].OutputSlot = _channelOutputSlots[chan_no + 3];
										break;
									case 2:
										// 1 -----------.
										// 2 -> 3 -> 4 -+- out */

										CH.Slot[Slot1].OutputSlot = _channelOutputSlots[chan_no];
										CH.Slot[Slot2].OutputSlot = _phaseModulation2;
										_channels[ch_num + 3].Slot[Slot1].OutputSlot = _phaseModulation;
										_channels[ch_num + 3].Slot[Slot2].OutputSlot = _channelOutputSlots[chan_no + 3];
										break;
									case 3:
										// 1 ------.
										// 2 -> 3 -+- out
										// 4 ------'

										CH.Slot[Slot1].OutputSlot = _channelOutputSlots[chan_no];
										CH.Slot[Slot2].OutputSlot = _phaseModulation2;
										_channels[ch_num + 3].Slot[Slot1].OutputSlot = _channelOutputSlots[chan_no + 3];
										_channels[ch_num + 3].Slot[Slot2].OutputSlot = _channelOutputSlots[chan_no + 3];
										break;
								}
							}
							else
							{
								/* 2 operators mode */
								CH.Slot[Slot1].OutputSlot = (CH.Slot[Slot1].ConnectionType != 0) ? _channelOutputSlots[(r & 0xf) + ch_offset] : _phaseModulation;
								CH.Slot[Slot2].OutputSlot = _channelOutputSlots[(r & 0xf) + ch_offset];
							}
							break;

						case 3: case 4: case 5:
						case 12: case 13: case 14:
							if (_channels[ch_num - 3].IsExtended)
							{
								byte conn = unchecked((byte)((_channels[ch_num - 3].Slot[Slot1].ConnectionType << 1) | (CH.Slot[Slot1].ConnectionType << 0)));
								switch (conn)
								{
									case 0:
										/* 1 -> 2 -> 3 -> 4 - out */

										_channels[ch_num - 3].Slot[Slot1].OutputSlot = _phaseModulation;
										_channels[ch_num - 3].Slot[Slot2].OutputSlot = _phaseModulation2;
										CH.Slot[Slot1].OutputSlot = _phaseModulation;
										CH.Slot[Slot2].OutputSlot = _channelOutputSlots[chan_no];
										break;
									case 1:
										/* 1 -> 2 -\
											3 -> 4 -+- out */

										_channels[ch_num - 3].Slot[Slot1].OutputSlot = _phaseModulation;
										_channels[ch_num - 3].Slot[Slot2].OutputSlot = _channelOutputSlots[chan_no - 3];
										CH.Slot[Slot1].OutputSlot = _phaseModulation;
										CH.Slot[Slot2].OutputSlot = _channelOutputSlots[chan_no];
										break;
									case 2:
										/* 1 -----------\
											2 -> 3 -> 4 -+- out */

										_channels[ch_num - 3].Slot[Slot1].OutputSlot = _channelOutputSlots[ chan_no - 3 ];
										_channels[ch_num - 3].Slot[Slot2].OutputSlot = _phaseModulation2;
										CH.Slot[Slot1].OutputSlot = _phaseModulation;
										CH.Slot[Slot2].OutputSlot = _channelOutputSlots[chan_no];
										break;
									case 3:
										/* 1 ------\
											2 -> 3 -+- out
											4 ------/     */
										_channels[ch_num - 3].Slot[Slot1].OutputSlot = _channelOutputSlots[chan_no - 3];
										_channels[ch_num - 3].Slot[Slot2].OutputSlot = _phaseModulation2;
										CH.Slot[Slot1].OutputSlot = _channelOutputSlots[chan_no];
										CH.Slot[Slot2].OutputSlot = _channelOutputSlots[chan_no];
										break;
								}
							}
							else
							{
								/* 2 operators mode */
								CH.Slot[Slot1].OutputSlot = (CH.Slot[Slot1].ConnectionType != 0) ? _channelOutputSlots[(r & 0xf) + ch_offset] : _phaseModulation;
								CH.Slot[Slot2].OutputSlot = _channelOutputSlots[(r & 0xf) + ch_offset];
							}
							break;

						default:
							/* 2 operators mode */
							CH.Slot[Slot1].OutputSlot = (CH.Slot[Slot1].ConnectionType != 0) ? _channelOutputSlots[(r & 0xf) + ch_offset] : _phaseModulation;
							CH.Slot[Slot2].OutputSlot = _channelOutputSlots[(r & 0xf) + ch_offset];
							break;
					}
				}
				else
				{
					/* OPL2 mode - always 2 operators mode */
					CH.Slot[Slot1].OutputSlot = (CH.Slot[Slot1].ConnectionType != 0) ? _channelOutputSlots[(r & 0xf) + ch_offset] : _phaseModulation;
					CH.Slot[Slot2].OutputSlot = _channelOutputSlots[(r & 0xf) + ch_offset];
				}
				break;
			}
			case 0xe0: /* waveform select */
			{
				int slot = YMF262Tables.SlotByRegister[r&0x1f];
				if (slot < 0)
					return;

				slot += ch_offset*2;

				ref var CH = ref _channels[slot / 2];


				/* store 3-bit value written regardless of current OPL2 or OPL3 mode... (verified on real YMF262) */
				v &= 7;
				CH.Slot[slot & 1].WaveformNumber = unchecked((byte)v);

				/* ... but select only waveforms 0-3 in OPL2 mode */
				if (!_opl3Mode)
				{
					v &= 3; /* we're in OPL2 mode */
				}
				CH.Slot[slot & 1].WaveTable = unchecked((uint)(v * YMF262Tables.SineLength));
				break;
			}
		}
	}

	/* lock/unlock for common table */
	bool LockTable()
	{
		YMF262Tables.LockCount++;

		if (YMF262Tables.LockCount > 1)
			return true;

		/* first time */

		if (!YMF262Tables.InitializeTables())
		{
			YMF262Tables.LockCount--;
			return false;
		}

		return true;
	}

	void UnlockTable()
	{
		if (YMF262Tables.LockCount != 0) YMF262Tables.LockCount--;
		if (YMF262Tables.LockCount != 0) return;

		/* last time */
		OPLCloseTable();
	}

	public void ResetChip()
	{
		int c,s;

		_globalEnvelopeGenerator.Counter = 0;
		_globalEnvelopeGenerator.TimerTick = 0;

		_noise.Value = 1;    /* noise shift register */
		_noteSelect       = 0;    /* note split */
		ResetStatus(StatusFlags.TimerA | StatusFlags.TimerB);

		/* reset with register write */
		WriteRegister(0x01, 0); /* test register */
		WriteRegister(0x02, 0); /* Timer1 */
		WriteRegister(0x03, 0); /* Timer2 */
		WriteRegister(0x04, 0); /* IRQ mask clear */


		//FIX IT  registers 101, 104 and 105


		//FIX IT (dont change CH.D, CH.C, CH.B and CH.A in C0-C8 registers)
		for (c = 0xff; c >= 0x20; c--)
			WriteRegister(c, 0);
		//FIX IT (dont change CH.D, CH.C, CH.B and CH.A in C0-C8 registers)
		for (c = 0x1ff; c >= 0x120; c-- )
			WriteRegister(c, 0);

		/* reset operator parameters */
		for (c = 0; c < 9*2; c++)
		{
			ref OPL3Channel CH = ref _channels[c];

			for (s = 0; s < 2; s++ )
			{
				CH.Slot[s].EnvelopeGenerator.Phase  = EnvelopeGeneratorPhase.Off;
				CH.Slot[s].EnvelopeGenerator.Volume = MaxAttenuationIndex;
			}
		}
	}

	/* Create one of virtual YMF262 */
	/* 'clock' is chip clock in Hz  */
	/* 'rate'  is sampling rate  */
	void Initialize(uint clock, uint rate)
	{
		if (!LockTable())
			throw new Exception("LockTable failed");

		_clock = clock;
		_rate  = rate;

		/* init global tables */
		InitializeGlobalTables();

		ResetChip();
	}

	/* YMF262 I/O interface */
	public bool OutPort(int portNumber, byte data)
	{
		switch (portNumber & 3)
		{
			case 0: /* address port 0 (register set #1) */
				_selectedRegister = data;
				break;

			case 1: /* data port - ignore A1 */
			case 3: /* data port - ignore A1 */
				//OnUpdate(0);
				WriteRegister(_selectedRegister, data);
				break;

			case 2: /* address port 1 (register set #2) */

				/* verified on real YMF262:
				in OPL3 mode:
					address line A1 is stored during *address* write and ignored during *data* write.

				in OPL2 mode:
					register set#2 writes go to register set#1 (ignoring A1)
					verified on registers from set#2: 0x01, 0x04, 0x20-0xef
					The only exception is register 0x05.
				*/
				if (_opl3Mode)
				{
					/* OPL3 mode */
					_selectedRegister = data | 0x100;
				}
				else
				{
					/* in OPL2 mode the only accessible in set #2 is register 0x05 */
					if (data == 5)
						_selectedRegister = data | 0x100;
					else
						_selectedRegister = data;  /* verified range: 0x01, 0x04, 0x20-0xef(set #2 becomes set #1 in opl2 mode) */
				}
				break;
		}

		return (_status & StatusFlags.IRQEnabled) == StatusFlags.IRQEnabled;
	}

	public byte InPort(int portNumber)
	{
		/* Note on status register: */

		/* YM3526(OPL) and YM3812(OPL2) return bit2 and bit1 in HIGH state */

		/* YMF262(OPL3) always returns bit2 and bit1 in LOW state */
		/* which can be used to identify the chip */

		/* YMF278(OPL4) returns bit2 in LOW and bit1 in HIGH state ??? info from manual - not verified */

		if (portNumber == 0)
		{
			/* status port */
			return unchecked((byte)_status);
		}

		return 0x00;    /* verified on real YMF262 */
	}

	public bool TimerOver(int c)
	{
		if (c != 0)
		{
			/* Timer B */
			SetStatus(StatusFlags.TimerB);
		}
		else
		{
			/* Timer A */
			SetStatus(StatusFlags.TimerA);
		}

		/* reload timer */
		//OnTimer(c, TimerBase * T[c]);

		return (_status & StatusFlags.IRQEnabled) == StatusFlags.IRQEnabled;
	}

	public void GetMoreSound(Span<short> samples)
	{
		while (samples.Length >= 1)
		{
			AdvanceLowFrequencyOscillator();

			/* clear channel outputs */
			for (int j=0; j < _channelOutputSlots.Length; j++)
				_channelOutputSlots[j].Value = 0;

			/* register set #1 */
			UpdateChannel(ref _channels[0]);            /* extended 4op ch#0 part 1 or 2op ch#0 */
			if (_channels[0].IsExtended)
				UpdateExtendedChannel(ref _channels[3]);    /* extended 4op ch#0 part 2 */
			else
				UpdateChannel(ref _channels[3]);        /* standard 2op ch#3 */

			UpdateChannel(ref _channels[1]);            /* extended 4op ch#1 part 1 or 2op ch#1 */
			if (_channels[1].IsExtended)
				UpdateExtendedChannel(ref _channels[4]);    /* extended 4op ch#1 part 2 */
			else
				UpdateChannel(ref _channels[4]);        /* standard 2op ch#4 */

			UpdateChannel(ref _channels[2]);            /* extended 4op ch#2 part 1 or 2op ch#2 */
			if (_channels[2].IsExtended)
				UpdateExtendedChannel(ref _channels[5]);    /* extended 4op ch#2 part 2 */
			else
				UpdateChannel(ref _channels[5]);        /* standard 2op ch#5 */

			if (!_hasRhythmPart)
			{
				UpdateChannel(ref _channels[6]);
				UpdateChannel(ref _channels[7]);
				UpdateChannel(ref _channels[8]);
			}
			else
			{
				/* Rhythm part */
				CalculateRhythm(_channels, (_noise.Value & 1) != 0);
			}

			/* register set #2 */
			UpdateChannel(ref _channels[ 9]);
			if (_channels[9].IsExtended)
				UpdateExtendedChannel(ref _channels[12]);
			else
				UpdateChannel(ref _channels[12]);


			UpdateChannel(ref _channels[10]);
			if (_channels[10].IsExtended)
				UpdateExtendedChannel(ref _channels[13]);
			else
				UpdateChannel(ref _channels[13]);


			UpdateChannel(ref _channels[11]);
			if (_channels[11].IsExtended)
				UpdateExtendedChannel(ref _channels[14]);
			else
				UpdateChannel(ref _channels[14]);


			/* channels 15,16,17 are fixed 2-operator channels only */
			UpdateChannel(ref _channels[15]);
			UpdateChannel(ref _channels[16]);
			UpdateChannel(ref _channels[17]);

			/* accumulate register set #1 */
			for (int j = 0, k = 0; j < 18; j++)
			{
				samples[0] += unchecked((short)(_channelOutputSlots[j].Value & _outputMasks[k++]));
				samples[1] += unchecked((short)(_channelOutputSlots[j].Value & _outputMasks[k++]));
				k += 2; // skip next two pans
			}

			AdvanceSample();

			samples = samples.Slice(2);
		}
	}
}
