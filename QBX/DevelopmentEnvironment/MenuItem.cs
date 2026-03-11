using System;

namespace QBX.DevelopmentEnvironment;

public class MenuItem(string label, string? helpContextString = null)
{
	public string Label = label;
	public bool IsChecked;
	public Action? Clicked;
	public bool IsEnabled = true;
	public bool IsSeparator;
	public string? HelpContextString = helpContextString;

	public static MenuItem Separator => new MenuItem("---") { IsSeparator = true };
}
