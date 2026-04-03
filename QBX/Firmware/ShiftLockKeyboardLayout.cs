using QBX.Hardware;

using SDL3;

namespace QBX.Firmware;

public abstract class ShiftLockKeyboardLayout(Machine machine) : KeyboardLayout(machine)
{
	protected override bool GetUpdatedCapsLockState(RawKeyEventData data, bool currentState)
	{
		switch (data.RawScanCode)
		{
			case SDL.Scancode.Capslock:
				return true;
			case SDL.Scancode.LShift:
			case SDL.Scancode.RShift:
				return false;
		}

		return currentState;
	}
}
