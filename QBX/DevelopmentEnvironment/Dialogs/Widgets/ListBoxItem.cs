using System;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public class ListBoxItem : IComparable<ListBoxItem>
{
	public string Label = "";
	public string Value = "";

	public ListBoxItem(string value)
	{
		Label = value;
		Value = value;
	}

	public ListBoxItem(string label, string value)
	{
		Label = label;
		Value = value;
	}

	public int CompareTo(ListBoxItem? other)
	{
		if (other == null)
			return -1;

		return Label.CompareTo(other.Label);
	}
}
