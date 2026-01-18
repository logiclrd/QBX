using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace QBX.Hardware;

public class Speaker(Machine machine)
{
	// Model:
	// - tick value counts up to Wavelength repeatedly
	// - on each sample, tick value is increased by tick length
	// - while (tick value > half wavelength)
	//   {
	//     tick value -= half wavelength;
	//     value = ~value;
	//   }
	// - NB: value flips on each half wavelength; full wavelength is two flips
	// - smaller tick lengths are lower frequencies, higher tick lengths
	//   traverse the wavelength faster and are higher frequencies
	// - when the parameters are changed asynchronously, sound changes are queued up,
	//   allowing the span of time between the last sample generated and the
	//   parameter change to be unaffected by the parameter change

	public const int SampleRate = 44100;

	const int HalfWavelength = 100 * 1048576;

	object _sync = new object();
	Queue<SoundParameters> _soundChanges = new Queue<SoundParameters>();

	volatile SoundParameters? _nextSoundChange = null;
	volatile int _holdCurrentRemainingSamples;
	volatile int _tickValue;
	volatile int _tickLength;
	volatile bool _enabled;
	volatile byte _value;

	bool? _eventualEnabled;

	public bool IsEnabled => _eventualEnabled ?? _enabled;

	class SoundParameters
	{
		public bool Enabled;
		public bool InvertValue;
		public double Frequency;
		public int TickLength;
		public int HoldForSamples;
	}

	public void OutPort(int portNumber, byte data)
	{
		if (portNumber == 0x61)
		{
			bool newEnabled = ((data & 1) != 0);

			if (newEnabled != _enabled)
			{
				_enabled = ((data & 1) != 0);
				_value = unchecked((byte)(((data & 2) >> 1) * 0xFF));
			}

			lock (_sync)
			{
				_soundChanges.Clear();
				_nextSoundChange = null;
				_holdCurrentRemainingSamples = 0;
			}
		}
	}

	public void WaitWhileQueued(TimeSpan threshold)
	{
		int thresholdSamples = (int)Math.Round(threshold.TotalSeconds * SampleRate);

		lock (_sync)
		{
			while (true)
			{
				var queued = _soundChanges.Sum(change => (long)change.HoldForSamples);

				if (queued < thresholdSamples)
					break;

				Monitor.Wait(_sync);
			}
		}
	}

	public void ChangeSound(bool enabled, bool invertValue, double frequency, bool immediate, TimeSpan hold)
	{
		var nextChange = new SoundParameters();

		nextChange.Enabled = enabled;
		nextChange.InvertValue = invertValue;
		nextChange.HoldForSamples = (int)Math.Round(hold.TotalSeconds * SampleRate);

		if (enabled)
		{
			nextChange.Frequency = frequency;
			nextChange.TickLength = (int)Math.Round(HalfWavelength * frequency * 2 / SampleRate);
		}

		lock (_sync)
		{
			if (immediate)
			{
				_soundChanges.Clear();
				_holdCurrentRemainingSamples = 0;
				_nextSoundChange = nextChange;
			}
			else
			{
				if (_nextSoundChange != null)
					_soundChanges.Enqueue(nextChange);
				else
					_nextSoundChange = nextChange;
			}
		}

		_eventualEnabled = enabled;
	}

	public void GetMoreSound(Span<byte> samples)
	{
		bool isEnabled = _enabled;
		byte value = _value;

		for (int i = 0; i < samples.Length; i++)
		{
			samples[i] = value;

			_tickValue += _tickLength;

			while (_tickValue >= HalfWavelength)
			{
				_tickValue -= HalfWavelength;
				_value = unchecked((byte)~_value);
				// value latches _value at the bottom of the loop if isEnabled
			}

			if (_holdCurrentRemainingSamples > 0)
				_holdCurrentRemainingSamples--;
			else if (_nextSoundChange != null)
			{
				_enabled = _nextSoundChange.Enabled;
				_tickLength = _nextSoundChange.TickLength;

				if (!_enabled)
					value = 0;
				else
				{
					if (_nextSoundChange.InvertValue)
						_value = unchecked((byte)~value);

					if (_nextSoundChange.Frequency != 0)
						machine.Timer.Timer2.ConfigureToMatchSound(_nextSoundChange.Frequency);
				}

				_holdCurrentRemainingSamples = _nextSoundChange.HoldForSamples;

				lock (_sync)
				{
					if (!_soundChanges.TryDequeue(out var nextSoundChange))
						_eventualEnabled = null;

					_nextSoundChange = nextSoundChange;

					Monitor.PulseAll(_sync);
				}
			}

			if (isEnabled)
				value = _value;
		}
	}
}
