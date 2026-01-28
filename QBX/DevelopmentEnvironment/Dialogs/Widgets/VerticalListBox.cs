using System;
using System.Collections.Generic;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public class VerticalListBox : Widget
{
	public readonly List<ListBoxItem> Items = new List<ListBoxItem>();

	public int SelectedIndex => _selectedIndex;

	public Action? SelectionChanged;

	public string SelectedValue
	{
		get
		{
			if ((_selectedIndex < 0) || (_selectedIndex >= Items.Count))
				throw new InvalidOperationException();

			return Items[_selectedIndex].Value;
		}
	}

	public VerticalListBox()
	{
		IsTabStop = true;
	}

	int _selectedIndex;
	int _scrollTop;

	public void Clear()
	{
		Items.Clear();
		_selectedIndex = -1;
		_scrollTop = 0;
		SelectionChanged?.Invoke();
	}

	[ThreadStatic]
	static char[]? s_charBuffer;

	public override bool ProcessKey(KeyEvent input, IOvertypeFlag overtypeFlag)
	{
		int innerHeight = Height - 2;

		int newSelectedIndex = _selectedIndex;

		switch (input.ScanCode)
		{
			case ScanCode.Home:
				newSelectedIndex = 0;
				break;
			case ScanCode.Left:
			case ScanCode.Up:
				newSelectedIndex--;
				break;
			case ScanCode.Right:
			case ScanCode.Down:
				newSelectedIndex++;
				break;
			case ScanCode.End:
				newSelectedIndex = Items.Count - 1;
				break;

			case ScanCode.PageUp:
				_scrollTop -= innerHeight;
				if (_scrollTop < 0)
					_scrollTop = 0;
				if (newSelectedIndex >= _scrollTop + innerHeight)
					newSelectedIndex = _scrollTop + innerHeight - 1;
				break;
			case ScanCode.PageDown:
				_scrollTop += innerHeight;
				if (_scrollTop > Items.Count - innerHeight)
					_scrollTop = Items.Count - innerHeight;
				if (newSelectedIndex < _scrollTop)
					newSelectedIndex = _scrollTop;
				break;

			default:
				if (input.IsNormalText)
				{
					s_charBuffer ??= new char[1];
					s_charBuffer[0] = input.TextCharacter;

					for (int i = 1; i < Items.Count; i++)
					{
						int idx = (newSelectedIndex + i) % Items.Count;

						if (Items[idx].Label.StartsWith(s_charBuffer, StringComparison.OrdinalIgnoreCase))
						{
							newSelectedIndex = idx;
							break;
						}
					}
				}

				break;
		}

		if (newSelectedIndex < 0)
			newSelectedIndex = 0;
		if (newSelectedIndex >= Items.Count)
			newSelectedIndex = Items.Count - 1;

		if (newSelectedIndex != _selectedIndex)
		{
			_selectedIndex = newSelectedIndex;

			SelectionChanged?.Invoke();

			if (_selectedIndex >= 0)
			{
				if (_selectedIndex < _scrollTop)
					_scrollTop = _selectedIndex;

				if (_selectedIndex >= _scrollTop + innerHeight)
					_scrollTop = _selectedIndex - innerHeight + 1;
			}
		}

		return true;
	}

	public override void PlaceCursorForFocus(TextLibrary visual, IntegerRect bounds)
	{
		int innerX1 = bounds.X1 + X + 1;
		int innerY1 = bounds.Y1 + Y + 1;
		int innerX2 = innerX1 + Width - 3;
		int innerY2 = innerY1 + Height - 3;

		using (visual.PushClipRect(bounds))
		using (visual.PushClipRect(innerX1, innerY1, innerX2, innerY2))
			visual.MoveCursor(innerX1 + 1, innerY1 + Math.Max(_selectedIndex, 0) - _scrollTop);
	}

	public override void Render(TextLibrary visual, IntegerRect bounds, Configuration configuration)
	{
		int x = X + bounds.X1;
		int y = Y + bounds.Y1;

		int innerX1 = x + 1;
		int innerY1 = y + 1;
		int innerX2 = innerX1 + Width - 3;
		int innerY2 = innerY1 + Height - 3;

		int innerWidth = innerX2 - innerX1 + 1;
		int innerHeight = innerY2 - innerY1 + 1;

		DialogPaint.DrawScrollableBox(
			x, y, Width, Height,
			title: "",
			configuration, visual,
			verticalScrollValue: _scrollTop,
			verticalScrollMax: Math.Max(1, Items.Count - innerHeight));

		using (visual.PushClipRect(bounds))
		using (visual.PushClipRect(innerX1, innerY1, innerX2, innerY2))
		{
			int availableChars = innerWidth - 1; // we always add a space on the left, but not on the right.

			for (int idx = _scrollTop, itemY = innerY1; itemY <= innerY2; idx++, itemY++)
			{
				bool highlight = (idx == _selectedIndex);

				string label = ((idx >= 0) && (idx < Items.Count)) ? Items[idx].Label : "";

				var labelChars = label.AsSpan();

				if (labelChars.Length > availableChars)
					labelChars = labelChars.Slice(0, availableChars);

				int padRight = availableChars - labelChars.Length;

				if (highlight)
					configuration.DisplayAttributes.DialogBoxNormalText.SetInverted(visual);

				visual.MoveCursor(innerX1, itemY);
				visual.WriteText(' ');
				visual.WriteText(labelChars);
				DialogPaint.WriteSpaces(padRight, visual);

				if (highlight)
					configuration.DisplayAttributes.DialogBoxNormalText.Set(visual);
			}
		}
	}
}
