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

	static CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

	public override bool ProcessKey(KeyEvent input, IFocusContext focusContext, IOvertypeFlag overtypeFlag)
	{
		if (input.IsRelease)
			return false;

		if (input.IsNormalText)
		{
			if (SelectionStart >= 0)
				DeleteSelection();

			SelectionStart = -1;

			if (overtypeFlag.Value && (CursorX < Text.Length))
				Text.SetCharacterAt(CursorX, input.TextCharacter);
			else
				Text.Insert(CursorX, input.TextCharacter);
			CursorX++;
		}
		else
		{
			int newSelectionStart = SelectionStart;

			if (!input.Modifiers.ShiftKey)
				newSelectionStart = -1;
			else if (SelectionStart < 0)
				newSelectionStart = CursorX;

			input = input.NormalizeModifierCombinationKey();

			try
			{
				switch (input.ScanCode)
				{
					case ScanCode.Left:
						if (input.Modifiers.CtrlKey)
							WordLeft();
						else
							CursorX--;
						break;
					case ScanCode.Right:
						if (input.Modifiers.CtrlKey)
							WordRight();
						else
							CursorX++;
						break;
					case ScanCode.Home:
						if (!input.Modifiers.CtrlKey)
							CursorX = 0;
						break;
					case ScanCode.End:
						if (!input.Modifiers.CtrlKey)
							CursorX = Text.Length;
						break;

					case ScanCode.Delete:
						if (!input.Modifiers.AltKey)
						{
							if (SelectionStart >= 0)
								DeleteSelection();
							else if (CursorX < Text.Length)
								Text.Remove(CursorX, 1);
						}
						break;
					case ScanCode.Backspace:
						if (!input.Modifiers.CtrlKey)
						{
							if (CursorX > 0)
							{
								CursorX--;
								Text.Remove(CursorX, 1);
							}
						}
						break;
					case ScanCode.Insert:
						overtypeFlag.Toggle();
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
		}

		return true;
	}

	bool IsWordCharacter(byte ch)
		=> (ch == (byte)'.') || s_cp437.IsAsciiLetterOrDigit(ch);

	void WordLeft()
	{
		if (CursorX > 0)
		{
			CursorX--;

			while ((CursorX > 0) && IsWordCharacter(Text[CursorX - 1]))
				CursorX--;
		}
	}

	void WordRight()
	{
		if (CursorX < Text.Length)
		{
			CursorX++;

			while ((CursorX < Text.Length) && IsWordCharacter(Text[CursorX]))
				CursorX++;
			while ((CursorX < Text.Length) && !IsWordCharacter(Text[CursorX]))
				CursorX++;
		}
	}

	public void SelectAll()
	{
		SelectionStart = 0;
		CursorX = Text.Length;
	}

	void DeleteSelection()
	{
		if ((SelectionStart >= 0) && (CursorX != SelectionStart))
		{
			int selectionStartIndex = Math.Min(CursorX, SelectionStart);
			int selectionLength = Math.Max(CursorX, SelectionStart) - selectionStartIndex;

			Text.Remove(selectionStartIndex, selectionLength);
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
