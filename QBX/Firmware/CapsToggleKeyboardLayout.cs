using QBX.Hardware;

using SDL3;

namespace QBX.Firmware;

public abstract class CapsToggleKeyboardLayout(Machine machine) : KeyboardLayout(machine)
{
	protected override void UpdateCapsLockState(RawKeyEventData data, ref int keyboardStatus)
	{
		if ((data.Modifiers & SDL.Keymod.Caps) != 0)
			keyboardStatus |= SystemMemory.KeyboardStatus_CapsLockBit;
		else
			keyboardStatus &= ~SystemMemory.KeyboardStatus_CapsLockBit;
	}
}
