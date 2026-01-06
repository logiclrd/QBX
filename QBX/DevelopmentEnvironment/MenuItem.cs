using System;

namespace QBX.DevelopmentEnvironment;

public class MenuItem(string label)
{
	public string Label = label;
	public bool IsChecked;
	public Action? Clicked;
	public bool IsEnabled = true;
	public bool IsSeparator;

	public static MenuItem Separator => new MenuItem("---") { IsSeparator = true };
}
