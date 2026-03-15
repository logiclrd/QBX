using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class FilePreviouslyLoadedDialog : Dialog
{
	const string BaseMessage = "File previously loaded";

	public FilePreviouslyLoadedDialog(Machine machine, Configuration configuration, string filePath)
		: base(machine, configuration)
	{
		Width = BaseMessage.Length + 8;
		Height = 8;
		HelpContextString = "-204";

		int midX = Width / 2;

		Widgets.Add(
			new Label()
			{
				X = 3,
				Y = 1,
				Width = Width - 5,
				Text = BaseMessage,
			});

		Widgets.Add(
			new Label()
			{
				X = Width / 2 - filePath.Length / 2,
				Y = 2,
				Text = filePath,
			}.AutoSize());

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
