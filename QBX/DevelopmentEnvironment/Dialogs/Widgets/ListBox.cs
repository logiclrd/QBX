using System;
using System.Collections.Generic;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public abstract class ListBox<TValue> : Widget
	where TValue : notnull
{
	public readonly List<ListBoxItem<TValue>> Items = new List<ListBoxItem<TValue>>();

	public int SelectedIndex
	{
		get => _selectedIndex;
		protected set => _selectedIndex = value;
	}

	public int ScrollPosition
	{
		get => _scrollPosition;
		set => _scrollPosition = value;
	}

	public Action? SelectionChanged;

	public TValue SelectedValue
	{
		get
		{
			if ((_selectedIndex < 0) || (_selectedIndex >= Items.Count))
				throw new InvalidOperationException();

			return Items[_selectedIndex].Value;
		}
	}

	public ListBox()
	{
		IsTabStop = true;
	}

	public bool ShowSelectionWhenUnfocused = false;

	int _selectedIndex;
	int _scrollPosition;

	public void Clear()
	{
		Items.Clear();
		_selectedIndex = -1;
		_scrollPosition = 0;
		SelectionChanged?.Invoke();
	}

	public void SelectItem(TValue item)
		=> SelectItem(Items.FindIndex(listBoxItem => listBoxItem.Value.Equals(item)));

	public void SelectItem(int index)
	{
		EnsureVisible(index);

		SelectedIndex = index;
		SelectionChanged?.Invoke();
	}

	public abstract void EnsureVisible(int index);
}
