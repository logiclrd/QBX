using System;
using System.Collections.Generic;
using System.Linq;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public class HorizontalListBox : Widget
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

	public HorizontalListBox()
	{
		IsTabStop = true;
	}

	int[] _columnWidths = Array.Empty<int>();
	int[] _columnOffsets = Array.Empty<int>();
	int _maxScrollColumnIndex;

	int _selectedIndex;
	int _scrollColumnIndex;

	public void Clear()
	{
		Items.Clear();
		_selectedIndex = -1;
		_scrollColumnIndex = 0;
		SelectionChanged?.Invoke();
	}

	[ThreadStatic]
	static char[]? s_charBuffer;

	public override bool ProcessKey(KeyEvent input, IOvertypeFlag overtypeFlag)
	{
		int innerWidth = Width - 2;
		int innerHeight = Height - 2;

		int newSelectedIndex = Math.Max(0, _selectedIndex);

		switch (input.ScanCode)
		{
			case ScanCode.Home: newSelectedIndex = 0; break;
			case ScanCode.End: newSelectedIndex = Items.Count - 1; break;

			case ScanCode.Left: newSelectedIndex -= innerHeight; break;
			case ScanCode.Right: newSelectedIndex += innerHeight; break;

			case ScanCode.Up: newSelectedIndex = _selectedIndex - 1; break;
			case ScanCode.Down: newSelectedIndex = _selectedIndex + 1; break;

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

		if (newSelectedIndex >= Items.Count)
			newSelectedIndex = Items.Count - 1;
		if (newSelectedIndex < 0)
			newSelectedIndex = 0;

		if (newSelectedIndex != _selectedIndex)
		{
			_selectedIndex = newSelectedIndex;

			SelectionChanged?.Invoke();

			int selectionColumn = _selectedIndex / innerHeight;

			int columnLeft = _columnOffsets[selectionColumn] - _columnOffsets[_scrollColumnIndex];
			int columnRight = columnLeft + _columnWidths[selectionColumn] - 1;

			while (columnRight >= innerWidth)
			{
				if (_scrollColumnIndex + 1 >= _columnWidths.Length)
					break;

				columnLeft -= _columnWidths[_scrollColumnIndex];
				columnRight = columnLeft + _columnWidths[selectionColumn] - 1;

				_scrollColumnIndex++;
			}

			while (columnLeft < 0)
			{
				_scrollColumnIndex--;

				columnLeft += _columnWidths[_scrollColumnIndex];
			}
		}

		return true;
	}

	public override void PlaceCursorForFocus(TextLibrary visual, IntegerRect bounds)
	{
		int columnHeight = Height - 2;

		int innerX1 = bounds.X1 + X + 1;
		int innerY1 = bounds.Y1 + Y + 1;

		int selectionIndex = Math.Max(0, _selectedIndex);

		int selectionColumn = selectionIndex / columnHeight;
		int selectionRow = selectionIndex % columnHeight;

		int scrollOffset = (_scrollColumnIndex >= _columnOffsets.Length) ? 0 : _columnOffsets[_scrollColumnIndex];
		int selectionOffset = (selectionColumn >= _columnOffsets.Length) ? 0 : _columnOffsets[selectionColumn];

		int selectionX = innerX1 + selectionOffset - scrollOffset;
		int selectionY = innerY1 + selectionRow;

		using (visual.PushClipRect(bounds))
			visual.MoveCursorWithinClip(selectionX + 1, selectionY);
	}

	public void RecalculateColumns()
	{
		int innerWidth = Width - 2;
		int columnHeight = Height - 2;

		int numColumns = (Items.Count + columnHeight - 1) / columnHeight;

		_columnWidths = new int[numColumns];
		_columnOffsets = new int[numColumns];

		for (int columnIndex = 0, i = 0; columnIndex < numColumns; columnIndex++, i += columnHeight)
		{
			_columnWidths[columnIndex] = Items.Skip(i).Take(columnHeight).Max(item => item.Label.Length) + 2;

			if (_columnWidths[columnIndex] < 14)
				_columnWidths[columnIndex] = 14;

			if (columnIndex > 0)
				_columnOffsets[columnIndex] = _columnOffsets[columnIndex - 1] + _columnWidths[columnIndex - 1];
		}

		int scrolledRightWidth = 0;

		_maxScrollColumnIndex = _columnWidths.Length;

		while ((_maxScrollColumnIndex > 0)
		    && (scrolledRightWidth + _columnWidths[_maxScrollColumnIndex - 1] <= innerWidth))
		{
			_maxScrollColumnIndex--;
			scrolledRightWidth += _columnWidths[_maxScrollColumnIndex];
		}
	}

	public override void Render(TextLibrary visual, IntegerRect bounds, Configuration configuration)
	{
		int x = X + bounds.X1;
		int y = Y + bounds.Y1;

		DialogPaint.DrawScrollableBox(
			x, y, Width, Height,
			title: "",
			configuration, visual,
			horizontalScrollValue: _scrollColumnIndex,
			horizontalScrollMax: _maxScrollColumnIndex + 1);

		int innerX1 = x + 1;
		int innerY1 = y + 1;
		int innerX2 = innerX1 + Width - 3;
		int innerY2 = innerY1 + Height - 3;

		int columnHeight = Height - 2;
		int scrollOffsetX = _scrollColumnIndex >= _columnOffsets.Length ? 0 : -_columnOffsets[_scrollColumnIndex];

		using (visual.PushClipRect(bounds))
		using (visual.PushClipRect(innerX1, innerY1, innerX2, innerY2))
		{
			for (int i = _scrollColumnIndex * columnHeight, columnIndex = _scrollColumnIndex; i < Items.Count; i += columnHeight, columnIndex++)
			{
				int columnX = _columnOffsets[columnIndex] + scrollOffsetX;
				int w = _columnWidths[columnIndex];

				if (columnX > innerX2)
					break;
				if (columnX + w > innerX2)
					w = innerX2 - columnX;

				columnX += innerX1;

				for (int idx = i, itemY = innerY1; itemY <= innerY2; idx++, itemY++)
				{
					string label = ((idx >= 0) && (idx < Items.Count)) ? Items[idx].Label : "";

					int padRight = _columnWidths[columnIndex] - label.Length;

					visual.MoveCursor(columnX, itemY);
					visual.WriteText(' ');
					visual.WriteText(label);
					DialogPaint.WriteSpaces(padRight, visual);
				}
			}

			if ((_selectedIndex >= 0) && (_selectedIndex < Items.Count))
			{
				int selectionColumn = _selectedIndex / columnHeight;
				int selectionRow = _selectedIndex % columnHeight;

				int selectionX = innerX1 + _columnOffsets[selectionColumn] + scrollOffsetX;
				int selectionY = innerY1 + selectionRow;
				int selectionWidth = _columnWidths[selectionColumn] + 1;

				configuration.DisplayAttributes.DialogBoxNormalText.SetInverted(visual);
				visual.WriteAttributesAt(selectionX, selectionY, selectionWidth);
				configuration.DisplayAttributes.DialogBoxNormalText.Set(visual);
			}
		}
	}
}
