using System;
using System.Diagnostics.CodeAnalysis;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public static class ListBoxItem
{
	public static ListBoxItem<TValue> Create<TValue>(string label, TValue value)
		where TValue : notnull
		=> new ListBoxItem<TValue>(label, value);
}

public class ListBoxItem<TValue> : IComparable<ListBoxItem<TValue>>
	where TValue : notnull
{
	public string Label = "";

	public TValue Value
	{
		get => _value;
		[MemberNotNull(nameof(_value))]
		set => _value = value;
	}

	TValue _value;

	public ListBoxItem(string label, TValue value)
	{
		Label = label;
		Value = value;
	}

	public int CompareTo(ListBoxItem<TValue>? other)
	{
		if (other == null)
			return -1;

		return Label.CompareTo(other.Label);
	}
}
