using QBX.Hardware;

using SDL3;

namespace QBX.Firmware;

public abstract class CapsToggleKeyboardLayout(Machine machine) : KeyboardLayout(machine)
{
	protected override bool GetUpdatedCapsLockState(RawKeyEventData data, bool currentState)
		=> (data.Modifiers & SDL.Keymod.Caps) != 0;
}
