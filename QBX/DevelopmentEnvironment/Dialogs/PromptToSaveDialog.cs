using System;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class PromptToSaveDialog : Dialog
{
	public event Action? Save;
	public event Action? DoNotSave;

	protected void OnSave() => Save?.Invoke();
	protected void OnDoNotSave() => DoNotSave?.Invoke();

	public PromptToSaveDialog(Configuration configuration)
		: base(configuration)
	{
		Width = 60;
		Height = 7;

		int midX = (Width - 2) / 2;

		Widgets.Add(
			new Label()
			{
				X = 2,
				Y = 1,
				Text = "One or more loaded files are not saved. Save them now?",
			}.AutoSize());

		Widgets.Add(
			new Button()
			{
				X = midX - 20,
				Y = 4,
				Text = "Yes",
				Width = 7,
				Activated = () => { Close(); OnSave(); }
			});

		Widgets.Add(
			new Button()
			{
				X = midX - 10,
				Y = 4,
				Text = "No",
				Width = 8,
				AccessKeyIndex = 0,
				Activated = () => { Close(); OnDoNotSave(); }
			});

		Widgets.Add(
			new Button()
			{
				X = midX + 1,
				Y = 4,
				Text = "Cancel",
				Activated = Close,
				Width = 8,
			});

		Widgets.Add(
			new Button()
			{
				X = midX + 12,
				Y = 4,
				Text = "Help",
				Width = 8,
				AccessKeyIndex = 0,
			});

		SetFocus(Widgets[1]);
	}
}
