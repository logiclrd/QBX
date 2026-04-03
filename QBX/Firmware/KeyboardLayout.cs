using System.Diagnostics.CodeAnalysis;

using QBX.Hardware;

using SDL3;

namespace QBX.Firmware;

public abstract class KeyboardLayout(Machine machine)
{
	public virtual bool IsHeuristicMatchForCurrentSDLState() => false;

	public abstract void ProcessKeyPress(RawKeyEventData rawData);
	public abstract void Reset();
	public abstract bool TryGetNextTranslatedKeyPress([NotNullWhen(true)] out KeyEventData? data);

	protected void UpdateModifiers(RawKeyEventData data)
	{
		UpdateLockStates(data);

		machine.SystemMemory.KeyboardStatus.UpdateKeyModifiers(data);
	}

	void UpdateLockStates(RawKeyEventData data)
	{
		bool currentCapsLockState = machine.SystemMemory.KeyboardStatus.Byte0.CapsLock;

		bool newCapsLockState = GetUpdatedCapsLockState(data, currentCapsLockState);

		if (newCapsLockState != currentCapsLockState)
		{
			machine.SystemMemory.KeyboardStatus.Byte0.CapsLock = newCapsLockState;
			machine.SystemMemory.KeyboardStatus.Byte2.CapsLockIndicator = newCapsLockState;
		}

		if (!data.IsRelease)
		{
			switch (data.RawScanCode)
			{
				case SDL.Scancode.NumLockClear:
				case SDL.Scancode.Scrolllock:
				case SDL.Scancode.Insert:
					int byte0 = machine.SystemMemory.KeyboardStatus.Byte0;
					int byte2 = machine.SystemMemory.KeyboardStatus.Byte2;

					if (data.RawScanCode == SDL.Scancode.NumLockClear)
					{
						byte0 ^= KeyboardStatus.Byte0Data.NumLockBit;
						byte2 ^= KeyboardStatus.Byte2Data.NumLockIndicatorBit;
					}

					if (data.RawScanCode == SDL.Scancode.Scrolllock)
					{
						byte0 ^= KeyboardStatus.Byte0Data.CapsLockBit;
						byte2 ^= KeyboardStatus.Byte2Data.CapsLockIndicatorBit;
					}

					if (data.RawScanCode == SDL.Scancode.Insert)
						byte0 ^= KeyboardStatus.Byte0Data.InsertBit;

					machine.SystemMemory.KeyboardStatus.Byte0.Set(byte0);
					machine.SystemMemory.KeyboardStatus.Byte2.Set(byte2);

					break;
			}
		}
	}

	protected abstract bool GetUpdatedCapsLockState(RawKeyEventData data, bool currentState);
}
