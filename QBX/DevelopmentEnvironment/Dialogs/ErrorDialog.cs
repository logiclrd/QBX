using QBX.DevelopmentEnvironment.Dialogs.Widgets;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class ErrorDialog : Dialog
{
	public ErrorDialog(Configuration configuration, string errorMessage)
		: base(configuration)
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
				Text = errorMessage,
			});

		Widgets.Add(
			new Button()
			{
				X = midX - 10,
				Y = 4,
				Width = 8,
				Text = "OK",
				Activated = OnClose
			});

		Widgets.Add(
			new Button()
			{
				X = midX + 1,
				Y = 4,
				Width = 8,
				Text = "Help",
				AcceleratorKeyIndex = 0,
			});

		SetFocus(Widgets[1]);
	}
}
