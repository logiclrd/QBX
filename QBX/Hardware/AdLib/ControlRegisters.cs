using System;

namespace QBX.Hardware.AdLib;

struct ControlRegisters
{
	public ControlRegisters()
	{
	}

	ControlRegisterData _data;

	double _finalVolumeLeft;
	double _finalVolumeRight;

	double _fmVolumeLeft;
	double _fmVolumeRight;

	double _masterVolumeLeft;
	double _masterVolumeRight;

	// FM Volume and Master Volume can be combined presently because we're
	// only processing FM-synthesized source data. Thus, the "mix" phase is
	// just a pass-through. If we add PCM output in the future, then this
	// will need to be uncombined, since FM volume only applies to the FM
	// source, PCM volume only applies to the PCM source, and master volume
	// then applies to the result of mixing them.

	double _combinedVolumeLeft;
	double _combinedVolumeRight;

	IIRFilter _bassFilter;
	IIRFilter _trebleFilter;

	enum ChannelSourceMode
	{
		None = 0b000,
		LeftMono = 0b010,
		RightMono = 0b100,
		Stereo = 0b110,

		Mask = 0b110,
	}

	enum StereoMixMode
	{
		ForcedMono = 0b00_000,
		LinearStereo = 0b01_000,
		PseudoStereo = 0b10_000,
		SpatialStereo = 0b11_000,

		Mask = 0b11_000,
	}

	ChannelSourceMode _channelSourceMode;
	StereoMixMode _stereoMixMode;
	PhaseShiftFilterSet _phaseShiftFilterSet = new PhaseShiftFilterSet();

	public void SetRegister(ControlRegister register, byte value)
	{
		if ((register < ControlRegister.First)
		 || (register > ControlRegister.Last))
			return;

		_data[(int)register] = value;

		switch (register)
		{
			case ControlRegister.FinalOutputVolumeLeft:
			case ControlRegister.FinalOutputVolumeRight:
			{
				value &= 0x3F;

				double multiplier;

				if (value < 0x1C)
					multiplier = 0;
				else
				{
					double dbGain = (value - 0x3C) * 2;

					multiplier = Math.Pow(10.0, dbGain * 0.05);
				}

				switch (register)
				{
					case ControlRegister.FinalOutputVolumeLeft: _finalVolumeLeft = multiplier; break;
					case ControlRegister.FinalOutputVolumeRight: _finalVolumeRight = multiplier; break;
				}

				break;
			}

			case ControlRegister.Bass:
			case ControlRegister.Treble:
			{
				value &= 0x0F;

				double dbGain = (value - 6) * 3;

				switch (register)
				{
					case ControlRegister.Bass:
						_bassFilter.Params.CalculateLowShelf(double.ClampNative(dbGain, -12, 15)); // NB: different upper bound
						break;
					case ControlRegister.Treble:
						_trebleFilter.Params.CalculateHighShelf(double.ClampNative(dbGain, -12, 12));
						break;
				}

				break;
			}

			case ControlRegister.OutputMode:
			{
				_channelSourceMode = (ChannelSourceMode)value & ChannelSourceMode.Mask;
				_stereoMixMode = (StereoMixMode)value & StereoMixMode.Mask;
				break;
			}

			case ControlRegister.Volume_FMLeft:
			case ControlRegister.Volume_FMRight:
			case ControlRegister.Volume_MasterLeft:
			case ControlRegister.Volume_MasterRight:
			{
				bool sign = (value & 0x80) == 0;

				value &= 0x7F;

				double multiplier = value / 127.0;

				if (sign)
					multiplier = -multiplier;

				switch (register)
				{
					case ControlRegister.Volume_FMLeft:
						_fmVolumeLeft = multiplier;
						_combinedVolumeLeft = _fmVolumeLeft * _masterVolumeLeft;
						break;
					case ControlRegister.Volume_FMRight:
						_fmVolumeRight = multiplier;
						_combinedVolumeRight = _fmVolumeRight * _masterVolumeRight;
						break;
					case ControlRegister.Volume_MasterLeft:
						_masterVolumeLeft = multiplier;
						_combinedVolumeLeft = _fmVolumeLeft * _masterVolumeLeft;
						break;
					case ControlRegister.Volume_MasterRight:
						_masterVolumeRight = multiplier;
						_combinedVolumeRight = _fmVolumeRight * _masterVolumeRight;
						break;
				}

				break;
			}

			// TODO
			default: break;
		}
	}

	public byte GetRegister(ControlRegister register)
		=> _data.GetRegister(register);

	public (double Left, double Right) ProcessFrame(double left, double right)
	{
		left *= _combinedVolumeLeft;
		right *= _combinedVolumeRight;

		switch (_channelSourceMode)
		{
			case ChannelSourceMode.None:
				left = 0;
				right = 0;
				break;
			case ChannelSourceMode.LeftMono:
				right = left;
				break;
			case ChannelSourceMode.RightMono:
				left = right;
				break;
		}

		switch (_stereoMixMode)
		{
			case StereoMixMode.ForcedMono:
				left = right = (left + right) * 0.5;
				break;
			case StereoMixMode.PseudoStereo:
				left = _phaseShiftFilterSet.ProcessSample(left);
				break;
			case StereoMixMode.SpatialStereo:
				(left, right) = (
					left + 0.52 * (left - right),
					right + 0.52 * (right - left));
				break;
		}

		(left, right) = _bassFilter.ProcessSample(left, right);
		(left, right) = _trebleFilter.ProcessSample(left, right);

		left *= _finalVolumeLeft;
		right *= _finalVolumeRight;

		return (left, right);
	}
}
