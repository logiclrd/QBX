using System;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class CannotContinueDialog : Dialog
{
	const string Message = "You will have to restart your program after this edit. Proceed anyway?";

	public event Action? Proceed;

	public CannotContinueDialog(Machine machine, Configuration configuration)
		: base(machine, configuration)
	{
		Width = 76;
		Height = 7;
		HelpContextString = "-198";

		Widgets.Add(
			new Label()
			{
				X = 2,
				Y = 0,
				Width = Width - 5,
				Text = Message,
			});

		Widgets.Add(
			new Button()
			{
				X = 23,
				Y = 4,
				Width = 8,
				Text = "OK",
				Activated = cmdOK_Activated
			});

		Widgets.Add(
			new Button()
			{
				X = 33,
				Y = 4,
				Width = 8,
				Text = "Cancel",
				Activated = cmdCancel_Activated
			});

		Widgets.Add(
			new Button()
			{
				X = 44,
				Y = 4,
				Width = 8,
				Text = "Help",
				AccessKeyIndex = 0,
				Activated = cmdHelp_Activated
			});

		SetFocus(Widgets[1]);
	}

	void cmdOK_Activated()
	{
		Close();
		Proceed?.Invoke();
	}

	void cmdCancel_Activated()
	{
		Close();
	}

	void cmdHelp_Activated()
	{
		OnShowHelpPopup();
	}
}
