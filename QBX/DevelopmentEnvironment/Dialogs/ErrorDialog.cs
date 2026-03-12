using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class ErrorDialog : Dialog
{
	public ErrorDialog(Machine machine, Configuration configuration, string errorMessage, int? errorNumber, ErrorSource source)
		: base(machine, configuration)
	{
		if (errorNumber.HasValue)
		{
			int errorNumberForSource = errorNumber.Value;

			if (source == ErrorSource.DevelopmentEnvironment)
				errorNumberForSource += 2000;

			HelpContextString = (-errorNumberForSource).ToString();
		}
		else
			HelpContextString = "-121"; // Syntax Error

		if (errorMessage.Length > 65)
			errorMessage = errorMessage.Substring(0, 65) + "...";

		Width = errorMessage.Length + 8;
		Height = 7;

		if (Width < 25)
			Width = 25;

		int midX = Width / 2;

		Widgets.Add(
			new Label()
			{
				X = 3,
				Y = 1,
				Width = Width - 5,
				Text = errorMessage,
			});

		Widgets.Add(
			new Button()
			{
				X = midX - 10,
				Y = 4,
				Width = 8,
				Text = "OK",
				Activated = cmdOK_Activated
			});

		Widgets.Add(
			new Button()
			{
				X = midX + 1,
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
	}

	void cmdHelp_Activated()
	{
		OnShowHelpPopup();
	}
}
