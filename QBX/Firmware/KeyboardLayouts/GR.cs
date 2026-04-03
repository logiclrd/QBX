using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using QBX.Hardware;

using SDL3;

namespace QBX.Firmware.KeyboardLayouts;

public class GR(Machine machine) : ShiftLockKeyboardLayout(machine)
{
	public override bool IsHeuristicMatchForCurrentSDLState()
	{
		var yKey = SDL.GetKeyFromScancode(SDL.Scancode.Y, default, false);
		var zKey = SDL.GetKeyFromScancode(SDL.Scancode.Z, default, false);
		var minusKey = SDL.GetKeyFromScancode(SDL.Scancode.Minus, default, false);
		var openSquareBracketKey = SDL.GetKeyFromScancode(SDL.Scancode.Leftbracket, default, false);

		return
			SDL.GetKeyName(yKey).Equals("z", StringComparison.OrdinalIgnoreCase) &&
			SDL.GetKeyName(zKey).Equals("y", StringComparison.OrdinalIgnoreCase) &&
			SDL.GetKeyName(minusKey).Equals("ß", StringComparison.OrdinalIgnoreCase) &&
			SDL.GetKeyName(openSquareBracketKey).Equals("ü", StringComparison.OrdinalIgnoreCase);
	}

	Queue<RawKeyEventData> _events = new Queue<RawKeyEventData>();
	Queue<KeyEventData> _future = new Queue<KeyEventData>();

	public override void Reset()
	{
		_events.Clear();
		_future.Clear();
	}

	public override void ProcessKeyPress(RawKeyEventData data)
		=> _events.Enqueue(data);

	public override bool TryGetNextTranslatedKeyPress([NotNullWhen(true)] out KeyEventData? data)
	{
		if (_future.TryDequeue(out data))
			return true;

		data = null;

		if (!_events.TryDequeue(out var rawData))
			return false;

		UpdateModifiers(rawData);

		if (UpdateState(rawData))
			return TryGetNextTranslatedKeyPress(out data);

		var modifiers = machine.SystemMemory.KeyboardStatus.GetKeyModifiers();

		ScanCode scanCode = default;
		bool isRight = false;
		bool isKeyPad = false;
		bool isEphemeral = false;
		char textCharacter = '\0';

		switch (rawData.RawScanCode)
		{
			case SDL.Scancode.LCtrl:
			case SDL.Scancode.RCtrl:
			case SDL.Scancode.LAlt:
			case SDL.Scancode.RAlt:
			case SDL.Scancode.LShift:
			case SDL.Scancode.RShift:
			case SDL.Scancode.Capslock:
			case SDL.Scancode.NumLockClear:
			case SDL.Scancode.Scrolllock:
			case SDL.Scancode.Pause:
			{
				switch (rawData.RawScanCode)
				{
					case SDL.Scancode.LCtrl: scanCode = ScanCode.Control; break;
					case SDL.Scancode.RCtrl: scanCode = ScanCode.Control; isRight = true; break;
					case SDL.Scancode.LAlt: scanCode = ScanCode.Alt; break;
					case SDL.Scancode.RAlt: scanCode = ScanCode.Alt; isRight = true; break;
					case SDL.Scancode.LShift: scanCode = ScanCode.LeftShift; break;
					case SDL.Scancode.RShift: scanCode = ScanCode.RightShift; isRight = true; break;
					case SDL.Scancode.Capslock: scanCode = ScanCode.CapsLock; break;
					case SDL.Scancode.NumLockClear: scanCode = ScanCode.NumLock; break;
					case SDL.Scancode.Scrolllock: scanCode = ScanCode.ScrollLock; break;
				}

				isEphemeral = true;

				break;
			}

			case SDL.Scancode.KpDivide:
			case SDL.Scancode.KpMultiply:
			case SDL.Scancode.KpMinus:
			case SDL.Scancode.KpPlus:
			case SDL.Scancode.KpEnter:
			case SDL.Scancode.KpPeriod:
			case SDL.Scancode.Kp0:
			case SDL.Scancode.Kp1:
			case SDL.Scancode.Kp2:
			case SDL.Scancode.Kp3:
			case SDL.Scancode.Kp4:
			case SDL.Scancode.Kp5:
			case SDL.Scancode.Kp6:
			case SDL.Scancode.Kp7:
			case SDL.Scancode.Kp8:
			case SDL.Scancode.Kp9:
				isKeyPad = true;
				break;
		}

		if ((scanCode == default)
		 && (_activeTranslation != TranslationType.None)
		 && (rawData.RawScanCode == SDL.Scancode.Space))
		{
			// This path ignores Ctrl and Alt.
			switch (_activeTranslation)
			{
				case TranslationType.Acute:
					scanCode = ScanCode.RightBracket;
					textCharacter = '\'';
					break;
				case TranslationType.Grave:
					scanCode = ScanCode.RightBracket;
					textCharacter = '\'';
					break;
				case TranslationType.Circumflex:
					scanCode = ScanCode.Grave;
					textCharacter = '^';
					break;
				default:
					scanCode = ScanCode.Space;
					textCharacter = ' ';
					break;
			}

			_activeTranslation = TranslationType.None;
		}

		if (scanCode == default)
		{
			bool upperCase = modifiers.ShiftKey ^ modifiers.CapsLock;
			bool altGr = (rawData.Modifiers & SDL.Keymod.RAlt) != 0;

			if (modifiers.AltKey)
			{
				if (modifiers.ShiftKey)
					altGr = false;

				switch (rawData.RawScanCode)
				{
					case SDL.Scancode.Escape: scanCode = ScanCode.Escape; break;
					case SDL.Scancode.F1: scanCode = ScanCode.AltF1; break;
					case SDL.Scancode.F2: scanCode = ScanCode.AltF2; break;
					case SDL.Scancode.F3: scanCode = ScanCode.AltF3; break;
					case SDL.Scancode.F4: scanCode = ScanCode.AltF4; break;
					case SDL.Scancode.F5: scanCode = ScanCode.AltF5; break;
					case SDL.Scancode.F6: scanCode = ScanCode.AltF6; break;
					case SDL.Scancode.F7: scanCode = ScanCode.AltF7; break;
					case SDL.Scancode.F8: scanCode = ScanCode.AltF8; break;
					case SDL.Scancode.F9: scanCode = ScanCode.AltF9; break;
					case SDL.Scancode.F10: scanCode = ScanCode.AltF10; break;
					case SDL.Scancode.F11: scanCode = ScanCode.AltF11; break;
					case SDL.Scancode.F12: scanCode = ScanCode.AltF12; break;
					case SDL.Scancode.Grave: scanCode = ScanCode.Grave; break;
					case SDL.Scancode.Alpha1: scanCode = ScanCode.Alt1; break;
					case SDL.Scancode.Alpha2: scanCode = ScanCode.Alt2; if (altGr) textCharacter = '²'; break;
					case SDL.Scancode.Alpha3: scanCode = ScanCode.Alt3; if (altGr) textCharacter = 'ⁿ'; break;
					case SDL.Scancode.Alpha4: scanCode = ScanCode.Alt4; break;
					case SDL.Scancode.Alpha5: scanCode = ScanCode.Alt5; break;
					case SDL.Scancode.Alpha6: scanCode = ScanCode.Alt6; break;
					case SDL.Scancode.Alpha7: scanCode = ScanCode.Alt7; if (altGr) textCharacter = '{'; break;
					case SDL.Scancode.Alpha8: scanCode = ScanCode.Alt8; if (altGr) textCharacter = '[';break;
					case SDL.Scancode.Alpha9: scanCode = ScanCode.Alt9; if (altGr) textCharacter = ']';break;
					case SDL.Scancode.Alpha0: scanCode = ScanCode.Alt0; if (altGr) textCharacter = '}';break;
					case SDL.Scancode.Minus: scanCode = ScanCode.AltMinus; break;
					case SDL.Scancode.Equals: scanCode = ScanCode.AltEquals; break;
					case SDL.Scancode.Backspace: scanCode = ScanCode.Backspace; break;
					case SDL.Scancode.Q: scanCode = ScanCode.Q; if (altGr) textCharacter = '@'; break;
					case SDL.Scancode.W: scanCode = ScanCode.W; break;
					case SDL.Scancode.E: scanCode = ScanCode.E; break;
					case SDL.Scancode.R: scanCode = ScanCode.R; break;
					case SDL.Scancode.T: scanCode = ScanCode.T; break;
					case SDL.Scancode.Y: scanCode = ScanCode.Y; break;
					case SDL.Scancode.U: scanCode = ScanCode.U; break;
					case SDL.Scancode.I: scanCode = ScanCode.I; break;
					case SDL.Scancode.O: scanCode = ScanCode.O; break;
					case SDL.Scancode.P: scanCode = ScanCode.P; break;
					case SDL.Scancode.Leftbracket: scanCode = ScanCode.LeftBracket; break;
					case SDL.Scancode.Rightbracket: scanCode = ScanCode.RightBracket; if (altGr) textCharacter = '~'; break;
					case SDL.Scancode.Backslash: scanCode = ScanCode.Backslash; break;
					case SDL.Scancode.A: scanCode = ScanCode.A; break;
					case SDL.Scancode.S: scanCode = ScanCode.S; break;
					case SDL.Scancode.D: scanCode = ScanCode.D; break;
					case SDL.Scancode.F: scanCode = ScanCode.F; break;
					case SDL.Scancode.G: scanCode = ScanCode.G; break;
					case SDL.Scancode.H: scanCode = ScanCode.H; break;
					case SDL.Scancode.J: scanCode = ScanCode.J; break;
					case SDL.Scancode.K: scanCode = ScanCode.K; break;
					case SDL.Scancode.L: scanCode = ScanCode.L; break;
					case SDL.Scancode.Semicolon: scanCode = ScanCode.Semicolon; break;
					case SDL.Scancode.Apostrophe: scanCode = ScanCode.Apostrophe; break;
					case SDL.Scancode.Return: scanCode = ScanCode.Return; break;
					case SDL.Scancode.Z: scanCode = ScanCode.Z; break;
					case SDL.Scancode.X: scanCode = ScanCode.X; break;
					case SDL.Scancode.C: scanCode = ScanCode.C; break;
					case SDL.Scancode.V: scanCode = ScanCode.V; break;
					case SDL.Scancode.B: scanCode = ScanCode.B; break;
					case SDL.Scancode.N: scanCode = ScanCode.N; break;
					case SDL.Scancode.M: scanCode = ScanCode.M; if (altGr) textCharacter = 'μ'; break;
					case SDL.Scancode.Comma: scanCode = ScanCode.Comma; break;
					case SDL.Scancode.Period: scanCode = ScanCode.Period; break;
					case SDL.Scancode.Slash: scanCode = ScanCode.Slash; break;
					case SDL.Scancode.Space: scanCode = ScanCode.Space; textCharacter = ' '; break;
					case SDL.Scancode.Insert: scanCode = ScanCode.AltInsert; break;
					case SDL.Scancode.Delete: scanCode = ScanCode.AltDelete; break;
					case SDL.Scancode.Home: scanCode = ScanCode.AltHome; break;
					case SDL.Scancode.End: scanCode = ScanCode.AltEnd; break;
					case SDL.Scancode.Pageup: scanCode = ScanCode.AltPageUp; break;
					case SDL.Scancode.Pagedown: scanCode = ScanCode.AltPageDown; break;
					case SDL.Scancode.KpDivide: scanCode = ScanCode.AltKpDivide; break;
					case SDL.Scancode.KpMultiply: scanCode = ScanCode.KpMultiply; break;
					case SDL.Scancode.KpMinus: scanCode = ScanCode.KpMinus; break;
					case SDL.Scancode.KpPlus: scanCode = ScanCode.KpPlus; break;
					case SDL.Scancode.KpEnter: scanCode = ScanCode.AltKpEnter; break;
					case SDL.Scancode.Up: scanCode = ScanCode.AltUp; break;
					case SDL.Scancode.Left: scanCode = ScanCode.AltLeft; break;
					case SDL.Scancode.Down: scanCode = ScanCode.AltDown; break;
					case SDL.Scancode.Right: scanCode = ScanCode.AltRight; break;

					default: return false;
				}
			}
			else if (modifiers.CtrlKey)
			{
				switch (rawData.RawScanCode)
				{
					case SDL.Scancode.Escape: scanCode = ScanCode.Escape; textCharacter = (char)27; break;
					case SDL.Scancode.F1: scanCode = ScanCode.CtrlF1; break;
					case SDL.Scancode.F2: scanCode = ScanCode.CtrlF2; break;
					case SDL.Scancode.F3: scanCode = ScanCode.CtrlF3; break;
					case SDL.Scancode.F4: scanCode = ScanCode.CtrlF4; break;
					case SDL.Scancode.F5: scanCode = ScanCode.CtrlF5; break;
					case SDL.Scancode.F6: scanCode = ScanCode.CtrlF6; break;
					case SDL.Scancode.F7: scanCode = ScanCode.CtrlF7; break;
					case SDL.Scancode.F8: scanCode = ScanCode.CtrlF8; break;
					case SDL.Scancode.F9: scanCode = ScanCode.CtrlF9; break;
					case SDL.Scancode.F10: scanCode = ScanCode.CtrlF10; break;
					case SDL.Scancode.F11: scanCode = ScanCode.CtrlF11; break;
					case SDL.Scancode.F12: scanCode = ScanCode.CtrlF12; break;
					case SDL.Scancode.Alpha2: scanCode = ScanCode._2; break;
					case SDL.Scancode.Alpha6: scanCode = ScanCode._6; textCharacter = (char)30; break;
					case SDL.Scancode.Minus: scanCode = ScanCode.Minus; textCharacter = (char)31; break;
					case SDL.Scancode.Backspace: scanCode = ScanCode.Backspace; textCharacter = (char)127; break;
					case SDL.Scancode.Tab: scanCode = ScanCode.CtrlTab; break;
					case SDL.Scancode.Q: scanCode = ScanCode.Q; textCharacter = (char)17; break;
					case SDL.Scancode.W: scanCode = ScanCode.W; textCharacter = (char)23; break;
					case SDL.Scancode.E: scanCode = ScanCode.E; textCharacter = (char)5; break;
					case SDL.Scancode.R: scanCode = ScanCode.R; textCharacter = (char)18; break;
					case SDL.Scancode.T: scanCode = ScanCode.T; textCharacter = (char)20; break;
					case SDL.Scancode.Y: scanCode = ScanCode.Y; textCharacter = (char)26; break; // Z
					case SDL.Scancode.U: scanCode = ScanCode.U; textCharacter = (char)21; break;
					case SDL.Scancode.I: scanCode = ScanCode.I; textCharacter = (char)9; break;
					case SDL.Scancode.O: scanCode = ScanCode.O; textCharacter = (char)15; break;
					case SDL.Scancode.P: scanCode = ScanCode.P; textCharacter = (char)16; break;
					case SDL.Scancode.Leftbracket: scanCode = ScanCode.LeftBracket; textCharacter = (char)27; break;
					case SDL.Scancode.Rightbracket: scanCode = ScanCode.RightBracket; textCharacter = (char)29; break;
					case SDL.Scancode.Backslash: scanCode = ScanCode.Backslash; textCharacter = (char)28; break;
					case SDL.Scancode.A: scanCode = ScanCode.A; textCharacter = (char)1; break;
					case SDL.Scancode.S: scanCode = ScanCode.S; textCharacter = (char)19; break;
					case SDL.Scancode.D: scanCode = ScanCode.D; textCharacter = (char)4; break;
					case SDL.Scancode.F: scanCode = ScanCode.F; textCharacter = (char)6; break;
					case SDL.Scancode.G: scanCode = ScanCode.G; textCharacter = (char)7; break;
					case SDL.Scancode.H: scanCode = ScanCode.H; textCharacter = (char)8; break;
					case SDL.Scancode.J: scanCode = ScanCode.J; textCharacter = (char)10; break;
					case SDL.Scancode.K: scanCode = ScanCode.K; textCharacter = (char)11; break;
					case SDL.Scancode.L: scanCode = ScanCode.L; textCharacter = (char)12; break;
					case SDL.Scancode.Return: scanCode = ScanCode.Return; textCharacter = (char)10; break;
					case SDL.Scancode.Z: scanCode = ScanCode.Z; textCharacter = (char)25; break; // Y
					case SDL.Scancode.X: scanCode = ScanCode.X; textCharacter = (char)24; break;
					case SDL.Scancode.C: scanCode = ScanCode.C; textCharacter = (char)3; break;
					case SDL.Scancode.V: scanCode = ScanCode.V; textCharacter = (char)22; break;
					case SDL.Scancode.B: scanCode = ScanCode.B; textCharacter = (char)2; break;
					case SDL.Scancode.N: scanCode = ScanCode.N; textCharacter = (char)14; break;
					case SDL.Scancode.M: scanCode = ScanCode.M; textCharacter = (char)13; break;
					case SDL.Scancode.Insert: scanCode = ScanCode.CtrlInsert; break;
					case SDL.Scancode.Delete: scanCode = ScanCode.CtrlDelete; break;
					case SDL.Scancode.Home: scanCode = ScanCode.CtrlHome; break;
					case SDL.Scancode.End: scanCode = ScanCode.CtrlEnd; break;
					case SDL.Scancode.Pageup: scanCode = ScanCode.CtrlPageUp; break;
					case SDL.Scancode.Pagedown: scanCode = ScanCode.CtrlPageDown; break;
					case SDL.Scancode.KpDivide: scanCode = ScanCode.CtrlKpDivide; break;
					case SDL.Scancode.KpMultiply: scanCode = ScanCode.CtrlKpMultiply; break;
					case SDL.Scancode.KpMinus: scanCode = ScanCode.CtrlKpMinus; break;
					case SDL.Scancode.Kp7: scanCode = ScanCode.CtrlHome; break;
					case SDL.Scancode.Kp8: scanCode = ScanCode.CtrlUp; break;
					case SDL.Scancode.Kp9: scanCode = ScanCode.CtrlPageUp; break;
					case SDL.Scancode.KpPlus: scanCode = ScanCode.CtrlKpPlus; break;
					case SDL.Scancode.Kp4: scanCode = ScanCode.CtrlLeft; break;
					case SDL.Scancode.Kp5: scanCode = ScanCode.CtrlKp5; break;
					case SDL.Scancode.Kp6: scanCode = ScanCode.CtrlRight; break;
					case SDL.Scancode.Kp1: scanCode = ScanCode.CtrlEnd; break;
					case SDL.Scancode.Kp2: scanCode = ScanCode.CtrlDown; break;
					case SDL.Scancode.Kp3: scanCode = ScanCode.CtrlPageDown; break;
					case SDL.Scancode.KpEnter: scanCode = ScanCode.Return; textCharacter = (char)10; break;
					case SDL.Scancode.Kp0: scanCode = ScanCode.CtrlInsert; break;
					case SDL.Scancode.KpPeriod: scanCode = ScanCode.CtrlDelete; break;
					case SDL.Scancode.Up: scanCode = ScanCode.CtrlUp; break;
					case SDL.Scancode.Left: scanCode = ScanCode.CtrlLeft; break;
					case SDL.Scancode.Down: scanCode = ScanCode.CtrlDown; break;
					case SDL.Scancode.Right: scanCode = ScanCode.CtrlRight; break;

					default: return false;
				}
			}
			else if (modifiers.ShiftKey)
			{
				switch (rawData.RawScanCode)
				{
					case SDL.Scancode.Escape: scanCode = ScanCode.Escape; textCharacter = (char)27; break;
					case SDL.Scancode.F1: scanCode = ScanCode.ShiftF1; break;
					case SDL.Scancode.F2: scanCode = ScanCode.ShiftF2; break;
					case SDL.Scancode.F3: scanCode = ScanCode.ShiftF3; break;
					case SDL.Scancode.F4: scanCode = ScanCode.ShiftF4; break;
					case SDL.Scancode.F5: scanCode = ScanCode.ShiftF5; break;
					case SDL.Scancode.F6: scanCode = ScanCode.ShiftF6; break;
					case SDL.Scancode.F7: scanCode = ScanCode.ShiftF7; break;
					case SDL.Scancode.F8: scanCode = ScanCode.ShiftF8; break;
					case SDL.Scancode.F9: scanCode = ScanCode.ShiftF9; break;
					case SDL.Scancode.F10: scanCode = ScanCode.ShiftF10; break;
					case SDL.Scancode.F11: scanCode = ScanCode.ShiftF11; break;
					case SDL.Scancode.F12: scanCode = ScanCode.ShiftF12; break;
					case SDL.Scancode.Grave: scanCode = ScanCode.Grave; textCharacter = '°'; break;
					case SDL.Scancode.Alpha1: scanCode = ScanCode._1; textCharacter = '!'; break;
					case SDL.Scancode.Alpha2: scanCode = ScanCode._2; textCharacter = '"'; break;
					case SDL.Scancode.Alpha3: scanCode = ScanCode._3; break;
					case SDL.Scancode.Alpha4: scanCode = ScanCode._4; textCharacter = '$'; break;
					case SDL.Scancode.Alpha5: scanCode = ScanCode._5; textCharacter = '%'; break;
					case SDL.Scancode.Alpha6: scanCode = ScanCode._6; textCharacter = '&'; break;
					case SDL.Scancode.Alpha7: scanCode = ScanCode._7; textCharacter = '/'; break;
					case SDL.Scancode.Alpha8: scanCode = ScanCode._8; textCharacter = '('; break;
					case SDL.Scancode.Alpha9: scanCode = ScanCode._9; textCharacter = ')'; break;
					case SDL.Scancode.Alpha0: scanCode = ScanCode._0; textCharacter = '='; break;
					case SDL.Scancode.Minus: scanCode = ScanCode.Minus; textCharacter = 'ß'; break;
					case SDL.Scancode.Equals: scanCode = ScanCode.Equals; break;
					case SDL.Scancode.Backspace: scanCode = ScanCode.Backspace; textCharacter = (char)8; break;
					case SDL.Scancode.Tab: scanCode = ScanCode.Tab; break;
					case SDL.Scancode.Q: scanCode = ScanCode.Q; textCharacter = upperCase ? 'Q' : 'q'; break;
					case SDL.Scancode.W: scanCode = ScanCode.W; textCharacter = upperCase ? 'W' : 'w'; break;
					case SDL.Scancode.E: scanCode = ScanCode.E; textCharacter = upperCase ? 'E' : 'e'; break;
					case SDL.Scancode.R: scanCode = ScanCode.R; textCharacter = upperCase ? 'R' : 'r'; break;
					case SDL.Scancode.T: scanCode = ScanCode.T; textCharacter = upperCase ? 'T' : 't'; break;
					case SDL.Scancode.Y: scanCode = ScanCode.Y; textCharacter = upperCase ? 'Z' : 'z'; break;
					case SDL.Scancode.U: scanCode = ScanCode.U; textCharacter = upperCase ? 'U' : 'u'; break;
					case SDL.Scancode.I: scanCode = ScanCode.I; textCharacter = upperCase ? 'I' : 'i'; break;
					case SDL.Scancode.O: scanCode = ScanCode.O; textCharacter = upperCase ? 'O' : 'o'; break;
					case SDL.Scancode.P: scanCode = ScanCode.P; textCharacter = upperCase ? 'P' : 'p'; break;
					case SDL.Scancode.Leftbracket: scanCode = ScanCode.LeftBracket; textCharacter = upperCase ? 'Ü' : 'ü'; break;
					case SDL.Scancode.Rightbracket: scanCode = ScanCode.RightBracket; textCharacter = '*'; break;
					case SDL.Scancode.Backslash: scanCode = ScanCode.Backslash; textCharacter = '\''; break;
					case SDL.Scancode.A: scanCode = ScanCode.A; textCharacter = upperCase ? 'A' : 'a'; break;
					case SDL.Scancode.S: scanCode = ScanCode.S; textCharacter = upperCase ? 'S' : 's'; break;
					case SDL.Scancode.D: scanCode = ScanCode.D; textCharacter = upperCase ? 'D' : 'd'; break;
					case SDL.Scancode.F: scanCode = ScanCode.F; textCharacter = upperCase ? 'F' : 'f'; break;
					case SDL.Scancode.G: scanCode = ScanCode.G; textCharacter = upperCase ? 'G' : 'g'; break;
					case SDL.Scancode.H: scanCode = ScanCode.H; textCharacter = upperCase ? 'H' : 'h'; break;
					case SDL.Scancode.J: scanCode = ScanCode.J; textCharacter = upperCase ? 'J' : 'j'; break;
					case SDL.Scancode.K: scanCode = ScanCode.K; textCharacter = upperCase ? 'K' : 'k'; break;
					case SDL.Scancode.L: scanCode = ScanCode.L; textCharacter = upperCase ? 'L' : 'l'; break;
					case SDL.Scancode.Semicolon: scanCode = ScanCode.Semicolon; textCharacter = upperCase ? 'Ö' : 'ö'; break;
					case SDL.Scancode.Apostrophe: scanCode = ScanCode.Apostrophe; textCharacter = upperCase ? 'Ä' : 'ä'; break;
					case SDL.Scancode.Return: scanCode = ScanCode.Return; textCharacter = (char)13; break;
					case SDL.Scancode.Z: scanCode = ScanCode.Z; textCharacter = upperCase ? 'Y' : 'y'; break;
					case SDL.Scancode.X: scanCode = ScanCode.X; textCharacter = upperCase ? 'X' : 'x'; break;
					case SDL.Scancode.C: scanCode = ScanCode.C; textCharacter = upperCase ? 'C' : 'c'; break;
					case SDL.Scancode.V: scanCode = ScanCode.V; textCharacter = upperCase ? 'V' : 'v'; break;
					case SDL.Scancode.B: scanCode = ScanCode.B; textCharacter = upperCase ? 'B' : 'b'; break;
					case SDL.Scancode.N: scanCode = ScanCode.N; textCharacter = upperCase ? 'N' : 'n'; break;
					case SDL.Scancode.M: scanCode = ScanCode.M; textCharacter = upperCase ? 'M' : 'm'; break;
					case SDL.Scancode.Comma: scanCode = ScanCode.Comma; textCharacter = ';'; break;
					case SDL.Scancode.Period: scanCode = ScanCode.Period; textCharacter = ':'; break;
					case SDL.Scancode.Slash: scanCode = ScanCode.Slash; textCharacter = '_'; break;
					case SDL.Scancode.Space: scanCode = ScanCode.Space; textCharacter = ' '; break;
					case SDL.Scancode.Insert: scanCode = ScanCode.Insert; break;
					case SDL.Scancode.Delete: scanCode = ScanCode.Delete; break;
					case SDL.Scancode.Home: scanCode = ScanCode.Home; break;
					case SDL.Scancode.End: scanCode = ScanCode.End; break;
					case SDL.Scancode.Pageup: scanCode = ScanCode.PageUp; break;
					case SDL.Scancode.Pagedown: scanCode = ScanCode.PageDown; break;
					case SDL.Scancode.KpDivide: textCharacter = '/'; break;
					case SDL.Scancode.KpMultiply: scanCode = ScanCode.KpMultiply; textCharacter = '*'; break;
					case SDL.Scancode.KpMinus: scanCode = ScanCode.KpMinus; textCharacter = '-'; break;
					case SDL.Scancode.Kp7: scanCode = ScanCode.Home; if (modifiers.NumLock) textCharacter = '7'; break;
					case SDL.Scancode.Kp8: scanCode = ScanCode.Up; if (modifiers.NumLock) textCharacter = '8'; break;
					case SDL.Scancode.Kp9: scanCode = ScanCode.PageUp; if (modifiers.NumLock) textCharacter = '9'; break;
					case SDL.Scancode.KpPlus: scanCode = ScanCode.KpPlus; textCharacter = '+'; break;
					case SDL.Scancode.Kp4: scanCode = ScanCode.Left; if (modifiers.NumLock) textCharacter = '4'; break;
					case SDL.Scancode.Kp5: scanCode = ScanCode.Kp5; if (modifiers.NumLock) textCharacter = '5'; break;
					case SDL.Scancode.Kp6: scanCode = ScanCode.Right; if (modifiers.NumLock) textCharacter = '6'; break;
					case SDL.Scancode.Kp1: scanCode = ScanCode.End; if (modifiers.NumLock) textCharacter = '1'; break;
					case SDL.Scancode.Kp2: scanCode = ScanCode.Down; if (modifiers.NumLock) textCharacter = '2'; break;
					case SDL.Scancode.Kp3: scanCode = ScanCode.PageDown; if (modifiers.NumLock) textCharacter = '3'; break;
					case SDL.Scancode.KpEnter: scanCode = ScanCode.Return; textCharacter = (char)13; break;
					case SDL.Scancode.Kp0: scanCode = ScanCode.Insert; if (modifiers.NumLock) textCharacter = '0'; break;
					case SDL.Scancode.KpPeriod: scanCode = ScanCode.Delete; if (modifiers.NumLock) textCharacter = '.'; break;
					case SDL.Scancode.Up: scanCode = ScanCode.Up; break;
					case SDL.Scancode.Left: scanCode = ScanCode.Left; break;
					case SDL.Scancode.Down: scanCode = ScanCode.Down; break;
					case SDL.Scancode.Right: scanCode = ScanCode.Right; break;

					default: return false;
				}
			}
			else
			{
				switch (rawData.RawScanCode)
				{
					case SDL.Scancode.Escape: scanCode = ScanCode.Escape; textCharacter = (char)27; break;
					case SDL.Scancode.F1: scanCode = ScanCode.F1; break;
					case SDL.Scancode.F2: scanCode = ScanCode.F2; break;
					case SDL.Scancode.F3: scanCode = ScanCode.F3; break;
					case SDL.Scancode.F4: scanCode = ScanCode.F4; break;
					case SDL.Scancode.F5: scanCode = ScanCode.F5; break;
					case SDL.Scancode.F6: scanCode = ScanCode.F6; break;
					case SDL.Scancode.F7: scanCode = ScanCode.F7; break;
					case SDL.Scancode.F8: scanCode = ScanCode.F8; break;
					case SDL.Scancode.F9: scanCode = ScanCode.F9; break;
					case SDL.Scancode.F10: scanCode = ScanCode.F10; break;
					case SDL.Scancode.F11: scanCode = ScanCode.F11; break;
					case SDL.Scancode.F12: scanCode = ScanCode.F12; break;
					case SDL.Scancode.Grave: scanCode = ScanCode.Grave; break;
					case SDL.Scancode.Alpha1: scanCode = ScanCode._1; textCharacter = '1'; break;
					case SDL.Scancode.Alpha2: scanCode = ScanCode._2; textCharacter = '2'; break;
					case SDL.Scancode.Alpha3: scanCode = ScanCode._3; textCharacter = '3'; break;
					case SDL.Scancode.Alpha4: scanCode = ScanCode._4; textCharacter = '4'; break;
					case SDL.Scancode.Alpha5: scanCode = ScanCode._5; textCharacter = '5'; break;
					case SDL.Scancode.Alpha6: scanCode = ScanCode._6; textCharacter = '6'; break;
					case SDL.Scancode.Alpha7: scanCode = ScanCode._7; textCharacter = '7'; break;
					case SDL.Scancode.Alpha8: scanCode = ScanCode._8; textCharacter = '8'; break;
					case SDL.Scancode.Alpha9: scanCode = ScanCode._9; textCharacter = '9'; break;
					case SDL.Scancode.Alpha0: scanCode = ScanCode._0; textCharacter = '0'; break;
					case SDL.Scancode.Minus: scanCode = ScanCode.Minus; textCharacter = 'ß'; break;
					case SDL.Scancode.Equals: scanCode = ScanCode.Equals; break;
					case SDL.Scancode.Backspace: scanCode = ScanCode.Backspace; textCharacter = (char)8; break;
					case SDL.Scancode.Tab: scanCode = ScanCode.Tab; textCharacter = '\t'; break;
					case SDL.Scancode.Q: scanCode = ScanCode.Q; textCharacter = upperCase ? 'Q' : 'q'; break;
					case SDL.Scancode.W: scanCode = ScanCode.W; textCharacter = upperCase ? 'W' : 'w'; break;
					case SDL.Scancode.E: scanCode = ScanCode.E; textCharacter = upperCase ? 'E' : 'e'; break;
					case SDL.Scancode.R: scanCode = ScanCode.R; textCharacter = upperCase ? 'R' : 'r'; break;
					case SDL.Scancode.T: scanCode = ScanCode.T; textCharacter = upperCase ? 'T' : 't'; break;
					case SDL.Scancode.Y: scanCode = ScanCode.Y; textCharacter = upperCase ? 'Z' : 'z'; break;
					case SDL.Scancode.U: scanCode = ScanCode.U; textCharacter = upperCase ? 'U' : 'u'; break;
					case SDL.Scancode.I: scanCode = ScanCode.I; textCharacter = upperCase ? 'I' : 'i'; break;
					case SDL.Scancode.O: scanCode = ScanCode.O; textCharacter = upperCase ? 'O' : 'o'; break;
					case SDL.Scancode.P: scanCode = ScanCode.P; textCharacter = upperCase ? 'P' : 'p'; break;
					case SDL.Scancode.Leftbracket: scanCode = ScanCode.LeftBracket; textCharacter = upperCase ? 'Ü' : 'ü'; break;
					case SDL.Scancode.Rightbracket: scanCode = ScanCode.RightBracket; textCharacter = '+'; break;
					case SDL.Scancode.Backslash: scanCode = ScanCode.Backslash; textCharacter = '#'; break;
					case SDL.Scancode.A: scanCode = ScanCode.A; textCharacter = upperCase ? 'A' : 'a'; break;
					case SDL.Scancode.S: scanCode = ScanCode.S; textCharacter = upperCase ? 'S' : 's'; break;
					case SDL.Scancode.D: scanCode = ScanCode.D; textCharacter = upperCase ? 'D' : 'd'; break;
					case SDL.Scancode.F: scanCode = ScanCode.F; textCharacter = upperCase ? 'F' : 'f'; break;
					case SDL.Scancode.G: scanCode = ScanCode.G; textCharacter = upperCase ? 'G' : 'g'; break;
					case SDL.Scancode.H: scanCode = ScanCode.H; textCharacter = upperCase ? 'H' : 'h'; break;
					case SDL.Scancode.J: scanCode = ScanCode.J; textCharacter = upperCase ? 'J' : 'j'; break;
					case SDL.Scancode.K: scanCode = ScanCode.K; textCharacter = upperCase ? 'K' : 'k'; break;
					case SDL.Scancode.L: scanCode = ScanCode.L; textCharacter = upperCase ? 'L' : 'l'; break;
					case SDL.Scancode.Semicolon: scanCode = ScanCode.Semicolon; textCharacter = upperCase ? 'Ö' : 'ö'; break;
					case SDL.Scancode.Apostrophe: scanCode = ScanCode.Apostrophe; textCharacter = upperCase ? 'Ä' : 'ä'; break;
					case SDL.Scancode.Return: scanCode = ScanCode.Return; textCharacter = (char)13; break;
					case SDL.Scancode.Z: scanCode = ScanCode.Z; textCharacter = upperCase ? 'Y' : 'y'; break;
					case SDL.Scancode.X: scanCode = ScanCode.X; textCharacter = upperCase ? 'X' : 'x'; break;
					case SDL.Scancode.C: scanCode = ScanCode.C; textCharacter = upperCase ? 'C' : 'c'; break;
					case SDL.Scancode.V: scanCode = ScanCode.V; textCharacter = upperCase ? 'V' : 'v'; break;
					case SDL.Scancode.B: scanCode = ScanCode.B; textCharacter = upperCase ? 'B' : 'b'; break;
					case SDL.Scancode.N: scanCode = ScanCode.N; textCharacter = upperCase ? 'N' : 'n'; break;
					case SDL.Scancode.M: scanCode = ScanCode.M; textCharacter = upperCase ? 'M' : 'm'; break;
					case SDL.Scancode.Comma: scanCode = ScanCode.Comma; textCharacter = ','; break;
					case SDL.Scancode.Period: scanCode = ScanCode.Period; textCharacter = '.'; break;
					case SDL.Scancode.Slash: scanCode = ScanCode.Slash; textCharacter = '-'; break;
					case SDL.Scancode.Space: scanCode = ScanCode.Space; textCharacter = ' '; break;
					case SDL.Scancode.Insert: scanCode = ScanCode.Insert; break;
					case SDL.Scancode.Delete: scanCode = ScanCode.Delete; break;
					case SDL.Scancode.Home: scanCode = ScanCode.Home; break;
					case SDL.Scancode.End: scanCode = ScanCode.End; break;
					case SDL.Scancode.Pageup: scanCode = ScanCode.PageUp; break;
					case SDL.Scancode.Pagedown: scanCode = ScanCode.PageDown; break;
					case SDL.Scancode.KpDivide: textCharacter = '/'; break;
					case SDL.Scancode.KpMultiply: textCharacter = '*'; break;
					case SDL.Scancode.KpMinus: textCharacter = '-'; break;
					case SDL.Scancode.Kp7: scanCode = ScanCode.Home; if (modifiers.NumLock) textCharacter = '7'; break;
					case SDL.Scancode.Kp8: scanCode = ScanCode.Up; if (modifiers.NumLock) textCharacter = '8'; break;
					case SDL.Scancode.Kp9: scanCode = ScanCode.PageUp; if (modifiers.NumLock) textCharacter = '9'; break;
					case SDL.Scancode.KpPlus: scanCode = ScanCode.KpPlus; textCharacter = '+'; break;
					case SDL.Scancode.Kp4: scanCode = ScanCode.Left; if (modifiers.NumLock) textCharacter = '4'; break;
					case SDL.Scancode.Kp5: scanCode = ScanCode.Kp5; if (modifiers.NumLock) textCharacter = '5'; break;
					case SDL.Scancode.Kp6: scanCode = ScanCode.Right; if (modifiers.NumLock) textCharacter = '6'; break;
					case SDL.Scancode.Kp1: scanCode = ScanCode.End; if (modifiers.NumLock) textCharacter = '1'; break;
					case SDL.Scancode.Kp2: scanCode = ScanCode.Down; if (modifiers.NumLock) textCharacter = '2'; break;
					case SDL.Scancode.Kp3: scanCode = ScanCode.PageDown; if (modifiers.NumLock) textCharacter = '3'; break;
					case SDL.Scancode.KpEnter: scanCode = ScanCode.Return; textCharacter = (char)13; break;
					case SDL.Scancode.Kp0: scanCode = ScanCode.Insert; if (modifiers.NumLock) textCharacter = '0'; break;
					case SDL.Scancode.KpPeriod: scanCode = ScanCode.Delete; if (modifiers.NumLock) textCharacter = '.'; break;
					case SDL.Scancode.Up: scanCode = ScanCode.Up; break;
					case SDL.Scancode.Left: scanCode = ScanCode.Left; break;
					case SDL.Scancode.Down: scanCode = ScanCode.Down; break;
					case SDL.Scancode.Right: scanCode = ScanCode.Right; break;

					default: return false;
				}
			}
		}

		char failedAccent = default;

		if (textCharacter != default)
		{
			textCharacter = TranslateCharacter(textCharacter, out failedAccent);
			_activeTranslation = TranslationType.None;
		}

		data = new KeyEventData(rawData, textCharacter, scanCode, modifiers, isRight, isKeyPad, isEphemeral);

		if (failedAccent != default)
		{
			_future.Enqueue(data);
			data = new KeyEventData(rawData, failedAccent, scanCode, modifiers, isRight, isKeyPad, isEphemeral);
		}

		return true;
	}

	char TranslateCharacter(char ch, out char failedAccent)
	{
		failedAccent = default;

		const string Plain = "aeiou";
		const string Acute = "áéíóú";
		const string Grave = "àèìòù";
		const string Circu = "âêîôû";

		string target;

		switch (_activeTranslation)
		{
			default: return ch;

			case TranslationType.Acute: target = Acute; break;
			case TranslationType.Grave: target = Grave; break;
			case TranslationType.Circumflex: target = Circu; break;
		}

		int index = Plain.IndexOf(ch);

		if (index >= 0)
			return target[index];

		switch (_activeTranslation)
		{
			case TranslationType.Acute: failedAccent = '\''; break;
			case TranslationType.Grave: failedAccent = '`'; break;
			case TranslationType.Circumflex: failedAccent = '^'; break;
		}

		return ch;
	}

	enum TranslationType
	{
		None,

		Acute,
		Grave,
		Circumflex,
	}

	TranslationType _activeTranslation = TranslationType.None;

	bool UpdateState(RawKeyEventData data)
	{
		if (data.IsRelease)
			return false;

		var modifiers = data.Modifiers;

		bool eitherCtrl = (modifiers & SDL.Keymod.Ctrl) != 0;
		bool eitherAlt = (modifiers & SDL.Keymod.Alt) != 0;
		bool eitherShift = (modifiers & SDL.Keymod.Shift) != 0;

		if (eitherCtrl || eitherAlt)
			return false;

		switch (data.RawScanCode)
		{
			case SDL.Scancode.Equals:
			{
				if (eitherShift)
					_activeTranslation = TranslationType.Grave;
				else
					_activeTranslation = TranslationType.Acute;

				return true;
			}
			case SDL.Scancode.Grave:
			{
				_activeTranslation = TranslationType.Circumflex;
				return true;
			}
		}

		return false;
	}
}
