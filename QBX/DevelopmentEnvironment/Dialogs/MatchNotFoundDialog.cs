using System;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class MatchNotFoundDialog : Dialog
{
	public MatchNotFoundDialog(Machine machine, Configuration configuration)
		: base(machine, configuration)
	{
		HelpContextString = "-254";

		Width = 25;
		Height = 7;

		Widgets.Add(
			new Label()
			{
				X = 4,
				Y = 1,
				Text = "Match not found",
			}.AutoSize());

		Widgets.Add(
			new Button()
			{
				X = 2,
				Y = 4,
				Text = "OK",
				Width = 8,
				Activated = () => { Close(); },
			});

		Widgets.Add(
			new Button()
			{
				X = 13,
				Y = 4,
				Text = "Help",
				Width = 8,
				AccessKeyIndex = 0,
				Activated = OnShowHelpPopup,
			});

		SetFocus(Widgets[1]);
	}
}
