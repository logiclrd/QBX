using System;
using System.Collections.Generic;
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
	PriorityQueue<SoundParameters, DateTime> _soundChanges = new();
	DateTime _queueMaxChangeAtTime;

	volatile SoundParameters? _nextSoundChange = null;
	volatile int _tickValue;
	volatile int _tickLength;
	volatile bool _isEnabled;
	volatile byte _value;
	volatile byte _latchedValue;

	DateTime _firstSampleEmittedTime;
	long _lastSampleEmitted;
	DateTime _scheduledHoldUntilTime;

	bool? _eventualEnabled;

	public bool IsEnabled => _eventualEnabled ?? _isEnabled;

	class SoundParameters
	{
		public DateTime ChangeAtTime;
		public bool? Enabled;
		public bool InvertValue;
		public double Frequency;
		public int TickLength;
	}

	public void OutPort(int portNumber, byte data)
	{
		if (portNumber == 0x61)
		{
			bool newEnabled = ((data & 1) != 0);

			if (newEnabled != _isEnabled)
			{
				_isEnabled = ((data & 1) != 0);
				_value = unchecked((byte)(((data & 2) >> 1) * 0xFF));
			}

			lock (_sync)
			{
				_soundChanges.Clear();
				_nextSoundChange = null;
			}
		}
	}

	public void WaitWhileQueued(TimeSpan threshold)
	{
		lock (_sync)
		{
			if (_soundChanges.Count == 0)
				return;

			while (true)
			{
				var waitUntil = _queueMaxChangeAtTime - threshold;

				var interval = waitUntil - DateTime.UtcNow;

				if (interval < threshold)
					break;

				Monitor.Wait(_sync, interval);
			}
		}
	}

	public void ChangeSound(bool? enabled, bool invertValue, double frequency, bool immediate, TimeSpan? hold = null)
	{
		var nextChange = new SoundParameters();

		nextChange.ChangeAtTime = DateTime.UtcNow;
		nextChange.Enabled = enabled;
		nextChange.InvertValue = invertValue;

		if (!immediate && (_scheduledHoldUntilTime > nextChange.ChangeAtTime))
			nextChange.ChangeAtTime = _scheduledHoldUntilTime;

		if ((hold != null) && (hold.Value.Ticks > 0))
			_scheduledHoldUntilTime = nextChange.ChangeAtTime + hold.Value;

		if (enabled != false)
		{
			nextChange.Frequency = frequency;
			nextChange.TickLength = (int)Math.Round(HalfWavelength * frequency * 2 / SampleRate);
		}

		lock (_sync)
		{
			if (nextChange.ChangeAtTime > _queueMaxChangeAtTime)
				_queueMaxChangeAtTime = nextChange.ChangeAtTime;

			if (_nextSoundChange == null)
				_nextSoundChange = nextChange;
			else if (nextChange.ChangeAtTime < _nextSoundChange.ChangeAtTime)
			{
				_soundChanges.Enqueue(_nextSoundChange, _nextSoundChange.ChangeAtTime);
				_nextSoundChange = nextChange;
			}
			else
				_soundChanges.Enqueue(nextChange, nextChange.ChangeAtTime);

			Monitor.PulseAll(_sync);
		}

		_eventualEnabled = enabled;
	}

	public void GetMoreSound(Span<byte> samples)
	{
		bool isEnabled = _isEnabled;
		byte value = _value;
		byte latchedValue = _latchedValue;
		long lastSampleEmitted = _lastSampleEmitted;
		int tickValue = _tickValue;
		int tickLength = _tickLength;

		if (_firstSampleEmittedTime == default)
			_firstSampleEmittedTime = DateTime.UtcNow - TimeSpan.FromSeconds(samples.Length / SampleRate);

		var thisBufferStartTime = _firstSampleEmittedTime
			.AddTicks(lastSampleEmitted * 10_000_000L / SampleRate);

		var slippage = DateTime.UtcNow - thisBufferStartTime;

		if (slippage.TotalSeconds > 0.5)
			_firstSampleEmittedTime += slippage;

		for (int i = 0; i < samples.Length; i++)
		{
			samples[i] = latchedValue;
			lastSampleEmitted++;

			tickValue += tickLength;

			while (tickValue >= HalfWavelength)
			{
				tickValue -= HalfWavelength;
				value = unchecked((byte)~value);
				if (isEnabled)
					latchedValue = value;
			}

			var nextSoundChange = _nextSoundChange;

			if (nextSoundChange != null)
			{
				var sampleTime = _firstSampleEmittedTime
					.AddTicks(lastSampleEmitted * 10_000_000L / SampleRate);

				if (sampleTime >= nextSoundChange.ChangeAtTime)
				{
					if (nextSoundChange.Enabled.HasValue)
						isEnabled = nextSoundChange.Enabled.Value;
					tickLength = nextSoundChange.TickLength;

					if (!isEnabled)
						latchedValue = 0;
					else
					{
						if (nextSoundChange.InvertValue)
						{
							value = unchecked((byte)~value);
							latchedValue = value;
						}

						if (nextSoundChange.Frequency != 0)
							machine.Timer.Timer2.ConfigureToMatchSound(nextSoundChange.Frequency);
					}

					lock (_sync)
					{
						if (!_soundChanges.TryDequeue(out nextSoundChange, out _))
							_eventualEnabled = null;

						_nextSoundChange = nextSoundChange;

						Monitor.PulseAll(_sync);
					}
				}
			}
		}

		_isEnabled = isEnabled;
		_value = value;
		_latchedValue = latchedValue;
		_lastSampleEmitted = lastSampleEmitted;
		_tickValue = tickValue;
		_tickLength = tickLength;
	}
}
