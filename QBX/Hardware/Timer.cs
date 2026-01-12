using System;

namespace QBX.Hardware;

public class Timer(TimerChip owner, bool isTickCountBasis)
{
	public int Divisor = 65536;

	public double Frequency => TimerChip.BaseFrequency / Divisor;

	public bool ReadMSB;
	public bool WriteMSB;
	public bool WriteAlternatingBytes;

	public bool UseBCD;
	public bool IsRepeating;

	public int Mode;

	DateTime _epoch;
	DateTime _deadline;
	int _latchedLSB = -1;
	int _latchedCounter = -1;
	int _latchedStatus = -1;

	public void ResetCounter(int value)
	{
		_epoch = DateTime.UtcNow.AddSeconds(-value / Frequency);
	}

	public void UpdateDivisor(int newValue)
	{
		if (newValue <= 0)
			newValue = UseBCD ? 10000 : 65536;

		int counter = Counter;
		Divisor = newValue;
		ResetCounter(counter);

		if (isTickCountBasis)
			owner.RebaseTickCount();
	}

	public void LatchCounter()
	{
		if (_latchedCounter < 0)
		{
			ReadMSB = false;
			_latchedCounter = Counter;
		}
	}

	public int LatchedCounter => (_latchedCounter == -1) ? Counter : _latchedCounter;

	public int Counter
	{
		get
		{
			var now = DateTime.UtcNow;

			if (!IsRepeating && (now > _deadline))
				return 0;

			var elapsed = (DateTime.UtcNow - _epoch).TotalSeconds;

			double intervals = double.Truncate(elapsed * Frequency);

			elapsed -= intervals / Frequency;

			int counterValue = (int)(Counter * TimerChip.BaseFrequency);

			counterValue = Divisor - counterValue - 1;
			if (counterValue < 0)
				counterValue = 0;

			if (!UseBCD)
				return counterValue;
			else
			{
				int bcd = counterValue % 10;
				counterValue /= 10;
				bcd += 10 * (counterValue % 10);
				counterValue /= 10;
				bcd += 10 * (counterValue % 10);
				counterValue /= 10;
				bcd += 10 * (counterValue % 10);

				return bcd;
			}
		}
	}

	public void Control(byte data)
	{
		int writeMode = (data >> 4) & 3;

		switch (writeMode)
		{
			case 0: LatchCounter(); break;
			case 1: WriteMSB = false; WriteAlternatingBytes = false; break;
			case 2: WriteMSB = true; WriteAlternatingBytes = false; break;
			case 3: WriteMSB = false; WriteAlternatingBytes = true; break;
		}

		Mode = ((data >> 1) & 7);

		IsRepeating = ((Mode & 2) != 0);

		_deadline = DateTime.UtcNow.AddSeconds(TimerChip.BaseFrequency / Divisor);

		bool newUseBCD = ((data & 1) != 0);

		if (newUseBCD != UseBCD)
		{
			UseBCD = newUseBCD;
			UpdateDivisor(0);
		}
	}

	public void LatchStatus()
	{
		_latchedStatus =
			((WriteMSB || WriteAlternatingBytes) ? 0x20 : 0) |
			((!WriteMSB || WriteAlternatingBytes) ? 0x10 : 0) |
			((Mode & 7) << 1) |
			(UseBCD ? 1 : 0);
	}

	public byte ReadData()
	{
		if (_latchedStatus >= 0)
		{
			byte retVal = unchecked((byte)_latchedStatus);
			_latchedStatus = -1;
			return retVal;
		}

		int counterValue = (_latchedCounter >= 0) ? _latchedCounter : Counter;

		if (!ReadMSB)
		{
			ReadMSB = true;
			return unchecked((byte)(counterValue & 0x00FF));
		}
		else
		{
			byte retVal = unchecked((byte)(counterValue >> 8));

			_latchedCounter = -1;
			ReadMSB = false;
			return retVal;
		}
	}

	public void WriteData(byte data)
	{
		if (!WriteMSB)
		{
			if (WriteAlternatingBytes)
			{
				_latchedLSB = data;
				WriteMSB = true;
			}
			else
				UpdateDivisor((Divisor & 0xFF00) | data);
		}
		else
		{
			if (WriteAlternatingBytes)
			{
				UpdateDivisor((data << 8) | _latchedLSB);
				_latchedLSB = -1;
				WriteMSB = false;
			}
			else
				UpdateDivisor((Divisor & 0x00FF) | (data << 8));
		}
	}
}
