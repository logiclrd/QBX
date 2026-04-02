using QBX.Hardware;

using SDL3;

namespace QBX.Firmware;

public abstract class ShiftLockKeyboardLayout(Machine machine) : KeyboardLayout(machine)
{
	protected override void UpdateCapsLockState(RawKeyEventData data, ref int keyboardStatus)
	{
		switch (data.RawScanCode)
		{
			case SDL.Scancode.Capslock:
				keyboardStatus |= SystemMemory.KeyboardStatus_CapsLockBit;
				break;
			case SDL.Scancode.LShift:
			case SDL.Scancode.RShift:
				keyboardStatus &= ~SystemMemory.KeyboardStatus_CapsLockBit;
				break;
		}
	}
}
