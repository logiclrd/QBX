using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class ErrorDialog : Dialog
{
	public ErrorDialog(Machine machine, Configuration configuration, string errorMessage)
		: base(machine, configuration)
	{
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
				Activated = Close
			});

		Widgets.Add(
			new Button()
			{
				X = midX + 1,
				Y = 4,
				Width = 8,
				Text = "Help",
				AccessKeyIndex = 0,
			});

		SetFocus(Widgets[1]);
	}
}
