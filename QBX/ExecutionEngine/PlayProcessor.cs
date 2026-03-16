using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

using QBX.Hardware;

using ExecutionContext = QBX.ExecutionEngine.Execution.ExecutionContext;

namespace QBX.ExecutionEngine;

public class PlayProcessor : ProcessorCommon
{
	Machine _machine;

	int _octave = 4;
	int _tempo = 120;
	int _noteLengthDivisor = 4;
	int _noteStyleFractionOutOf8 = 7;

	object _noteQueueSync = new object();
	Queue<Note> _noteQueue = new Queue<Note>();
	int _maxNoteQueue = 1;

	public int QueueLength => _noteQueue.Count;

	public event Action? QueueLengthChanged;

	class Note
	{
		public double Frequency;
		public bool IsRest;
		public TimeSpan On;
		public TimeSpan Off;
	}

	public PlayProcessor(Machine machine)
	{
		_machine = machine;

		UpdateNoteDurations();
	}

	public void StartProcessingThread()
	{
		var thread = new Thread(ProcessNoteQueue);

		thread.IsBackground = true;
		thread.Name = "PLAY Processor";

		thread.Start();
	}

	TimeSpan _noteOnDuration, _noteOffDuration;

	public double GetZNoteFrequency(int znote)
	{
		const int ZNote440 = 34;

		return 440.0 * Math.Pow(2.0, (znote - ZNote440) / 12.0);
	}

	public void CalculateNoteDurations(int divisor, out TimeSpan on, out TimeSpan off)
	{
		var wholeNoteLength = TimeSpan.FromMinutes(4) / _tempo;

		var noteLength = wholeNoteLength / divisor;

		on = noteLength * _noteStyleFractionOutOf8 / 8;
		off = noteLength - on;
	}

	void UpdateNoteDurations()
	{
		CalculateNoteDurations(
			_noteLengthDivisor,
			out _noteOnDuration,
			out _noteOffDuration);
	}

	public void PlayCommandString(StringValue commandString, CodeModel.Statements.Statement? source)
		=> PlayCommandString(commandString.AsSpan(), source);

	public void PlayCommandString(Span<byte> commandString, CodeModel.Statements.Statement? source)
		=> PlayCommandString(commandString, executionContext: null, source);

	public void PlayCommandString(StringValue commandString, ExecutionContext? executionContext, CodeModel.Statements.Statement? source)
		=> PlayCommandString(commandString.AsSpan(), executionContext, source);

	public void PlayCommandString(Span<byte> commandString, ExecutionContext? executionContext, CodeModel.Statements.Statement? source)
	{
		CurrentSource = source;

		var input = commandString;

		while (input.Length > 0)
		{
			byte ch = Encoding.ToUpper(input[0]);

			switch (ch)
			{
				case 32: // space
				case 9: // tab
				{
					SkipWhitespace(ref input);
					break;
				}
				case O: // octave
				{
					AdvanceAndSkipWhitespace(ref input);

					_octave = ExpectIntegerInRange(ref input, 0, 6, executionContext);

					break;
				}
				case LeftAngle: // octave down
				{
					AdvanceAndSkipWhitespace(ref input);

					_octave = _octave - 1;
					if (_octave < 0)
						_octave = 0;

					break;
				}
				case RightAngle: // octave up
				{
					AdvanceAndSkipWhitespace(ref input);

					_octave = _octave + 1;
					if (_octave > 6)
						_octave = 6;

					break;
				}

				case N: // znote
				{
					AdvanceAndSkipWhitespace(ref input);

					int znote = ExpectIntegerInRange(ref input, 0, 84, executionContext);

					if (znote == 0)
						PlayRest(_noteOnDuration + _noteOffDuration);
					else
					{
						PlaySound(GetZNoteFrequency(znote), _noteOnDuration);
						PlayRest(_noteOffDuration);
					}

					break;
				}

				case C: // note in current octave
				case D:
				case E:
				case F:
				case G:
				case A:
				case B:
				{
					int znote = _octave * 12;

					switch (ch)
					{
						case C: znote += 1; break;
						case D: znote += 3; break;
						case E: znote += 5; break;
						case F: znote += 6; break;
						case G: znote += 8; break;
						case A: znote += 10; break;
						case B: znote += 12; break;
					}

					AdvanceAndSkipWhitespace(ref input);

					if (input.Length > 0)
					{
						switch (input[0])
						{
							case Plus:
							case Sharp:
								znote++;
								AdvanceAndSkipWhitespace(ref input);
								break;
							case Minus:
								znote--;
								AdvanceAndSkipWhitespace(ref input);
								break;
						}
					}

					var on = _noteOnDuration;
					var off = _noteOffDuration;

					if (input.Length > 0)
					{
						if (Encoding.IsDigit(input[0]))
						{
							int duration = ExpectIntegerInRange(ref input, 1, 64, null);

							CalculateNoteDurations(duration, out on, out off);
						}
					}

					var onDot = on / 2;
					var offDot = off / 2;

					while ((input.Length > 0) && (input[0] == Dot))
					{
						on += onDot;
						off += offDot;

						onDot *= 0.5;
						offDot *= 0.5;

						AdvanceAndSkipWhitespace(ref input);
					}

					ExpectRange(znote, 1, 84);

					PlayNote(znote, on, off);

					break;
				}

				case L: // note length, as divisor -- 64 == 1/64 note
				{
					AdvanceAndSkipWhitespace(ref input);

					_noteLengthDivisor = ExpectIntegerInRange(ref input, 1, 64, executionContext);

					UpdateNoteDurations();

					break;
				}

				case M: // music style -- staccato, normal, legato and foreground/background
				{
					AdvanceAndSkipWhitespace(ref input);

					if (input.Length == 0)
						Fail();

					ch = Encoding.ToUpper(input[0]);

					switch (ch)
					{
						case S: _noteStyleFractionOutOf8 = 6; break;
						case N: _noteStyleFractionOutOf8 = 7; break;
						case L: _noteStyleFractionOutOf8 = 8; break;

						case F:
							_maxNoteQueue = 1;
							DrainNoteQueue();
							break;
						case B:
							_maxNoteQueue = 32;
							break;

						default: throw Fail();
					}

					AdvanceAndSkipWhitespace(ref input);

					UpdateNoteDurations();

					break;
				}

				case P: // pause for n quarter notes
				{
					AdvanceAndSkipWhitespace(ref input);

					int numQuarterNotes = ExpectIntegerInRange(ref input, 1, 64, executionContext);

					var quarterNoteDuration = TimeSpan.FromMinutes(1) / _tempo;

					PlayRest(quarterNoteDuration * numQuarterNotes);

					break;
				}

				case T: // set tempo
				{
					AdvanceAndSkipWhitespace(ref input);

					_tempo = ExpectIntegerInRange(ref input, 32, 255, executionContext);

					UpdateNoteDurations();

					break;
				}

				case X:
				{
					Advance(ref input);

					var descriptorBytes = input.Slice(0, 3);
					var descriptorSpan = MemoryMarshal.Cast<byte, SurfacedVariableDescriptor>(descriptorBytes);

					var descriptor = descriptorSpan[0];

					input = input.Slice(3);

					var surfaced = executionContext?.GetSurfacedVariable(descriptor.Key);

					if (surfaced is StringVariable surfacedString)
						PlayCommandString(surfacedString.ValueSpan, executionContext, source);

					break;
				}

				default:
					throw Fail();
			}
		}
	}

	public void PlaySound(double frequency, int tickCount)
	{
		var duration = TimeSpan.FromSeconds(tickCount / _machine.Timer.Timer0.Frequency);

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
		QueueNote(
			new Note()
			{
				IsRest = true,
				Frequency = 100,
				On = duration,
			});
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

			QueueLengthChanged?.Invoke();
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
						_machine.Speaker.ChangeSound(
							enabled: false,
							invertValue: false,
							note?.Frequency ?? 100,
							immediate: false,
							hold: TimeSpan.Zero);
					}

					while (_noteQueue.Count == 0)
						Monitor.Wait(_noteQueueSync);

					note = _noteQueue.Dequeue();

					Monitor.PulseAll(_noteQueueSync);

					QueueLengthChanged?.Invoke();
				}

				_machine.Speaker.WaitWhileQueued(threshold: TimeSpan.FromSeconds(0.05));

				_machine.Speaker.ChangeSound(
					enabled: !note.IsRest,
					invertValue: false,
					note.Frequency,
					immediate: false,
					hold: note.On);

				if (note.Off > TimeSpan.Zero)
				{
					_machine.Speaker.ChangeSound(
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
			StartProcessingThread();
		}
	}
}
