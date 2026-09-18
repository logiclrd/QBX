using System;

using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using QBX.Firmware.Fonts;
using QBX.Hardware;
using QBX.Utility;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public class TextInput : Widget
{
	// This widget's Height is always 1. It ignores the value of the Height property.

	public StringValue Text = new StringValue();
	public int ScrollX = 0;
	public int CursorX = 0;
	public int SelectionStart = -1;

	public TextInput()
	{
		IsTabStop = true;
	}

	enum TextInputAction
	{
		None,

		Ignore,
		InputCharacter,
		Backspace,
		Delete,
		CharLeft,
		CharRight,
		WordLeft,
		WordRight,
		BegLine,
		EndLine,
		DelWord,
		ToggleInsertMode,
		Beep,
		CutToEOL,
	}

	public override bool ProcessKey(KeyEvent input, IFocusContext focusContext, IOvertypeFlag overtypeFlag)
	{
		if (input.IsRelease)
			return false;

		var action = TextInputAction.None;

		byte inputByte = 0;

		var wasInChord = TextInputChordManager.ChordType;

		TextInputChordManager.ChordType = TextInputChordType.None;

		if (wasInChord == TextInputChordType.CtrlK)
		{
			// Text Input behaviour: Input after ^K is eaten, ^K 0 through ^K 3 do nothing
			action = TextInputAction.Ignore;
			wasInChord = TextInputChordType.None;
		}

		if (action == TextInputAction.None)
		{
			char? inputCharacter = '\0';

			if (wasInChord == TextInputChordType.CtrlP)
			{
				if (input.HasTextCharacter)
				{
					switch (input.TextCharacter)
					{
						case (char)10:
						case (char)13:
							action = TextInputAction.Beep;
							break;
						case (char)27:
							TextInputChordManager.ChordType = TextInputChordType.CtrlP;
							return false; // allow upstream handling

						default:
							action = TextInputAction.InputCharacter;
							inputCharacter = input.TextCharacter;
							break;
					}
				}
				else
				{
					switch (input.ScanCode)
					{
						case ScanCode.Return:
						case ScanCode.Delete:
							TextInputChordManager.ChordType = TextInputChordType.CtrlP;
							action = TextInputAction.Beep;
							break;

						case ScanCode.F12: inputCharacter = '{'; break;
						case ScanCode.Backspace: inputCharacter = '\x08'; break;
						case ScanCode.Tab: inputCharacter = '\x09'; break;
						case ScanCode.Insert: inputCharacter = '-'; break;
						case ScanCode.Home: inputCharacter = '$'; break;
						case ScanCode.PageUp: inputCharacter = '!'; break;
						case ScanCode.End: inputCharacter = '#'; break;
						case ScanCode.PageDown: inputCharacter = '"'; break;
						case ScanCode.Up: inputCharacter = '&'; break;
						case ScanCode.Down: inputCharacter = '('; break;
						case ScanCode.Left: inputCharacter = '%'; break;
						case ScanCode.Right: inputCharacter = '\''; break;

						case ScanCode.Kp5:
							if (input.Modifiers.NumLock)
								inputCharacter = '5';
							else
								inputCharacter = '\x0C';
							break;

						default:
							TextInputChordManager.ChordType = wasInChord;
							break;
					}
				}

				if (inputCharacter != '\0')
					action = TextInputAction.InputCharacter;
			}
			else if (wasInChord == TextInputChordType.CtrlQ)
			{
				if (input.HasTextCharacter)
				{
					switch (input.TextCharacter)
					{
						case (char)('D' - 64): action = TextInputAction.EndLine; break;
						case (char)('S' - 64): action = TextInputAction.BegLine; break;
						case (char)('Y' - 64): action = TextInputAction.CutToEOL; break;

						// BookMark actions meaningless here
					}
				}
			}
			else if (input.Modifiers.CtrlKey && (input.ScanCode == ScanCode.Backspace))
				action = TextInputAction.Delete;
			else
			{
				inputByte = CP437Encoding.GetByteSemantic(input.TextCharacter);

				switch (inputByte)
				{
					case 0: break; // NUL does nothing
					case 1: action = TextInputAction.BegLine; break;
					case 2: action = TextInputAction.Beep; break;
					case 3: break; // ^C does nothing here
					case 4: action = TextInputAction.CharRight; break;
					case 5: break; // Cursor up does nothing here
					case 6: action = TextInputAction.EndLine; break;
					case 7: action = TextInputAction.Delete; break;
					case 8: action = TextInputAction.Backspace; break;
					case 9: break; // Tab handled upstream
					case 10: action = TextInputAction.BegLine; break; // Next Line
					case 11: TextInputChordManager.ChordType = TextInputChordType.CtrlK; break;
					case 12: break; // Find Next does nothing here
					case 13: break; // Return handled upstream
					case 14: break; // Split Line meaningless here
					case 15: action = TextInputAction.Beep; break;
					case 16: TextInputChordManager.ChordType = TextInputChordType.CtrlP; break;
					case 17: TextInputChordManager.ChordType = TextInputChordType.CtrlQ; break;
					case 18: break; // Page Up meaningless here
					case 19: action = TextInputAction.CharLeft; break;
					case 20: action = TextInputAction.DelWord; break;
					case 21: break; // ^U doesn't seem to mean anything
					case 22: action = TextInputAction.ToggleInsertMode; break;
					case 23: break; // Scroll Up meaningless here
					case 24: break; // Line Down meaningless here
					case 25: break; // Cut Current does nothing here
					case 26: break; // Scroll Down meaningless here
					case 127: action = TextInputAction.Delete; break;

					case 27: case 28: case 29: case 30: case 31: break;

					default: if (input.IsNormalText) action = TextInputAction.InputCharacter; break;
				}
			}
		}

		if (action == TextInputAction.None)
		{
			input = input.NormalizeModifierCombinationKey();

			switch (input.ScanCode)
			{
				case ScanCode.Left: action = input.Modifiers.CtrlKey ? TextInputAction.WordLeft : TextInputAction.CharLeft; break;
				case ScanCode.Right: action = input.Modifiers.CtrlKey ? TextInputAction.WordRight : TextInputAction.CharRight; break;
				case ScanCode.Home: if (!input.Modifiers.CtrlKey) action = TextInputAction.BegLine; break;
				case ScanCode.End: if (!input.Modifiers.CtrlKey) action = TextInputAction.EndLine; break;
				case ScanCode.Delete: if (!input.Modifiers.AltKey) action = TextInputAction.Delete; break;
				case ScanCode.Backspace: action = input.Modifiers.CtrlKey ? TextInputAction.Delete : action = TextInputAction.Backspace; break;
				case ScanCode.Insert: action = TextInputAction.ToggleInsertMode; break;
			}
		}

		int newSelectionStart = SelectionStart;

		if (!input.IsModifierKey)
		{
			if (!input.Modifiers.ShiftKey)
				newSelectionStart = -1;
			else if (SelectionStart < 0)
				newSelectionStart = CursorX;
		}

		try
		{
			switch (action)
			{
				case TextInputAction.CharLeft: CursorX--; break;
				case TextInputAction.CharRight: CursorX++; break;
				case TextInputAction.WordLeft: WordLeft(); break;
				case TextInputAction.WordRight: WordRight(); break;
				case TextInputAction.BegLine: CursorX = 0; break;
				case TextInputAction.EndLine: CursorX = Text.Length; break;

				case TextInputAction.Delete:
					if (SelectionStart >= 0)
						DeleteSelection();
					else if (CursorX < Text.Length)
						Text.Remove(CursorX, 1);
					else
						Beep?.Invoke();

					break;

				case TextInputAction.Backspace:
					newSelectionStart = -1;

					if (CursorX > 0)
					{
						CursorX--;
						Text.Remove(CursorX, 1);
					}

					break;

				case TextInputAction.DelWord:
					// Default to delete a single character (symbol)
					int removeEnd = CursorX + 1;

					if (Text[CursorX] == ' ')
					{
						// Delete contiguous spaces
						while ((removeEnd < Text.Length) && (Text[removeEnd] == ' '))
							removeEnd++;
					}
					else if (Text[CursorX].IsWordCharacter())
					{
						// Delete contiguous word characters
						while ((removeEnd < Text.Length) && Text[removeEnd].IsWordCharacter())
							removeEnd++;
					}

					int removeCount = removeEnd - CursorX;

					Text.Remove(CursorX, removeCount);

					break;

				case TextInputAction.ToggleInsertMode:
					overtypeFlag.Toggle();
					break;

				case TextInputAction.Beep: Beep?.Invoke(); break;
				case TextInputAction.CutToEOL:
					try
					{
						SetClipboard?.Invoke(GetSelection().ToString());
						DeleteSelection();
					}
					catch { }

					break;

				case TextInputAction.InputCharacter:
					if (SelectionStart >= 0)
						DeleteSelection();

					newSelectionStart = -1;

					if (overtypeFlag.Value && (CursorX < Text.Length))
						Text.SetCharacterAt(CursorX, input.TextCharacter);
					else
						Text.Insert(CursorX, input.TextCharacter);
					CursorX++;

					break;

				default:
					return false;
			}
		}
		finally
		{
			if (CursorX < 0)
				CursorX = 0;
			if (CursorX > Text.Length)
				CursorX = Text.Length;

			if (CursorX >= ScrollX + Width)
				ScrollX = CursorX - Width + 1;
			if (CursorX < ScrollX)
				ScrollX = CursorX;

			SelectionStart = newSelectionStart;
		}

		return true;
	}

	void WordLeft()
	{
		if (CursorX > 0)
		{
			CursorX--;

			while ((CursorX > 0) && Text[CursorX - 1].IsWordCharacter())
				CursorX--;
		}
	}

	void WordRight()
	{
		if (CursorX < Text.Length)
		{
			CursorX++;

			while ((CursorX < Text.Length) && Text[CursorX].IsWordCharacter())
				CursorX++;
			while ((CursorX < Text.Length) && !Text[CursorX].IsWordCharacter())
				CursorX++;
		}
	}

	public void SelectAll()
	{
		SelectionStart = 0;
		CursorX = Text.Length;
	}

	StringValue GetSelection()
	{
		int selectionStartIndex = Math.Min(CursorX, SelectionStart);
		int selectionLength = Math.Max(CursorX, SelectionStart) - selectionStartIndex;

		return Text.Substring(selectionStartIndex, selectionLength);
	}

	void DeleteSelection()
	{
		if ((SelectionStart >= 0) && (CursorX != SelectionStart))
		{
			int selectionStartIndex = Math.Min(CursorX, SelectionStart);
			int selectionLength = Math.Max(CursorX, SelectionStart) - selectionStartIndex;

			if ((selectionStartIndex >= 0) && (selectionStartIndex < Text.Length))
			{
				if (selectionStartIndex + selectionLength > Text.Length)
					selectionLength = Text.Length - selectionStartIndex;

				Text.Remove(selectionStartIndex, selectionLength);
			}

			CursorX = selectionStartIndex;

			SelectionStart = -1;
		}
	}

	public override void PlaceCursorForFocus(TextLibrary visual, IntegerRect bounds)
	{
		using (visual.PushClipRect(bounds))
			visual.MoveCursor(bounds.X1 + X + CursorX - ScrollX, bounds.Y1 + Y);
	}

	public override void Render(TextLibrary visual, IntegerRect bounds, Configuration configuration)
	{
		visual.MoveCursor(bounds.X1 + X, bounds.Y1 + Y);

		if (ScrollX >= Text.Length)
		{
			DialogPaint.WriteSpaces(Width, visual);
			return;
		}

		var chars = Text.AsSpan().Slice(ScrollX);

		int selectionStartIndex = Math.Min(CursorX, SelectionStart);
		int selectionLength = Math.Max(CursorX, SelectionStart) - selectionStartIndex;

		int selectionStartOffset = selectionStartIndex - ScrollX;
		int selectionEndOffset = selectionStartOffset + selectionLength - 1;

		void WriteSubstring(ReadOnlySpan<byte> chars, int fromIndex, int toIndex)
		{
			int availableChars = chars.Length - fromIndex;
			int neededChars = toIndex - fromIndex + 1;

			if (availableChars >= neededChars)
				visual.WriteText(chars.Slice(fromIndex, neededChars));
			else if (availableChars <= 0)
				DialogPaint.WriteSpaces(neededChars, visual);
			else
			{
				visual.WriteText(chars.Slice(fromIndex));
				DialogPaint.WriteSpaces(neededChars - availableChars, visual);
			}
		}

		if (!IsFocused || (SelectionStart < 0) || (selectionStartOffset >= Width) || (selectionEndOffset <= 0))
			WriteSubstring(chars, 0, Width - 1);
		else
		{
			WriteSubstring(chars, 0, selectionStartOffset - 1);

			configuration.DisplayAttributes.DialogBoxNormalText.SetInverted(visual);
			WriteSubstring(chars, selectionStartOffset, selectionEndOffset);
			configuration.DisplayAttributes.DialogBoxNormalText.Set(visual);

			WriteSubstring(chars, selectionEndOffset + 1, Width - 1);
		}
	}
}
