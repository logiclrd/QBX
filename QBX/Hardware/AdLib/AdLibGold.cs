using System;
using System.Collections.Concurrent;

namespace QBX.Hardware.AdLib;

public class AdLibGold
{
	public const int DefaultBasePort = 0x388;

	public int BasePort = DefaultBasePort;

	public const int Bank0IndexPortOffset = 0;
	public const int Bank0DataPortOffset = 1;
	public const int Bank1IndexPortOffset = 2;
	public const int Bank1DataPortOffset = 3;

	public const int SampleRate = 44100;

	enum PortMode
	{
		FMBank,
		ControlChip,
	}

	bool _isActive;

	PortMode _secondPortMode = PortMode.FMBank;

	ControlRegisters _control = new ControlRegisters();
	ControlRegisterData _liveControlRegisters;

	ControlRegister _controlRegister;

	YMF262Chip _generator;
	YMF262Chip _liveGeneratorRegisters;

	struct PortIOOperation
	{
		public long Frame;

		public int PortNumber;
		public ControlRegister ControlRegister;
		public byte Value;
	}

	DateTime _firstSampleEmittedTime;
	long _lastFrameEmitted;

	ConcurrentQueue<PortIOOperation> _portIOOperations = new ConcurrentQueue<PortIOOperation>();

	public AdLibGold()
	{
		_generator = new YMF262Chip(SampleRate);
		_liveGeneratorRegisters = new YMF262Chip(SampleRate);
	}

	public void OutPort(int port, byte value)
	{
		switch (port - BasePort)
		{
			// Bank 1's index/data ports are overloaded for accessing the
			// control chip. Values 0xFF and 0xFE sent to the index port
			// select the mapping.
			case Bank1IndexPortOffset:
			{
				if (value == 0xFF)
					_secondPortMode = PortMode.ControlChip;
				else if (value == 0xFE)
					_secondPortMode = PortMode.FMBank;
				else if (_secondPortMode == PortMode.FMBank)
					goto case Bank0IndexPortOffset;

				_controlRegister = (ControlRegister)value;

				break;
			}

			case Bank1DataPortOffset:
			{
				if (_secondPortMode == PortMode.FMBank)
					goto case Bank0DataPortOffset;

				_liveControlRegisters.SetRegister(_controlRegister, value);

				long nowSample = (DateTime.UtcNow - _firstSampleEmittedTime).Ticks * SampleRate / TimeSpan.TicksPerSecond;

				var op = new PortIOOperation();

				op.Frame = nowSample;
				op.PortNumber = -1;
				op.ControlRegister = _controlRegister;
				op.Value = value;

				_portIOOperations.Enqueue(op);

				break;
			}
			case Bank0IndexPortOffset:
			case Bank0DataPortOffset:
			{
				if (!_isActive)
				{
					_isActive = true;
					_firstSampleEmittedTime = DateTime.UtcNow;
				}

				_liveGeneratorRegisters.OutPort(port, value);

				long nowSample = (DateTime.UtcNow - _firstSampleEmittedTime).Ticks * SampleRate / TimeSpan.TicksPerSecond;

				var op = new PortIOOperation();

				op.Frame = nowSample;
				op.PortNumber = port;
				op.Value = value;

				_portIOOperations.Enqueue(op);

				break;
			}
		}
	}

	public byte InPort(int port, out bool handled)
	{
		port -= BasePort;

		switch (port)
		{
			case Bank0IndexPortOffset:
			case Bank0DataPortOffset:
				handled = true;
				return _liveGeneratorRegisters.InPort(port);
			case Bank1IndexPortOffset:
			{
				if (_secondPortMode == PortMode.ControlChip)
				{
					handled = true;

					// The return value is a status byte:
					//
					//   7    6    5    4    3    2    1    0
					//   RB   SB   x    x    SCSI TEL  SMP  FM
					//
					// RB and SB go high while the chip is busy reading/writing
					// settings from EEPROM.
					//
					// SCSI, TEL, SMP and FM go high when the respective subsystem
					// raises an IRQ. But, we don't have SCSI, TEL or SMP subsystems,
					// and our YMF262 chip doesn't currently track timer advancement,
					// so we don't have any meaningful bits to return.
					//
					// This does mean that anybody who is polling for a timer
					// interrupt is going to be waiting a long time... We'll cross
					// that bridge when we get to it. :-)

					return 0;
				}

				goto case Bank0IndexPortOffset;
			}
			case Bank1DataPortOffset:
			{
				if (_secondPortMode == PortMode.ControlChip)
				{
					handled = true;
					return _liveControlRegisters.GetRegister(_controlRegister);
				}

				goto case Bank0DataPortOffset;
			}
		}

		handled = false;
		return 0;
	}

	double _leftOverRightSample;
	bool _haveLeftOverRightSample;

	double[]? s_buffer;

	public void GetMoreSound(Span<double> samples)
	{
		if (!_isActive)
			return;

		if ((s_buffer == null) || (s_buffer.Length < samples.Length))
			s_buffer = new double[Math.Max(256, samples.Length)];

		int frameCount = (samples.Length + 1) >> 1;

		Span<double> buffer = s_buffer.AsSpan().Slice(0, frameCount * 2);

		var thisBufferStartTime = _firstSampleEmittedTime
			.AddTicks(_lastFrameEmitted * 10_000_000L / SampleRate);

		var slippage = DateTime.UtcNow - thisBufferStartTime;

		if (slippage.TotalSeconds > 0.5)
			_firstSampleEmittedTime += slippage;

		if (_haveLeftOverRightSample)
		{
			samples[0] += _leftOverRightSample;
			samples = samples.Slice(1);

			_haveLeftOverRightSample = false;
		}

		while (samples.Length > 0)
		{
			bool haveChange = _portIOOperations.TryPeek(out var op);

			int framesBeforeNextChange;

			if (haveChange)
			{
				long nextChangeFrame = op.Frame;

				framesBeforeNextChange = (int)(nextChangeFrame - _lastFrameEmitted);
			}
			else
				framesBeforeNextChange = buffer.Length / 2;

			if (framesBeforeNextChange > 0)
			{
				int samplesBeforeNextChange = framesBeforeNextChange * 2;

				int samplesToGenerate = Math.Min(samplesBeforeNextChange, samples.Length);

				// Don't generate partial frames here
				samplesToGenerate &= ~1;

				_generator.GetMoreSound(buffer.Slice(0, samplesToGenerate));

				for (int i = 0; i < samplesToGenerate; i += 2)
				{
					var finalOutput = _control.ProcessFrame(buffer[i], buffer[i + 1]);

					samples[i + 0] += finalOutput.Left;
					samples[i + 1] += finalOutput.Right;
				}

				samples = samples.Slice(samplesToGenerate);

				_lastFrameEmitted += (samplesToGenerate >> 1);

				if (samples.Length == 1)
				{
					// How odd. We've been asked to generate a partial frame.
					_generator.GetMoreSound(buffer.Slice(0, 2));

					var finalOutput = _control.ProcessFrame(buffer[0], buffer[1]);

					buffer[0] = (short)Math.Clamp(finalOutput.Left, short.MinValue, short.MaxValue);
					buffer[1] = (short)Math.Clamp(finalOutput.Right, short.MinValue, short.MaxValue);

					_lastFrameEmitted++;

					samples[0] = buffer[0];

					_leftOverRightSample = buffer[1];
					_haveLeftOverRightSample = true;

					return;
				}
			}

			if (haveChange && (_lastFrameEmitted >= op.Frame))
			{
				_portIOOperations.TryDequeue(out _);

				if (op.PortNumber >= 0)
					_generator.OutPort(op.PortNumber, op.Value);
				else
					_control.SetRegister(op.ControlRegister, op.Value);
			}
		}
	}
}
