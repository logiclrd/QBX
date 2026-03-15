using System;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class PromptToDeleteProcedureDialog : Dialog
{
	public event Action? Delete;
	public event Action? DoNotDelete;

	protected void OnDelete() => Delete?.Invoke();
	protected void OnDoNotDelete() => DoNotDelete?.Invoke();

	public PromptToDeleteProcedureDialog(Machine machine, Configuration configuration)
		: base(machine, configuration)
	{
		HelpContextString = "-212";

		Width = 36;
		Height = 7;

		int midX = (Width - 2) / 2;

		Widgets.Add(
			new Label()
			{
				X = 2,
				Y = 1,
				Text = "Delete procedure from module?",
			}.AutoSize());

		Widgets.Add(
			new Button()
			{
				X = midX - 15,
				Y = 4,
				Text = "OK",
				Width = 8,
				Activated = () => { Close(); OnDelete(); }
			});

		Widgets.Add(
			new Button()
			{
				X = midX - 4,
				Y = 4,
				Text = "Cancel",
				Width = 8,
				AccessKeyIndex = 0,
				Activated = () => { Close(); OnDoNotDelete(); }
			});

		Widgets.Add(
			new Button()
			{
				X = midX + 7,
				Y = 4,
				Text = "Help",
				Width = 8,
				AccessKeyIndex = 0,
				Activated = OnShowHelpPopup,
			});

		SetFocus(Widgets[1]);
	}
}
