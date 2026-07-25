using System;
using System.Collections.Generic;
using System.Linq;

using QBX.Firmware.Fonts;
using QBX.Hardware;

using SDL3;

namespace QBX.Firmware;

public class KeyboardDriver
{
	Machine _machine;
	KeyboardAltNumPadEntry _altNumPadEntry;

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
		_altNumPadEntry = machine.SystemMemory.KeyboardAltNumPadEntry;

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

	public void InferLayoutFromSDLState()
	{
		KeyboardLayout? newLayout = null;

		foreach (var layoutType in s_keyboardLayouts.Values)
		{
			if (Activator.CreateInstance(layoutType, _machine) is KeyboardLayout layout)
			{
				if (layout.IsHeuristicMatchForCurrentSDLState())
				{
					newLayout = layout;
					break;
				}
			}
		}

		if (newLayout == null)
			newLayout = new KeyboardLayouts.US(_machine);

		if (ActiveKeyboardLayout.GetType() != newLayout.GetType())
			ActiveKeyboardLayout = newLayout;
	}

	public IEnumerable<KeyEvent> GenerateKeyEvents(SDL.Scancode sdlScanCode, SDL.Keymod modifiers, bool isRelease)
	{
		var rawData = new RawKeyEventData(sdlScanCode, modifiers, isRelease);

		ActiveKeyboardLayout.ProcessKeyPress(rawData);

		while (ActiveKeyboardLayout.TryGetNextTranslatedKeyPress(out var translatedData))
		{
			if (!_altNumPadEntry.ProcessKeyEvent(translatedData))
			{
				var keyEvent = new KeyEvent(translatedData);

				if (_altNumPadEntry.InputQueue.Any())
					keyEvent.IsKeyPad = true; // When releasing Alt, if characters were generated, set IsKeyPad.

				yield return keyEvent;
			}
		}

		if (_altNumPadEntry.InputQueue.Any())
		{
			var syntheticRawData = new RawKeyEventData(
				rawData.RawScanCode,
				rawData.Modifiers & ~SDL.Keymod.Alt,
				isRelease: false);

			var keyModifiers = _machine.SystemMemory.KeyboardStatus.GetKeyModifiers();

			while (_altNumPadEntry.InputQueue.TryDequeue(out var altByte))
			{
				var syntheticEventData = new KeyEventData(
					syntheticRawData,
					textCharacter: CP437Encoding.GetCharSemantic(altByte),
					ScanCode.None,
					keyModifiers,
					isRight: false,
					isKeyPad: false,
					isEphemeral: false);

				yield return new KeyEvent(syntheticEventData);
			}
		}
	}
}
