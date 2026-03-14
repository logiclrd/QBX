using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using QBX.Hardware;

using SDL3;

namespace QBX.ExecutionEngine.Execution.Events;

public class KeyEventKeyNumber
{
	public const int F1 = 1;
	public const int F2 = 2;
	public const int F3 = 3;
	public const int F4 = 4;
	public const int F5 = 5;
	public const int F6 = 6;
	public const int F7 = 7;
	public const int F8 = 8;
	public const int F9 = 9;
	public const int F10 = 10;
	public const int Up = 11;
	public const int Left = 12;
	public const int Right = 13;
	public const int Down = 14;
	public const int CustomKey0 = 15;
	public const int CustomKey1 = 16;
	public const int CustomKey2 = 17;
	public const int CustomKey3 = 18;
	public const int CustomKey4 = 19;
	public const int CustomKey5 = 20;
	public const int CustomKey6 = 21;
	public const int CustomKey7 = 22;
	public const int CustomKey8 = 23;
	public const int CustomKey9 = 24;
	public const int CustomKey10 = 25;
	public const int F11 = 30;
	public const int F12 = 31;

	static HashSet<int> s_defined = typeof(KeyEventKeyNumber)
		.GetFields(BindingFlags.Public | BindingFlags.Static)
		.Select(field => (int)field.GetValue(null)!)
		.ToHashSet();

	public static IEnumerable<int> AllKeyNumbers => s_defined;

	public static bool IsDefined(int keyNumber) => s_defined.Contains(keyNumber);

	public static bool IsCustomKey(int keyNumber) => (CustomKey0 <= keyNumber) && (keyNumber <= CustomKey10);

	static readonly HashSet<ScanCode> ExtendedKeys =
		new HashSet<ScanCode>()
		{
			ScanCode.Insert,
			ScanCode.Home,
			ScanCode.PageUp,
			ScanCode.Delete,
			ScanCode.End,
			ScanCode.PageDown,
			ScanCode.Up,
			ScanCode.Down,
			ScanCode.Left,
			ScanCode.Right,
		};

	public static int ForKeyEvent(KeyEvent keyEvent, EventConfiguration configuration)
	{
		switch (keyEvent.SDLScanCode)
		{
			case SDL.Scancode.F1: return F1;
			case SDL.Scancode.F2: return F2;
			case SDL.Scancode.F3: return F3;
			case SDL.Scancode.F4: return F4;
			case SDL.Scancode.F5: return F5;
			case SDL.Scancode.F6: return F6;
			case SDL.Scancode.F7: return F7;
			case SDL.Scancode.F8: return F8;
			case SDL.Scancode.F9: return F9;
			case SDL.Scancode.F10: return F10;
			case SDL.Scancode.F11: return F11;
			case SDL.Scancode.F12: return F12;
			case SDL.Scancode.Kp8: return Up;
			case SDL.Scancode.Kp2: return Down;
			case SDL.Scancode.Kp4: return Left;
			case SDL.Scancode.Kp6: return Right;
		}

		var modifiers = KeyEventKeyModifiers.None;

		if (keyEvent.Modifiers.ShiftKey)
			modifiers |= KeyEventKeyModifiers.Shift;
		if (keyEvent.Modifiers.CtrlKey)
			modifiers |= KeyEventKeyModifiers.Control;
		if (keyEvent.Modifiers.AltKey)
			modifiers |= KeyEventKeyModifiers.Alt;
		if (keyEvent.Modifiers.NumLock)
			modifiers |= KeyEventKeyModifiers.NumLock;
		if (keyEvent.Modifiers.CapsLock)
			modifiers |= KeyEventKeyModifiers.CapsLock;
		if (!keyEvent.IsKeyPad && ExtendedKeys.Contains(keyEvent.ScanCode))
			modifiers |= KeyEventKeyModifiers.Extended;

		var key = new KeyEventKeyDefinition(modifiers, keyEvent.ScanCode);

		return configuration.GetCustomKey(key);
	}
}

