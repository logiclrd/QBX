using System;
using System.Linq;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.DevelopmentEnvironment.Help;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class HelpPopupDialog : Dialog
{
	public HelpPopupDialog(Machine machine, Configuration configuration, HelpDatabaseTopic topic) : base(machine, configuration)
	{
		Width = 68;
		Height = Math.Min(20, 6 + topic.Lines.Count);
		Title = "HELP: " + topic.TopicName;

		int midX = (Width - 2) / 2;

		int listBoxLines = this.Height - 4;
		int visibleLines = listBoxLines - 2;

		Widgets.Add(
			new VerticalListBox<string>()
			{
				X = 1,
				Y = 0,
				Width = 64,
				Height = listBoxLines,
				ShowSelectionWhenUnfocused = false,
				ShowSelectionWhenFocused = false,
				ShowScrollBar = (visibleLines < topic.Lines.Count),
			}.AddItems(topic.Lines.Select(line => line.ToPlainTextString())));

		Widgets.Add(
			new Button()
			{
				X = midX - 4,
				Y = this.Height - 3,
				Width = 8,
				Height = 1,
				Text = "OK",
				IsDefault = true,
			});

		SetFocus(0);
	}

	protected override void OnActivated()
	{
		Close();
	}
}
