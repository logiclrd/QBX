using System;
using System.Collections.Generic;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.ExecutionEngine.Execution;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class FindDialog : SearchDialogBase
{
	Button cmdOK;
	Button cmdCancel;
	Button cmdHelp;

	public Action<StringValue, SearchScope>? Find;

#pragma warning disable CS8618
	public FindDialog(SearchScopeMode searchScopeMode, Machine machine, Configuration configuration)
		: base(width: 59, searchScopeMode, machine, configuration)
	{
		HelpContextString = "-908";
	}
#pragma warning restore

	protected override void ConfigureDialog(List<Widget> widgets)
	{
		Height -= 3;

		for (int i=0; i < widgets.Count; i++)
		{
			var widget = widgets[i];

			if ((widget.Y > 1) && (widget.Y <= 4))
			{
				widgets.RemoveAt(i);
				i--;
			}

			foreach (var childWidget in widget.EnumerateAllWidgets())
				if (childWidget.Y > 4)
					childWidget.Y -= 3;
		}
	}

	protected override void AddDialogButtons(List<Widget> widgets)
	{
		cmdOK = new Button();
		cmdCancel = new Button();
		cmdHelp = new Button();

		cmdOK.X = 8;
		cmdOK.Y = 10;
		cmdOK.Width = 6;
		cmdOK.Height = 1;
		cmdOK.Text = "OK";
		cmdOK.IsDefault = true;
		cmdOK.Activated = cmdOK_Activated;

		cmdCancel.X = 22;
		cmdCancel.Y = 10;
		cmdCancel.Width = 10;
		cmdCancel.Height = 1;
		cmdCancel.Text = "Cancel";
		cmdCancel.Activated = cmdCancel_Activated;

		cmdHelp.X = 41;
		cmdHelp.Y = 10;
		cmdHelp.Width = 8;
		cmdHelp.Height = 1;
		cmdHelp.Text = "Help";
		cmdHelp.AccessKeyIndex = 0;
		cmdHelp.Activated = cmdHelp_Activated;

		widgets.Add(cmdOK);
		widgets.Add(cmdCancel);
		widgets.Add(cmdHelp);
	}

	private void cmdOK_Activated()
	{
		Close();
		Find?.Invoke(FindWhat, SearchScope);
	}

	private void cmdCancel_Activated()
	{
		Close();
	}

	private void cmdHelp_Activated()
	{
		OnShowHelpPopup();
	}
}
