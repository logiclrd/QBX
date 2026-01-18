using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using QBX.ExecutionEngine.Execution;
using QBX.Firmware.Fonts;
using QBX.Hardware;

namespace QBX.ExecutionEngine;

public class PlayProcessor(Machine machine)
{
	int _octave = 4;
	int _tempo = 120;
	int _noteLengthDivisor;
	int _noteStyleFractionOutOf8;

	object _noteQueueSync = new object();
	Queue<Note> _noteQueue = new Queue<Note>();
	int _maxNoteQueue = 0;

	class Note
	{
		public double Frequency;
		public TimeSpan On;
		public TimeSpan Off;
	}

	public void StartProcessingThread()
	{
		Task.Run(ProcessNoteQueue);
	}

	TimeSpan _noteOnDuration, _noteOffDuration;

	static readonly CP437Encoding s_cp437 = new CP437Encoding();

	public double GetZNoteFrequency(int znote)
	{
		const int ZNote440 = 34;

		return 440.0 * Math.Pow(1.0 / 12.0, znote - ZNote440);
	}

	public void UpdateNoteDurations()
	{
		var wholeNoteLength = TimeSpan.FromMinutes(4) / _tempo;

		var noteLength = wholeNoteLength / _noteLengthDivisor;

		_noteOnDuration = noteLength * _noteStyleFractionOutOf8 / 8;
		_noteOffDuration = noteLength - _noteOnDuration;
	}

	public void PlayCommandString(StringValue commandString, CodeModel.Statements.Statement? source)
	{
		var input = commandString.AsSpan();

		static void Advance(ref Span<byte> i)
			=> i = i.Slice(1);

		static void SkipWhitespace(ref Span<byte> i)
		{
			// PLAY statements ignore spaces (32) and tabs (9)
			while ((i.Length > 0) && ((i[0] == 32) || (i[0] == 9)))
				i = i.Slice(1);
		}

		static void AdvanceAndSkipWhitespace(ref Span<byte> i)
		{
			Advance(ref i);
			SkipWhitespace(ref i);
		}

		Exception Fail() => throw RuntimeException.IllegalFunctionCall(source);

		int ExpectNumber(ref Span<byte> input)
		{
			SkipWhitespace(ref input);

			if (input.Length == 0)
				Fail();

			byte ch = input[0];

			if (!s_cp437.IsDigit(ch))
				Fail();

			int value = s_cp437.DigitValue(ch);
			int numDigits = 1;

			Advance(ref input);

			while (input.Length > 0)
			{
				ch = input[0];

				if (!s_cp437.IsDigit(ch))
					break;

				if (numDigits == 4)
					Fail();

				value = value * 10 + s_cp437.DigitValue(ch);
				numDigits++;
			}

			return value;
		}

		void ExpectRange(int n, int min, int max)
		{
			if ((n < min) || (n > max))
				Fail();
		}

		int ExpectNumberInRange(ref Span<byte> input, int min, int max)
		{
			int value = ExpectNumber(ref input);

			ExpectRange(value, min, max);

			return value;
		}

		while (input.Length > 0)
		{
			byte ch = s_cp437.ToUpper(input[0]);

			switch (ch)
			{
				case 32: // space
				case 9: // tab
				{
					SkipWhitespace(ref input);
					break;
				}
				case (byte)'O': // octave
				{
					AdvanceAndSkipWhitespace(ref input);

					_octave = ExpectNumberInRange(ref input, 0, 6);

					break;
				}
				case (byte)'<': // octave down
				{
					AdvanceAndSkipWhitespace(ref input);

					_octave = _octave - 1;
					if (_octave < 0)
						_octave = 0;

					break;
				}
				case (byte)'>': // octave up
				{
					AdvanceAndSkipWhitespace(ref input);

					_octave = _octave - 1;
					if (_octave > 6)
						_octave = 6;

					break;
				}

				case (byte)'N': // znote
				{
					AdvanceAndSkipWhitespace(ref input);

					int znote = ExpectNumberInRange(ref input, 0, 84);

					if (znote == 0)
						PlayRest(_noteOnDuration + _noteOffDuration);
					else
					{
						PlaySound(GetZNoteFrequency(znote), _noteOnDuration);
						PlayRest(_noteOffDuration);
					}

					break;
				}

				case (byte)'C': // note in current octave
				case (byte)'D':
				case (byte)'E':
				case (byte)'F':
				case (byte)'G':
				case (byte)'A':
				case (byte)'B':
				{
					int znote = _octave * 12;

					switch (ch)
					{
						case (byte)'C': znote += 1; break;
						case (byte)'D': znote += 3; break;
						case (byte)'E': znote += 5; break;
						case (byte)'F': znote += 6; break;
						case (byte)'G': znote += 8; break;
						case (byte)'A': znote += 10; break;
						case (byte)'B': znote += 12; break;
					}

					ExpectRange(znote, 1, 84);

					PlayNote(znote, _noteOnDuration, _noteOffDuration);

					break;
				}

				case (byte)'L': // note length, as divisor -- 64 == 1/64 note
				{
					AdvanceAndSkipWhitespace(ref input);

					_noteLengthDivisor = ExpectNumberInRange(ref input, 1, 64);

					UpdateNoteDurations();

					break;
				}

				case (byte)'M': // music style -- staccato, normal, legato and foreground/background
				{
					AdvanceAndSkipWhitespace(ref input);

					if (input.Length == 0)
						Fail();

					ch = s_cp437.ToUpper(input[0]);

					switch (ch)
					{
						case (byte)'S': _noteStyleFractionOutOf8 = 6; break;
						case (byte)'N': _noteStyleFractionOutOf8 = 7; break;
						case (byte)'L': _noteStyleFractionOutOf8 = 8; break;

						case (byte)'F':
							_maxNoteQueue = 0;
							DrainNoteQueue();
							break;
						case (byte)'B':
							_maxNoteQueue = 32;
							break;

						default: throw Fail();
					}

					UpdateNoteDurations();

					break;
				}

				case (byte)'P': // pause for n quarter notes
				{
					AdvanceAndSkipWhitespace(ref input);

					int numQuarterNotes = ExpectNumberInRange(ref input, 1, 64);

					var quarterNoteDuration = TimeSpan.FromMinutes(60) / _tempo;

					PlayRest(quarterNoteDuration * numQuarterNotes);

					break;
				}

				case (byte)'T': // set tempo
				{
					AdvanceAndSkipWhitespace(ref input);

					_tempo = ExpectNumberInRange(ref input, 32, 255);

					UpdateNoteDurations();

					break;
				}



				default:
					throw Fail();
			}
		}
	}

	public void PlaySound(double frequency, int tickCount)
	{
		var duration = TimeSpan.FromSeconds(tickCount / machine.Timer.Timer0.Frequency);

		PlaySound(frequency, duration);
	}

	public void PlaySound(double frequency, TimeSpan duration)
	{
		QueueNote(
			new Note()
			{
				Frequency = frequency,
				On = duration,
				Off = TimeSpan.Zero,
			});
	}

	public void PlayRest(TimeSpan duration)
	{
		machine.Speaker.ChangeSound(
			enabled: false,
			invertValue: false,
			machine.Timer.Timer2.Frequency,
			immediate: false,
			hold: duration);
	}

	void PlayNote(int znote, TimeSpan on, TimeSpan off)
	{
		QueueNote(
			new Note()
			{
				Frequency = GetZNoteFrequency(znote),
				On = on,
				Off = off,
			});
	}

	void QueueNote(Note note)
	{
		lock (_noteQueueSync)
		{
			while (_noteQueue.Count >= _maxNoteQueue)
				Monitor.Wait(_noteQueueSync);

			_noteQueue.Enqueue(note);

			Monitor.PulseAll(_noteQueueSync);
		}
	}

	void DrainNoteQueue()
	{
		lock (_noteQueueSync)
		{
			while (_noteQueue.Count > 0)
				Monitor.Wait(_noteQueueSync);
		}
	}

	void ProcessNoteQueue()
	{
		try
		{
			Note? note = null;

			while (true)
			{
				lock (_noteQueueSync)
				{
					if ((_noteQueue.Count == 0)
					 && ((note == null) || (note.Off <= TimeSpan.Zero)))
					{
						machine.Speaker.ChangeSound(
							enabled: false,
							invertValue: false,
							note?.Frequency ?? 100,
							immediate: false,
							hold: TimeSpan.Zero);
					}

					while (_noteQueue.Count == 0)
						Monitor.Wait(_noteQueueSync);

					note = _noteQueue.Dequeue();
				}

				machine.Speaker.ChangeSound(
					enabled: true,
					invertValue: false,
					note.Frequency,
					immediate: false,
					hold: note.On);

				if (note.Off > TimeSpan.Zero)
				{
					machine.Speaker.ChangeSound(
						enabled: false,
						invertValue: false,
						note.Frequency,
						immediate: false,
						hold: note.Off);
				}
			}
		}
		finally
		{
			Thread.Sleep(100);
			Task.Run(ProcessNoteQueue);
		}
	}
}
