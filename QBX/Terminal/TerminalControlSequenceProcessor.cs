using System;
using System.Collections.Generic;
using System.Text;

using QBX.ExecutionEngine.Execution;
using QBX.Firmware.Fonts;

namespace QBX.Terminal;

public class TerminalControlSequenceProcessor(TerminalEmulator terminal, TerminalInput input)
{
	enum State
	{
		Normal,
		HaveOriginatorEscape,
		IgnoreCharacter,

		InCharacterSetSequence, // ESC % x

		AtStartOfVTControlSequence, // allow ESC [ a X or ESC [ ? a X
		InVTControlSequence, // ESC [ a ; b X

		InDesignateG0CharacterSetSequence, // ESC ( x
		InDesignateG1CharacterSetSequence, // ESC ) x

		InOSCommandSequence, // ESC ] n ; <string> <ST>
		HaveOSCommandTerminatorEscape, // <ST> = ESC \ | BEL
	}

	State _state;
	bool _privateMode;
	List<string> _parameters = new List<string>();
	StringValue _parameterBuffer = new StringValue();

	void ResetState()
	{
		_state = State.Normal;
		_privateMode = false;
		_parameters.Clear();
		_parameterBuffer.Clear();
	}

	const byte ESC = 27;

	public void ProcessByte(byte b, Action<byte> emit)
	{
		switch (_state)
		{
			case State.IgnoreCharacter:
				ResetState();
				break;
			case State.Normal:
				if (b == ESC)
					_state = State.HaveOriginatorEscape;
				else
					emit(b);
				break;
			case State.HaveOriginatorEscape:
				switch (b)
				{
					case (byte)'%': _state = State.InCharacterSetSequence; break;
					case (byte)'[': _state = State.AtStartOfVTControlSequence; break;
					case (byte)'(': _state = State.InDesignateG0CharacterSetSequence; break;
					case (byte)')': _state = State.InDesignateG1CharacterSetSequence; break;
					case (byte)']': _state = State.InOSCommandSequence; break;

					case (byte)'c': // ESC c => Reset
					case (byte)'D': // ESC D => Line Feed
					case (byte)'E': // ESC E => Newline
					case (byte)'H': // ESC H => Horizontal Tab Set
					case (byte)'M': // ESC M => Reverse Index, a backwards newline basically
					case (byte)'7': // ESC 7 => Save Cursor Position
					case (byte)'8': // ESC 8 => Restore Cursor Position
					case (byte)'=': // ESC = => Enable Keypad Application Mode
					case (byte)'>': // ESC > => Enable Keypad Numeric Mode
						switch (b)
						{
							case (byte)'c': terminal.Reset(); break;
							case (byte)'D': terminal.LineFeed(); break;
							case (byte)'E': terminal.NewLine(); break;
							case (byte)'H': terminal.SetHorizontalTabStop(); break;
							case (byte)'M': terminal.ReverseLineFeed(); break;
							case (byte)'Z': input.InjectInput([ESC, (byte)'[', (byte)'?', (byte)'6', (byte)'c']); break;
							case (byte)'7': terminal.SaveState(); break;
							case (byte)'8': terminal.RestoreState(); break;
							case (byte)'=': input.SetNumLockState(false); break;
							case (byte)'>': input.SetNumLockState(true); break;
						}

						ResetState();
						break;

					default:
						emit(ESC);
						emit(b);

						ResetState();

						break;
				}

				break;

			case State.InCharacterSetSequence:
				switch (b)
				{
					case (byte)'@': terminal.InputDecoder = Encoding.Latin1.GetDecoder(); break;
					case (byte)'8':
					case (byte)'G': terminal.InputDecoder = Encoding.UTF8.GetDecoder(); break;
				}

				ResetState();

				break;

			case State.AtStartOfVTControlSequence:
				if (b == '[') // ESC [ [ -- ignore echoed function key
					_state = State.IgnoreCharacter;
				else
				{
					_state = State.InVTControlSequence;

					if ((b == '?') || (b == '<') || (b == '=') || (b == '>'))
						_privateMode = true;
					else
						goto case State.InVTControlSequence;
				}

				break;

			case State.InVTControlSequence:
				if ((b >= '0') && (b <= '9'))
					_parameterBuffer.Append(b);
				else
				{
					if (_parameterBuffer.Length == 0)
						_parameters.Add("0");
					else
					{
						_parameters.Add(_parameterBuffer.ToString());
						_parameterBuffer.Clear();
					}

					if (b != ';')
					{
						int.TryParse(_parameters[0], out var param);

						switch (b)
						{
							case (byte)'@': terminal.WriteBlanks(param); break;
							case (byte)'A': terminal.MoveCursor(dy: -param); break;
							case (byte)'B': terminal.MoveCursor(dy: +param); break;
							case (byte)'C': terminal.MoveCursor(dx: +param); break;
							case (byte)'D': terminal.MoveCursor(dx: -param); break;
							case (byte)'E': terminal.MoveCursor(x: 0, dy: +param); break;
							case (byte)'F': terminal.MoveCursor(x: 0, dy: -param); break;
							case (byte)'G': terminal.MoveCursor(x: param - 1); break;

							case (byte)'H':
							{
								int row = (param < 1) ? 1 : param;
								int column;

								if ((_parameters.Count < 2)
								 || !int.TryParse(_parameters[1], out column))
									column = 1;

								terminal.MoveCursor(x: column - 1, y: row - 1);

								break;
							}

							case (byte)'J':
							{
								switch (param)
								{
									case 0: terminal.ClearBelowCursor(); break;
									case 1: terminal.ClearAboveCursor(); break;
									default: terminal.Clear(); break;
								}

								break;
							}

							case (byte)'K':
							{
								switch (param)
								{
									case 0: terminal.ClearRightOfCursor(); break;
									case 1: terminal.ClearLeftOfCursor(); break;
									default: terminal.ClearCurrentLine(); break;
								}

								break;
							}

							case (byte)'L': terminal.InsertLines(param); break;
							case (byte)'M': terminal.DeleteLines(param); break;
							case (byte)'P': terminal.DeleteCharacters(param); break;
							case (byte)'X': terminal.ClearCharacters(param); break;

							case (byte)'a': terminal.MoveCursor(dx: param); break;
							case (byte)'c': input.InjectInput([ESC, (byte)'[', (byte)'?', (byte)'6', (byte)'c']); break;
							case (byte)'d': terminal.MoveCursor(y: param + 1); break;
							case (byte)'e': terminal.MoveCursor(dy: param); break;
							case (byte)'f': goto case (byte)'H';
							case (byte)'g': terminal.ClearHorizontalTabStop(); break;

							case (byte)'h':
								if (_privateMode) // ESC [ ?
								{
									switch (param)
									{
										case 2: // Designate USASCII for character sets G0-G3
											terminal.OutputEncodings[0] = Encoding.ASCII;
											terminal.OutputEncodings[1] = Encoding.ASCII;
											break;
										case 25: // Show cursor
											terminal.ShowCursor();
											break;
										case 67: // Backarrow key sends backspace
											input.BackarrowMode = true;
											break;
									}
								}

								break;

							case (byte)'l':
								if (_privateMode) // ESC [ ?
								{
									switch (param)
									{
										case 25: // Hide cursor
											terminal.HideCursor();
											break;
										case 67: // Backarrow key sends delete
											input.BackarrowMode = false;
											break;
									}
								}

								break;

							case (byte)'m': ConfigureAttribute(_parameters); break;
							case (byte)'n': /* TODO: status report */ break;
							case (byte)'q': /* set status LEDs -- not supported */ break;
							case (byte)'r':
							{
								int topRow = param;
								int bottomRow;

								if ((_parameters.Count < 2)
								 || !int.TryParse(_parameters[1], out bottomRow))
									bottomRow = topRow;

								terminal.SetCharacterLineWindow(topRow - 1, bottomRow - 1);

								break;
							}

							case (byte)'s': terminal.SaveState(); break;
							case (byte)'u': terminal.RestoreState(); break;
							case (byte)'`': terminal.MoveCursor(x: param - 1); break;
						}

						ResetState();
					}
				}

				break;

			case State.InDesignateG0CharacterSetSequence:
			case State.InDesignateG1CharacterSetSequence:
				int index =
					_state switch
					{
						State.InDesignateG0CharacterSetSequence => 0,
						State.InDesignateG1CharacterSetSequence => 1,
						_ => throw new Exception("Sanity failure")
					};

				switch (b)
				{
					case (byte)'B': terminal.OutputEncodings[index] = Encoding.Latin1; break;
					case (byte)'0': terminal.OutputEncodings[index] = new DECGraphicsEncoding(); break;
					case (byte)'U': terminal.OutputEncodings[index] = new CP437Encoding(ControlCharacterInterpretation.Graphic); break;
					case (byte)'K': goto case (byte)'U'; // "user mapping" loaded by mapscrn
				}

				ResetState();
				break;

			case State.InOSCommandSequence:
				if (b == ESC)
					_state = State.HaveOSCommandTerminatorEscape;
				else if (b == 7)
					ResetState();
				break;
			case State.HaveOSCommandTerminatorEscape:
				if (b == (byte)'\\')
					ResetState();
				else if (b != ESC)
					_state = State.InOSCommandSequence;
				break;
		}
	}

	public void ConfigureAttribute(IEnumerable<string> commands)
	{
		var config = terminal.Attribute;

		foreach (var commandString in commands)
			if (int.TryParse(commandString, out int command))
				ConfigureAttribute(command, config);

		config.Commit();
	}

	void ConfigureAttribute(int command, TerminalAttribute config)
	{
		switch (command)
		{
			case 0: config.Reset(); break;
			case 1: config.SetBold(); break;
			case 2: config.SetHalfBright(); break;
			case 3: config.SetItalic(); break;
			case 4: config.SetUnderline(); break;
			case 5: config.SetBlink(); break;
			case 7: config.SetReverse(); break;
			case 21: config.SetUnderline(); break;
			case 22: config.SetRegularIntensity(); break;
			case 23: config.ClearItalic(); break;
			case 24: config.ClearUnderline(); break;
			case 25: config.ClearBlink(); break;
			case 27: config.ClearReverse(); break;

			case 30: case 31: case 32: case 33: case 34: case 35: case 36: case 37:
				config.SetForeground(TranslateColour(command));
				break;
			case 90: case 91: case 92: case 93: case 94: case 95: case 96: case 97:
				config.SetBold();
				goto case 30;
			case 100: case 101: case 102: case 103: case 104: case 105: case 106: case 107:
			case 40: case 41: case 42: case 43: case 44: case 45: case 46: case 47:
				config.SetBackground(TranslateColour(command));
				break;
		}
	}

	int TranslateColour(int colour)
	{
		colour %= 10;

		int r = (colour & 1) >> 0;
		int g = (colour & 2) >> 1;
		int b = (colour & 4) >> 2;

		return (r << 2) | (g << 1) | (b << 0);
	}
}
