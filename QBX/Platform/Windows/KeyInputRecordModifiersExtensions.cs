using QBX.Hardware;
using QBX.Platform.Windows;

namespace QBX.Terminal.Platform.Windows;

public static class KeyInputRecordModifiersExtensions
{
	public static KeyInputRecordModifiers ToKeyInputRecordModifier(this KeyModifiers modifiers)
	{
		var ret = default(KeyInputRecordModifiers);

		if (modifiers.CtrlKey)
			ret |= KeyInputRecordModifiers.LEFT_CTRL_PRESSED;
		if (modifiers.ShiftKey)
			ret |= KeyInputRecordModifiers.SHIFT_PRESSED;
		if (modifiers.AltKey)
			ret |= KeyInputRecordModifiers.LEFT_ALT_PRESSED;
		if (modifiers.AltGrKey)
			ret |= KeyInputRecordModifiers.RIGHT_ALT_PRESSED;
		if (modifiers.CapsLock)
			ret |= KeyInputRecordModifiers.CAPSLOCK_ON;
		if (modifiers.NumLock)
			ret |= KeyInputRecordModifiers.NUMLOCK_ON;

		return ret;
	}
}
