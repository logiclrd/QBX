using System;

namespace QBX.DevelopmentEnvironment;

public class ReferenceBarAction
{
	public string Label = "";
	public Action? Clicked;

	public static implicit operator ReferenceBarAction(string label)
	{
		return
			new ReferenceBarAction()
			{
				Label = label,
			};
	}

	public static implicit operator ReferenceBarAction((string Label, Action Clicked) tuple)
	{
		return
			new ReferenceBarAction()
			{
				Label = tuple.Label,
				Clicked = tuple.Clicked,
			};
	}
}
