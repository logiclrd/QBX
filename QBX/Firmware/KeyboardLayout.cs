using System.Diagnostics.CodeAnalysis;

using QBX.Hardware;

using SDL3;

namespace QBX.Firmware;

public abstract class KeyboardLayout(Machine machine)
{
	public virtual bool IsMatchForCurrentSDLState() => false;

	public abstract void ProcessKeyPress(RawKeyEventData rawData);
	public abstract void Reset();
	public abstract bool TryGetNextTranslatedKeyPress([NotNullWhen(true)] out KeyEventData? data);

	protected void UpdateModifiers(RawKeyEventData data)
	{
		switch (data.RawScanCode)
		{
			case SDL.Scancode.LCtrl:
			case SDL.Scancode.RCtrl:
			case SDL.Scancode.LShift:
			case SDL.Scancode.RShift:
			case SDL.Scancode.LAlt:
			case SDL.Scancode.RAlt:
			case SDL.Scancode.Capslock:
			case SDL.Scancode.NumLockClear:
			{
				var mods = data.Modifiers;

				int keyboardStatus = machine.SystemMemory[SystemMemory.KeyboardStatusAddress];

				if ((mods & SDL.Keymod.Ctrl) != 0)
					keyboardStatus |= SystemMemory.KeyboardStatus_ControlBit;
				else
					keyboardStatus &= ~SystemMemory.KeyboardStatus_ControlBit;

				if ((mods & SDL.Keymod.Alt) != 0)
					keyboardStatus |= SystemMemory.KeyboardStatus_AltBit;
				else
					keyboardStatus &= ~SystemMemory.KeyboardStatus_AltBit;

				if ((mods & SDL.Keymod.Shift) == 0)
					keyboardStatus &= ~(SystemMemory.KeyboardStatus_LeftShiftBit | SystemMemory.KeyboardStatus_RightShiftBit);
				else
				{
					int shiftBit =
						data.RawScanCode switch
						{
							SDL.Scancode.LShift => SystemMemory.KeyboardStatus_LeftShiftBit,
							SDL.Scancode.RShift => SystemMemory.KeyboardStatus_RightShiftBit,

							_ => 0
						};

					if (!data.IsRelease)
						keyboardStatus |= shiftBit;
					else
						keyboardStatus &= ~shiftBit;
				}

				if ((mods & SDL.Keymod.Scroll) != 0)
					keyboardStatus |= SystemMemory.KeyboardStatus_ScrollLockBit;
				else
					keyboardStatus &= ~SystemMemory.KeyboardStatus_ScrollLockBit;

				if ((mods & SDL.Keymod.Num) != 0)
					keyboardStatus |= SystemMemory.KeyboardStatus_NumLockBit;
				else
					keyboardStatus &= ~SystemMemory.KeyboardStatus_NumLockBit;

				UpdateCapsLockState(data, ref keyboardStatus);

				if (!data.IsRelease && (data.RawScanCode == SDL.Scancode.Insert))
					keyboardStatus ^= SystemMemory.KeyboardStatus_InsertBit;

				machine.SystemMemory[SystemMemory.KeyboardStatusAddress] =
					unchecked((byte)keyboardStatus);

				break;
			}
		}
	}

	protected abstract void UpdateCapsLockState(RawKeyEventData data, ref int keyboardStatus);
}
