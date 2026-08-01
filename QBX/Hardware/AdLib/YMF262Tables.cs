using System;
using System.Runtime.CompilerServices;

namespace QBX.Hardware.AdLib;

public static class YMF262Tables
{
	/* sinwave entries */
	public const int SineBits      = 10;
	public const int SineLength    = 1 << SineBits;
	public const int SineMask      = SineLength - 1;

	/* sin waveform table in 'decibel' scale */
	/* there are eight waveforms on OPL3 chips */
	[InlineArray(length: SineLength * 8)]
	public struct SineTableArray { int _element0; }

	public static SineTableArray SineTable;

	static void InitializeSineTable()
	{
		for (int i=0; i<SineLength; i++)
		{
			/* non-standard sinus */
			double m = Math.Sin( ((i*2)+1) * Math.PI / SineLength ); /* checked against the real chip */

			/* we never reach zero here due to ((i*2)+1) */
			int o;

			if (m > 0.0)
				o = (int)(8 * Math.Log(1.0 / m) / Math.Log(2.0));  /* convert to 'decibels' */
			else
				o = (int)(8 * Math.Log(-1.0 / m) / Math.Log(2.0)); /* convert to 'decibels' */

			o = (int)(o / (YMF262Chip.EnvelopeStep / 4));

			int n = (int)(2.0*o);
			if (((n & 1) != 0))                        /* round to nearest */
				n = (n>>1)+1;
			else
				n = n>>1;

			SineTable[i] = n * 2 + (m >= 0.0 ? 0 : 1);
		}

		for (int i=0; i<SineLength; i++)
		{
			/* these 'pictures' represent _two_ cycles */
			/* waveform 1:  __      __     */
			/*             /  \____/  \____*/
			/* output only first half of the sinus waveform (positive one) */

			if ((i & (1<<(SineBits-1))) != 0)
				SineTable[1*SineLength+i] = TLTableLength;
			else
				SineTable[1*SineLength+i] = SineTable[i];

			/* waveform 2:  __  __  __  __ */
			/*             /  \/  \/  \/  \*/
			/* abs(sin) */

			SineTable[2*SineLength+i] = SineTable[i & (SineMask>>1) ];

			/* waveform 3:  _   _   _   _  */
			/*             / |_/ |_/ |_/ |_*/
			/* abs(output only first quarter of the sinus waveform) */

			if ((i & (1<<(SineBits-2))) != 0)
				SineTable[3*SineLength+i] = TLTableLength;
			else
				SineTable[3*SineLength+i] = SineTable[i & (SineMask>>2)];

			/* waveform 4:                 */
			/*             /\  ____/\  ____*/
			/*               \/      \/    */
			/* output whole sinus waveform in half the cycle(step=2) and output 0 on the other half of cycle */

			if ((i & (1<<(SineBits-1))) != 0)
				SineTable[4*SineLength+i] = TLTableLength;
			else
				SineTable[4*SineLength+i] = SineTable[i*2];

			/* waveform 5:                 */
			/*             /\/\____/\/\____*/
			/*                             */
			/* output abs(whole sinus) waveform in half the cycle(step=2) and output 0 on the other half of cycle */

			if ((i & (1<<(SineBits-1))) != 0)
				SineTable[5*SineLength+i] = TLTableLength;
			else
				SineTable[5*SineLength+i] = SineTable[(i*2) & (SineMask>>1) ];

			/* waveform 6: ____    ____    */
			/*                             */
			/*                 ____    ____*/
			/* output maximum in half the cycle and output minimum on the other half of cycle */

			if ((i & (1<<(SineBits-1))) != 0)
				SineTable[6*SineLength+i] = 1;   /* negative */
			else
				SineTable[6*SineLength+i] = 0;   /* positive */

			/* waveform 7:                 */
			/*             |\____  |\____  */
			/*                   \|      \|*/
			/* output sawtooth waveform    */

			int x;

			if ((i & (1<<(SineBits-1))) != 0)
				x = ((SineLength - 1) - i) * 16 + 1; /* negative: from 8177 to 1 */
			else
				x = i*16;   /*positive: from 0 to 8176 */

			if (x > TLTableLength)
				x = TLTableLength; /* clip to the allowed range */

			SineTable[7*SineLength+i] = x;
		}
	}

	public const int TLResolutionLength = 256; /* 8 bits addressing (real chip) */

	/*  TLTableLength is calculated as:

	*   (12+1)=13 - sinus amplitude bits     (Y axis)
	*   additional 1: to compensate for calculations of negative part of waveform
	*   (if we don't add it then the greatest possible _negative_ value would be -2
	*   and we really need -1 for waveform #7)
	*   2  - sinus sign bit           (Y axis)
	*   TLResolutionLength - sinus resolution (X axis)
	*/
	public const int TLTableLength = 13 * 2 * TLResolutionLength;

	[InlineArray(length: TLTableLength)]
	public struct TLTableArray { int _element0; }

	public static TLTableArray TLTable;

	static void InitializeTotalLevelTable()
	{
		for (int x=0; x<TLResolutionLength; x++)
		{
			double m = (1<<16) / Math.Pow(2.0, (x+1) * (YMF262Chip.EnvelopeStep/4.0) / 8.0);
			m = Math.Floor(m);

			/* we never reach (1<<16) here due to the (x+1) */
			/* result fits within 16 bits at maximum */

			int n = (int)m;     /* 16 bits here */
			n >>= 4;        /* 12 bits here */
			if (((n & 1) != 0))        /* round to nearest */
				n = (n>>1)+1;
			else
				n = n>>1;
							/* 11 bits here (rounded) */
			n <<= 1;        /* 12 bits here (as in real chip) */
			TLTable[ x*2 + 0 ] = (short)n;
			TLTable[ x*2 + 1 ] = (short)~TLTable[ x*2 + 0 ]; /* this *is* different from OPL2 (verified on real YMF262) */

			for (int i=1; i<13; i++)
			{
				TLTable[ x*2+0 + i*2*TLResolutionLength ] = (short)( TLTable[ x*2+0 ]>>i);
				TLTable[ x*2+1 + i*2*TLResolutionLength ] = (short)(~TLTable[ x*2+0 + i*2*TLResolutionLength ]);  /* this *is* different from OPL2 (verified on real YMF262) */
			}
		}
	}

	/* mapping of register number (offset) to slot number used by the emulator */
	[InlineArray(length: 32)]
	public struct SlotByRegisterArray { int _element0; }

	public static SlotByRegisterArray SlotByRegister = new SlotByRegisterArray();

	static void InitializeSlotByRegister()
	{
		int[] data =
			{
				0, 2, 4, 1, 3, 5,-1,-1,
				6, 8,10, 7, 9,11,-1,-1,
				12,14,16,13,15,17,-1,-1,
				-1,-1,-1,-1,-1,-1,-1,-1
			};

		data.AsSpan().CopyTo(SlotByRegister);
	}

	/* key scale level */
	[InlineArray(length: 128)]
	public struct KeyScaleLevelTableArray { double _element0; }

	public static KeyScaleLevelTableArray KeyScaleLevelTable;

	static void InitializeKeyScaleLevelTable()
	{
		/* table is 3dB/octave , DV converts this into 6dB/octave */
		/* 0.1875 is bit 0 weight of the envelope counter (volume) expressed in the 'decibel' scale */
		const double DV = 0.1875 / 2.0;

		double[] data =
			{
				/* OCT 0 */
				0.000/DV, 0.000/DV, 0.000/DV, 0.000/DV,
				0.000/DV, 0.000/DV, 0.000/DV, 0.000/DV,
				0.000/DV, 0.000/DV, 0.000/DV, 0.000/DV,
				0.000/DV, 0.000/DV, 0.000/DV, 0.000/DV,
				/* OCT 1 */
				0.000/DV, 0.000/DV, 0.000/DV, 0.000/DV,
				0.000/DV, 0.000/DV, 0.000/DV, 0.000/DV,
				0.000/DV, 0.750/DV, 1.125/DV, 1.500/DV,
				1.875/DV, 2.250/DV, 2.625/DV, 3.000/DV,
				/* OCT 2 */
				0.000/DV, 0.000/DV, 0.000/DV, 0.000/DV,
				0.000/DV, 1.125/DV, 1.875/DV, 2.625/DV,
				3.000/DV, 3.750/DV, 4.125/DV, 4.500/DV,
				4.875/DV, 5.250/DV, 5.625/DV, 6.000/DV,
				/* OCT 3 */
				0.000/DV, 0.000/DV, 0.000/DV, 1.875/DV,
				3.000/DV, 4.125/DV, 4.875/DV, 5.625/DV,
				6.000/DV, 6.750/DV, 7.125/DV, 7.500/DV,
				7.875/DV, 8.250/DV, 8.625/DV, 9.000/DV,
				/* OCT 4 */
				0.000/DV, 0.000/DV, 3.000/DV, 4.875/DV,
				6.000/DV, 7.125/DV, 7.875/DV, 8.625/DV,
				9.000/DV, 9.750/DV,10.125/DV,10.500/DV,
				10.875/DV,11.250/DV,11.625/DV,12.000/DV,
				/* OCT 5 */
				0.000/DV, 3.000/DV, 6.000/DV, 7.875/DV,
				9.000/DV,10.125/DV,10.875/DV,11.625/DV,
				12.000/DV,12.750/DV,13.125/DV,13.500/DV,
				13.875/DV,14.250/DV,14.625/DV,15.000/DV,
				/* OCT 6 */
				0.000/DV, 6.000/DV, 9.000/DV,10.875/DV,
				12.000/DV,13.125/DV,13.875/DV,14.625/DV,
				15.000/DV,15.750/DV,16.125/DV,16.500/DV,
				16.875/DV,17.250/DV,17.625/DV,18.000/DV,
				/* OCT 7 */
				0.000/DV, 9.000/DV,12.000/DV,13.875/DV,
				15.000/DV,16.125/DV,16.875/DV,17.625/DV,
				18.000/DV,18.750/DV,19.125/DV,19.500/DV,
				19.875/DV,20.250/DV,20.625/DV,21.000/DV
			};

		data.AsSpan().CopyTo(KeyScaleLevelTable);
	}

	/* 0 / 3.0 / 1.5 / 6.0 dB/OCT */
	[InlineArray(length: 4)]
	public struct KeyScaleLevelShiftArray { byte _element0; }

	public static KeyScaleLevelShiftArray KeyScaleLevelShift;

	static void InitializeKeyScaleLevelShift()
	{
		KeyScaleLevelShift[0] = 31;
		KeyScaleLevelShift[1] = 2;
		KeyScaleLevelShift[2] = 1;
		KeyScaleLevelShift[3] = 0;
	}


	/* sustain level table (3dB per step) */
	[InlineArray(length: 16)]
	public struct SustainLevelsArray { int _element0; }

	public static SustainLevelsArray SustainLevels;

	static void InitializeSustainLevels()
	{
		/* 0 - 15: 0, 3, 6, 9,12,15,18,21,24,27,30,33,36,39,42,93 (dB)*/
		static int SC(int db) => (int)(db * (2.0 / YMF262Chip.EnvelopeStep));

		int[] data =
			{
				SC( 0),SC( 1),SC( 2),SC(3 ),SC(4 ),SC(5 ),SC(6 ),SC( 7),
				SC( 8),SC( 9),SC(10),SC(11),SC(12),SC(13),SC(14),SC(31)
			};

		data.AsSpan().CopyTo(SustainLevels);
	}

	[InlineArray(length: 120)]
	public struct EnvelopeGeneratorIncrementsArray { byte _element0; }

	public static EnvelopeGeneratorIncrementsArray EnvelopeGeneratorIncrements;

	static void InitializeEnvelopeGeneratorIncrements()
	{
		byte[] data =
			{
				/*cycle:0 1  2 3  4 5  6 7*/

				/* 0 */ 0,1, 0,1, 0,1, 0,1, /* rates 00..12 0 (increment by 0 or 1) */
				/* 1 */ 0,1, 0,1, 1,1, 0,1, /* rates 00..12 1 */
				/* 2 */ 0,1, 1,1, 0,1, 1,1, /* rates 00..12 2 */
				/* 3 */ 0,1, 1,1, 1,1, 1,1, /* rates 00..12 3 */

				/* 4 */ 1,1, 1,1, 1,1, 1,1, /* rate 13 0 (increment by 1) */
				/* 5 */ 1,1, 1,2, 1,1, 1,2, /* rate 13 1 */
				/* 6 */ 1,2, 1,2, 1,2, 1,2, /* rate 13 2 */
				/* 7 */ 1,2, 2,2, 1,2, 2,2, /* rate 13 3 */

				/* 8 */ 2,2, 2,2, 2,2, 2,2, /* rate 14 0 (increment by 2) */
				/* 9 */ 2,2, 2,4, 2,2, 2,4, /* rate 14 1 */
				/*10 */ 2,4, 2,4, 2,4, 2,4, /* rate 14 2 */
				/*11 */ 2,4, 4,4, 2,4, 4,4, /* rate 14 3 */

				/*12 */ 4,4, 4,4, 4,4, 4,4, /* rates 15 0, 15 1, 15 2, 15 3 for decay */
				/*13 */ 8,8, 8,8, 8,8, 8,8, /* rates 15 0, 15 1, 15 2, 15 3 for attack (zero time) */
				/*14 */ 0,0, 0,0, 0,0, 0,0, /* infinity rates for attack and decay(s) */
			};

		data.AsSpan().CopyTo(EnvelopeGeneratorIncrements);
	}

	[InlineArray(length: 96)]
	public struct EnvelopeGeneratorRateSelectArray { byte _element0; }

	/* Envelope Generator rates (16 + 64 rates + 16 RKS) */
	public static EnvelopeGeneratorRateSelectArray EnvelopeGeneratorRateSelect;

	static void InitializeEnvelopeGeneratorRateSelect()
	{
		static byte O(int a) => unchecked((byte)(a * YMF262Chip.RateSteps));

		/* note that there is no O(13) in this table - it's directly in the code */
		byte[] data =
			{
				/* 16 infinite time rates */
				O(14),O(14),O(14),O(14),O(14),O(14),O(14),O(14),
				O(14),O(14),O(14),O(14),O(14),O(14),O(14),O(14),

				/* rates 00-12 */
				O( 0),O( 1),O( 2),O( 3),
				O( 0),O( 1),O( 2),O( 3),
				O( 0),O( 1),O( 2),O( 3),
				O( 0),O( 1),O( 2),O( 3),
				O( 0),O( 1),O( 2),O( 3),
				O( 0),O( 1),O( 2),O( 3),
				O( 0),O( 1),O( 2),O( 3),
				O( 0),O( 1),O( 2),O( 3),
				O( 0),O( 1),O( 2),O( 3),
				O( 0),O( 1),O( 2),O( 3),
				O( 0),O( 1),O( 2),O( 3),
				O( 0),O( 1),O( 2),O( 3),
				O( 0),O( 1),O( 2),O( 3),

				/* rate 13 */
				O( 4),O( 5),O( 6),O( 7),

				/* rate 14 */
				O( 8),O( 9),O(10),O(11),

				/* rate 15 */
				O(12),O(12),O(12),O(12),

				/* 16 dummy rates (same as 15 3) */
				O(12),O(12),O(12),O(12),O(12),O(12),O(12),O(12),
				O(12),O(12),O(12),O(12),O(12),O(12),O(12),O(12),
			};

		data.AsSpan().CopyTo(EnvelopeGeneratorRateSelect);
	}

	/*rate  0,    1,    2,    3,   4,   5,   6,  7,  8,  9,  10, 11, 12, 13, 14, 15 */
	/*shift 12,   11,   10,   9,   8,   7,   6,  5,  4,  3,  2,  1,  0,  0,  0,  0  */
	/*mask  4095, 2047, 1023, 511, 255, 127, 63, 31, 15, 7,  3,  1,  0,  0,  0,  0  */

	/* Envelope Generator counter shifts (16 + 64 rates + 16 RKS) */
	[InlineArray(length: 96)]
	public struct EnvelopeGeneratorRateShiftArray { byte _element0; }

	public static EnvelopeGeneratorRateShiftArray EnvelopeGeneratorRateShift;

	static void InitializeEnvelopeGeneratorRateShift()
	{
		static byte P(int a) => unchecked((byte)(a * 1));

		byte[] data =
			{
				/* 16 infinite time rates */
				P(0),P(0),P(0),P(0),P(0),P(0),P(0),P(0),
				P(0),P(0),P(0),P(0),P(0),P(0),P(0),P(0),

				/* rates 00-12 */
				P(12),P(12),P(12),P(12),
				P(11),P(11),P(11),P(11),
				P(10),P(10),P(10),P(10),
				P( 9),P( 9),P( 9),P( 9),
				P( 8),P( 8),P( 8),P( 8),
				P( 7),P( 7),P( 7),P( 7),
				P( 6),P( 6),P( 6),P( 6),
				P( 5),P( 5),P( 5),P( 5),
				P( 4),P( 4),P( 4),P( 4),
				P( 3),P( 3),P( 3),P( 3),
				P( 2),P( 2),P( 2),P( 2),
				P( 1),P( 1),P( 1),P( 1),
				P( 0),P( 0),P( 0),P( 0),

				/* rate 13 */
				P( 0),P( 0),P( 0),P( 0),

				/* rate 14 */
				P( 0),P( 0),P( 0),P( 0),

				/* rate 15 */
				P( 0),P( 0),P( 0),P( 0),

				/* 16 dummy rates (same as 15 3) */
				P( 0),P( 0),P( 0),P( 0),P( 0),P( 0),P( 0),P( 0),
				P( 0),P( 0),P( 0),P( 0),P( 0),P( 0),P( 0),P( 0),
			};

		data.AsSpan().CopyTo(EnvelopeGeneratorRateShift);
	}

	/* multiple table */
	[InlineArray(length: 16)]
	public struct MultipleArray { byte _element0; }

	public static MultipleArray Multiple;

	static void InitializeMultiple()
	{
		const int ML = 2;

		byte[] data =
			{
			/* 1/2, 1, 2, 3, 4, 5, 6, 7, 8, 9,10,10,12,12,15,15 */
				ML/2, 1*ML, 2*ML, 3*ML, 4*ML, 5*ML, 6*ML, 7*ML,
				8*ML, 9*ML,10*ML,10*ML,12*ML,12*ML,15*ML,15*ML
			};

		data.AsSpan().CopyTo(Multiple);
	}

	/* LFO Amplitude Modulation table (verified on real YM3812)
		27 output levels (triangle waveform); 1 level takes one of: 192, 256 or 448 samples

		Length: 210 elements.

			Each of the elements has to be repeated
			exactly 64 times (on 64 consecutive samples).
			The whole table takes: 64 * 210 = 13440 samples.

			When AM = 1 data is used directly
			When AM = 0 data is divided by 4 before being used (losing precision is important)
	*/
	[InlineArray(length: Length)]
	public struct LFOAmplitudeModulatorTableArray
	{
		public const int Length = 210;

		byte _element0;
	}

	public static LFOAmplitudeModulatorTableArray LFOAmplitudeModulatorTable;

	static void InitializeLFOAmplitudeModulatorTable()
	{
		byte[] data =
			{
				0,0,0,0,0,0,0,
				1,1,1,1,
				2,2,2,2,
				3,3,3,3,
				4,4,4,4,
				5,5,5,5,
				6,6,6,6,
				7,7,7,7,
				8,8,8,8,
				9,9,9,9,
				10,10,10,10,
				11,11,11,11,
				12,12,12,12,
				13,13,13,13,
				14,14,14,14,
				15,15,15,15,
				16,16,16,16,
				17,17,17,17,
				18,18,18,18,
				19,19,19,19,
				20,20,20,20,
				21,21,21,21,
				22,22,22,22,
				23,23,23,23,
				24,24,24,24,
				25,25,25,25,
				26,26,26,
				25,25,25,25,
				24,24,24,24,
				23,23,23,23,
				22,22,22,22,
				21,21,21,21,
				20,20,20,20,
				19,19,19,19,
				18,18,18,18,
				17,17,17,17,
				16,16,16,16,
				15,15,15,15,
				14,14,14,14,
				13,13,13,13,
				12,12,12,12,
				11,11,11,11,
				10,10,10,10,
				9,9,9,9,
				8,8,8,8,
				7,7,7,7,
				6,6,6,6,
				5,5,5,5,
				4,4,4,4,
				3,3,3,3,
				2,2,2,2,
				1,1,1,1
			};

		data.AsSpan().CopyTo(LFOAmplitudeModulatorTable);
	}

	/* LFO Phase Modulation table (verified on real YM3812) */
	[InlineArray(length: 128)]
	public struct LFOPhaseModulatorTableArray { sbyte _element0; }

	public static LFOPhaseModulatorTableArray LFOPhaseModulatorTable;

	static void InitializeLFOPhaseModulatorTable()
	{
		sbyte[] data =
			{
				/* FNUM2/FNUM = 00 0xxxxxxx (0x0000) */
				0, 0, 0, 0, 0, 0, 0, 0, /*LFO PM depth = 0*/
				0, 0, 0, 0, 0, 0, 0, 0, /*LFO PM depth = 1*/

				/* FNUM2/FNUM = 00 1xxxxxxx (0x0080) */
				0, 0, 0, 0, 0, 0, 0, 0, /*LFO PM depth = 0*/
				1, 0, 0, 0,-1, 0, 0, 0, /*LFO PM depth = 1*/

				/* FNUM2/FNUM = 01 0xxxxxxx (0x0100) */
				1, 0, 0, 0,-1, 0, 0, 0, /*LFO PM depth = 0*/
				2, 1, 0,-1,-2,-1, 0, 1, /*LFO PM depth = 1*/

				/* FNUM2/FNUM = 01 1xxxxxxx (0x0180) */
				1, 0, 0, 0,-1, 0, 0, 0, /*LFO PM depth = 0*/
				3, 1, 0,-1,-3,-1, 0, 1, /*LFO PM depth = 1*/

				/* FNUM2/FNUM = 10 0xxxxxxx (0x0200) */
				2, 1, 0,-1,-2,-1, 0, 1, /*LFO PM depth = 0*/
				4, 2, 0,-2,-4,-2, 0, 2, /*LFO PM depth = 1*/

				/* FNUM2/FNUM = 10 1xxxxxxx (0x0280) */
				2, 1, 0,-1,-2,-1, 0, 1, /*LFO PM depth = 0*/
				5, 2, 0,-2,-5,-2, 0, 2, /*LFO PM depth = 1*/

				/* FNUM2/FNUM = 11 0xxxxxxx (0x0300) */
				3, 1, 0,-1,-3,-1, 0, 1, /*LFO PM depth = 0*/
				6, 3, 0,-3,-6,-3, 0, 3, /*LFO PM depth = 1*/

				/* FNUM2/FNUM = 11 1xxxxxxx (0x0380) */
				3, 1, 0,-1,-3,-1, 0, 1, /*LFO PM depth = 0*/
				7, 3, 0,-3,-7,-3, 0, 3  /*LFO PM depth = 1*/
			};

		data.AsSpan().CopyTo(LFOPhaseModulatorTable);
	}

	/* lock level of common table */
	public static int LockCount = 0;

	/* generic table initialize */
	public static bool InitializeTables()
	{
		InitializeSineTable();
		InitializeTotalLevelTable();
		InitializeSlotByRegister();
		InitializeKeyScaleLevelTable();
		InitializeKeyScaleLevelShift();
		InitializeSustainLevels();
		InitializeEnvelopeGeneratorIncrements();
		InitializeEnvelopeGeneratorRateSelect();
		InitializeEnvelopeGeneratorRateShift();
		InitializeMultiple();
		InitializeLFOAmplitudeModulatorTable();
		InitializeLFOPhaseModulatorTable();

		return true;
	}
}
