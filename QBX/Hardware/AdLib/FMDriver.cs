using System;

namespace QBX.Hardware.AdLib;

public static class FMDriver
{
	/// Convert a frequency into an OPL f-number
	/**
	* @param milliHertz
	* Input frequency.
	*
	* @param fnum
	* Output frequency number for OPL chip. This is a 10-bit number, so it will
	* always be between 0 and 1023 inclusive.
	*
	* @param block
	* Output block number for OPL chip. This is a 3-bit number, so it will
	* always be between 0 and 7 inclusive.
	*
	* @param conversionFactor
	* Conversion factor to use. Normally will be 49716 and occasionally 50000.
	*
	* @post fnum will be set to a value between 0 and 1023 inclusive. block will
	* be set to a value between 0 and 7 inclusive. assert() calls inside this
	* function ensure this will always be the case.
	*
	* @note As the block value increases, the frequency difference between two
	* adjacent fnum values increases. This means the higher the frequency,
	* the less precision is available to represent it. Therefore, converting
	* a value to fnum/block and back to milliHertz is not guaranteed to reproduce
	* the original value.
	*/
	public static void MilliHertzToFnum(int milliHertz, out int fnum, out int block, uint conversionFactor)
	{
		// Special case to avoid divide by zero
		if (milliHertz <= 0)
		{
			block = 0; // actually any block will work
			fnum = 0;
			return;
		}

		// Special case for frequencies too high to produce
		if (milliHertz > 6208431)
		{
			block = 7;
			fnum = 1023;
			return;
		}

		/// This formula will provide a pretty good estimate as to the best block to
		/// use for a given frequency.  It tries to use the lowest possible block
		/// number that is capable of representing the given frequency.  This is
		/// because as the block number increases, the precision decreases (i.e. there
		/// are larger steps between adjacent note frequencies.)  The 6M constant is
		/// the largest frequency (in milliHertz) that can be represented by the
		/// block/fnum system.
		//int invertedBlock = log2(6208431 / milliHertz);

		// Very low frequencies will produce very high inverted block numbers, but
		// as they can all be covered by inverted block 7 (block 0) we can just clip
		// the value.
		//if (invertedBlock > 7) invertedBlock = 7;
		//*block = 7 - invertedBlock;

		// This is a bit more efficient and doesn't need log2() from math.h
		if (milliHertz > 3104215) block = 7;
		else if (milliHertz > 1552107) block = 6;
		else if (milliHertz > 776053) block = 5;
		else if (milliHertz > 388026) block = 4;
		else if (milliHertz > 194013) block = 3;
		else if (milliHertz > 97006) block = 2;
		else if (milliHertz > 48503) block = 1;
		else block = 0;

		// Original formula
		//*fnum = milliHertz * pow(2, 20 - *block) / 1000 / conversionFactor + 0.5;

		// Slightly more efficient version
		fnum = (int)(((long)milliHertz << (20 - block)) / (conversionFactor * 1000.0) + 0.5);

		if (fnum > 1023)
		{
			block++;
			fnum = (int)(((long)milliHertz << (20 - block)) / (conversionFactor * 1000.0) + 0.5);
		}
	}

	/***************************************/

	public static readonly int[] PortBases = {0, 1, 2, 8, 9, 10, 16, 17, 18};

	/*
	 *      Register numbers for the global registers
	 */

	public const int TestRegister             = 0x01;
	public const int   EnableWaveSelect       = 0x20;

	public const int Timer1Register           = 0x02;
	public const int Timer2Register           = 0x03;
	public const int TimerControlRegister     = 0x04;    /* Left side */
	public const int   IRQReset               = 0x80;
	public const int   Timer1Mask             = 0x40;
	public const int   Timer2Mask             = 0x20;
	public const int   Timer1Start            = 0x01;
	public const int   Timer2Start            = 0x02;

	public const int ConnectionSelectRegister = 0x04;    /* Right side */
	public const int   Right4OP0              = 0x01;
	public const int   Right4OP1              = 0x02;
	public const int   Right4OP2              = 0x04;
	public const int   Left4OP0               = 0x08;
	public const int   Left4OP1               = 0x10;
	public const int   Left4OP2               = 0x20;

	public const int OPL3ModeRegister         = 0x05;    /* Right side */
	public const int   OPL3Enable             = 0x01;
	public const int   OPL4Enable             = 0x02;

	public const int KeyboardSplitRegister    = 0x08;    /* Left side */
	public const int   CompositeSineWaveMode  = 0x80;    /* Don't use with OPL-3? */
	public const int   KeyboardSplit          = 0x40;

	public const int PercussionRegister       = 0xbd;    /* Left side only */
	public const int   TremoloDepth           = 0x80;
	public const int   VibratoDepth           = 0x40;
	public const int   PercussionEnable       = 0x20;
	public const int   BassdrumOn             = 0x10;
	public const int   SnaredrumOn            = 0x08;
	public const int   TomtomOn               = 0x04;
	public const int   CymbalOn               = 0x02;
	public const int   HiHatOn                = 0x01;

	/*
	 *      Offsets to the register banks for operators. To get the
	 *      register number just add the operator offset to the bank offset
	 *
	 *      AM/VIB/EG/KSR/Multiple (0x20 to 0x35)
	 */
	public const int AMVib          = 0x20;
	public const int   TremoloOn    = 0x80;
	public const int   VibratoOn    = 0x40;
	public const int   SustainOn    = 0x20;
	public const int   KSR          = 0x10;    /* Key scaling rate */
	public const int   MultipleMask = 0x0f;    /* Frequency multiplier */

	/*
	 *     KSL/Total level (0x40 to 0x55)
	 */
	public const int KSLLevel = 0x40;
	public const int   KSLMask = 0xc0;           /* Envelope scaling bits */
	public const int   TotalLevelMask = 0x3f;    /* Strength (volume) of OP */

	/*
	 *      Attack / Decay rate (0x60 to 0x75)
	 */
	public const int AttackDecay = 0x60;
	public const int   AttackMask = 0xf0;
	public const int   DecayMask = 0x0f;

	/*
	 * Sustain level / Release rate (0x80 to 0x95)
	 */
	public const int SustainRelease = 0x80;
	public const int   SustainMask = 0xf0;
	public const int   ReleaseMask = 0x0f;

	/*
	 * Wave select (0xE0 to 0xF5)
	 */
	public const int WaveSelect = 0xe0;

	/*
	 *      Offsets to the register banks for voices. Just add to the
	 *      voice number to get the register number.
	 *
	 *      F-Number low bits (0xA0 to 0xA8).
	 */
	public const int FNumLow = 0xa0;

	/*
	 *      F-number high bits / Key on / Block (octave) (0xB0 to 0xB8)
	 */
	public const int KeyOnBlock   = 0xb0;
	public const byte KeyOnBit    = 0x20;
	public const int BlockNumMask = 0x1c;
	public const int FNumHighMask = 0x03;

	/*
	 *      Feedback / Connection (0xc0 to 0xc8)
	 *
	 *      These registers have two new bits when the OPL-3 mode
	 *      is selected. These bits controls connecting the voice
	 *      to the stereo channels. For 4 OP voices this bit is
	 *      defined in the second half of the voice (add 3 to the
	 *      register offset).
	 *
	 *      For 4 OP voices the connection bit is used in the
	 *      both halves (gives 4 ways to connect the operators).
	 */
	public const int FeedbackConnection = 0xc0;
	public const int   FeedbackMask     = 0x0e;    /* Valid just for 1st OP of a voice */
	public const int   ConnectionBit    = 0x01;
	/*
	 *      In the 4 OP mode there is four possible configurations how the
	 *      operators can be connected together (in 2 OP modes there is just
	 *      AM or FM). The 4 OP connection mode is defined by the rightmost
	 *      bit of the FEEDBACK_CONNECTION (0xC0-0xC8) on the both halves.
	 *
	 *      First half      Second half     Mode
	 *
	 *                                       +---+
	 *                                       v   |
	 *      0               0               >+-1-+--2--3--4-->
	 *
	 *
	 *
	 *                                       +---+
	 *                                       |   |
	 *      0               1               >+-1-+--2-+
	 *                                                |->
	 *                                      >--3----4-+
	 *
	 *                                       +---+
	 *                                       |   |
	 *      1               0               >+-1-+-----+
	 *                                                 |->
	 *                                      >--2--3--4-+
	 *
	 *                                       +---+
	 *                                       |   |
	 *      1               1               >+-1-+--+
	 *                                              |
	 *                                      >--2--3-+->
	 *                                              |
	 *                                      >--4----+
	 */
	public const int StereoBits     = 0x30;    /* OPL-3 only */
	public const int   VoiceToLeft  = 0x10;
	public const int   VoiceToRight = 0x20;
}
