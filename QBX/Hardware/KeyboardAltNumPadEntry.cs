using System;
using System.Collections.Generic;

using QBX.Firmware;

using SDL3;

namespace QBX.Hardware;

public class KeyboardAltNumPadEntry(SystemMemory systemMemory)
{
	public readonly Queue<byte> InputQueue = new Queue<byte>();

	public const int AccumulatorAddress = 1049;

	bool _altKeyDown = false;

	public bool ProcessKeyEvent(KeyEventData keyEvent)
	{
		if (keyEvent.RawKeyEventData.IsRelease)
		{
			if (keyEvent.ScanCode == ScanCode.Alt)
				NotifyAltKeyUp();
		}
		else
		{
			if (keyEvent.ScanCode == ScanCode.Alt)
				NotifyAltKeyDown();
			else if (_altKeyDown)
			{
				switch (keyEvent.RawKeyEventData.RawScanCode)
				{
					case SDL.Scancode.Kp0:
					case SDL.Scancode.Kp1:
					case SDL.Scancode.Kp2:
					case SDL.Scancode.Kp3:
					case SDL.Scancode.Kp4:
					case SDL.Scancode.Kp5:
					case SDL.Scancode.Kp6:
					case SDL.Scancode.Kp7:
					case SDL.Scancode.Kp8:
					case SDL.Scancode.Kp9:
					{
						int digitValue = -1;

						switch (keyEvent.RawKeyEventData.RawScanCode)
						{
							case SDL.Scancode.Kp0: digitValue = 0; break;
							case SDL.Scancode.Kp1: digitValue = 1; break;
							case SDL.Scancode.Kp2: digitValue = 2; break;
							case SDL.Scancode.Kp3: digitValue = 3; break;
							case SDL.Scancode.Kp4: digitValue = 4; break;
							case SDL.Scancode.Kp5: digitValue = 5; break;
							case SDL.Scancode.Kp6: digitValue = 6; break;
							case SDL.Scancode.Kp7: digitValue = 7; break;
							case SDL.Scancode.Kp8: digitValue = 8; break;
							case SDL.Scancode.Kp9: digitValue = 9; break;
						}

						if (digitValue >= 0)
						{
							NotifyNumPadEntry(digitValue);

							return true; // swallow this input
						}

						break;
					}
				}
			}
		}

		return false; // do not swallow input by default
	}

	public void NotifyAltKeyDown()
	{
		systemMemory[AccumulatorAddress] = 0;
		_altKeyDown = true;
	}

	public void NotifyNumPadEntry(int digitValue)
	{
		if (_altKeyDown)
		{
			systemMemory[AccumulatorAddress] = unchecked((byte)(
				10 * systemMemory[AccumulatorAddress] + digitValue));
		}
	}

	public void NotifyAltKeyUp()
	{
		_altKeyDown = false;

		byte inputCharacter = systemMemory[AccumulatorAddress];

		systemMemory[AccumulatorAddress] = 0;

		if (inputCharacter != 0)
			InputQueue.Enqueue(inputCharacter);
	}
}
