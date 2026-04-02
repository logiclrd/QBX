using System;
using System.Collections.Generic;
using System.Linq;

using QBX.Hardware;

using SDL3;

namespace QBX.Firmware;

public class KeyboardDriver
{
	Machine _machine;

	public KeyboardLayout ActiveKeyboardLayout
	{
		get;
		set
		{
			value.Reset();
			field = value;
		}
	}

	public KeyboardDriver(Machine machine)
	{
		_machine = machine;

		ActiveKeyboardLayout = new KeyboardLayouts.US(machine);
	}

	static Dictionary<string, Type> s_keyboardLayouts =
		typeof(KeyboardLayout).Assembly.GetTypes()
		.Where(type => typeof(KeyboardLayout).IsAssignableFrom(type))
		.Where(type => !type.IsAbstract)
		.Where(type => type.GetConstructor([typeof(Machine)]) != null)
		.ToDictionary(
			key => key.Name,
			StringComparer.OrdinalIgnoreCase);

	public bool SetLayoutByName(string name)
	{
		if (s_keyboardLayouts.TryGetValue(name, out var layoutType))
		{
			try
			{
				if (Activator.CreateInstance(layoutType, _machine) is KeyboardLayout newLayout)
				{
					ActiveKeyboardLayout = newLayout;

					return true;
				}
			}
			catch { }
		}

		return false;
	}

	public IEnumerable<KeyEvent> GenerateKeyEvents(SDL.Scancode sdlScanCode, SDL.Keymod modifiers, bool isRelease)
	{
		var rawData = new RawKeyEventData(sdlScanCode, modifiers, isRelease);

		ActiveKeyboardLayout.ProcessKeyPress(rawData);

		while (ActiveKeyboardLayout.TryGetNextTranslatedKeyPress(out var translatedData))
			yield return new KeyEvent(translatedData);
	}
}
