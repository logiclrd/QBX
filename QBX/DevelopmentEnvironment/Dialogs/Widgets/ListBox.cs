using System;
using System.Collections.Generic;
using System.Linq;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public abstract class ListBox<TSelf, TValue> : Widget
	where TSelf : ListBox<TSelf, TValue>
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

	public bool ShowSelectionWhenFocused = true;
	public bool ShowSelectionWhenUnfocused = false;

	public bool ShowScrollBar = true;

	int _selectedIndex;
	int _scrollPosition;

	public void Clear()
	{
		Items.Clear();
		_selectedIndex = -1;
		_scrollPosition = 0;
		SelectionChanged?.Invoke();
	}

	protected abstract TSelf GetSelf();

	public TSelf SelectItem(TValue item)
		=> SelectItem(Items.FindIndex(listBoxItem => listBoxItem.Value.Equals(item)));

	public TSelf SelectItem(int index)
	{
		EnsureVisible(index);

		SelectedIndex = index;
		SelectionChanged?.Invoke();

		return GetSelf();
	}

	public abstract TSelf EnsureVisible(int index);

	ListBoxItem<TValue> CreateItem(TValue value)
		=> new ListBoxItem<TValue>(value.ToString() ?? "", value);

	public TSelf AddItem(TValue item)
		=> AddItem(CreateItem(item));

	public TSelf AddItem(ListBoxItem<TValue> item)
	{
		Items.Add(item);
		return GetSelf();
	}

	public TSelf AddItems(IEnumerable<TValue> items)
		=> AddItems(items.Select(item => CreateItem(item)));

	public TSelf AddItems(IEnumerable<ListBoxItem<TValue>> items)
	{
		Items.AddRange(items);
		return GetSelf();
	}

}
