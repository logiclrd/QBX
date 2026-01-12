using System;

namespace QBX.Hardware;

public class TimerChip
{
	public const double BaseFrequency = 105_000_000d / 88d;

	const int Timer0DataPort = 0x40;
	const int Timer1DataPort = 0x41;
	const int Timer2DataPort = 0x42;
	const int ControlPort = 0x43;

	public readonly Timer Timer0;
	public readonly Timer Timer1;
	public readonly Timer Timer2;

	public TimerChip()
	{
		Timer0 = new Timer(this, isTickCountBasis: true);
		Timer1 = new Timer(this, isTickCountBasis: false);
		Timer2 = new Timer(this, isTickCountBasis: false);
	}

	public const double TicksPerSecond = BaseFrequency / 65536.0;

	DateTime _startupTime = DateTime.UtcNow;
	DateTime _tickEpoch = DateTime.UtcNow;
	long _tickBase = (long)(DateTime.Now.TimeOfDay.TotalSeconds * TicksPerSecond);

	public DateTime StartupTime => _startupTime;

	public DateTime CurrentTime => _startupTime + TimeSpan.FromSeconds(TickCount / TicksPerSecond);

	public long TickCount
	{
		get
		{
			return (long)(_tickBase + (DateTime.UtcNow - _tickEpoch).TotalSeconds * Timer0.Frequency);
		}
	}

	public void RebaseTickCount()
	{
		_tickBase = TickCount;
		_tickEpoch = DateTime.UtcNow;
	}

	public void OutPort(int portNumber, byte data)
	{
		switch (portNumber)
		{
			case Timer0DataPort: Timer0.WriteData(data); break;
			case Timer1DataPort: Timer1.WriteData(data); break;
			case Timer2DataPort: Timer2.WriteData(data); break;

			case ControlPort:
			{
				switch (data >> 6)
				{
					case 0: Timer0.Control(data); break;
					case 1: Timer1.Control(data); break;
					case 2: Timer2.Control(data); break;
				}

				break;
			}
		}
	}

	public byte InPort(int portNumber, out bool handled)
	{
		switch (portNumber)
		{
			case Timer0DataPort:
				handled = true;
				return Timer0.ReadData();
			case Timer1DataPort:
				handled = true;
				return Timer0.ReadData();
			case Timer2DataPort:
				handled = true;
				return Timer0.ReadData();
		}

		handled = false;
		return default;
	}
}
