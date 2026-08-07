using System;
using System.Collections.Generic;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.ExecutionEngine.Execution;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class ChangeDialog : SearchDialogBase
{
	Button cmdFindAndVerify;
	Button cmdChangeAll;
	Button cmdCancel;
	Button cmdHelp;

#pragma warning disable CS8618
	public ChangeDialog(Machine machine, Configuration configuration)
		: base(width: 58, machine, configuration)
	{
		HelpContextString = "-907";
	}
#pragma warning restore

	public Action<StringValue, StringValue>? Change;
	public Action<StringValue, StringValue>? ChangeAll;

	protected override void AddDialogButtons(List<Widget> widgets)
	{
		cmdFindAndVerify = new Button();
		cmdChangeAll = new Button();
		cmdCancel = new Button();
		cmdHelp = new Button();

		cmdFindAndVerify.X = 1;
		cmdFindAndVerify.Y = 13;
		cmdFindAndVerify.Width = 19;
		cmdFindAndVerify.Height = 1;
		cmdFindAndVerify.Text = "Find and Verify";
		cmdFindAndVerify.AccessKeyIndex = 9;
		cmdFindAndVerify.IsDefault = true;
		cmdFindAndVerify.Activated = cmdFindAndVerify_Activated;

		cmdChangeAll.X = 21;
		cmdChangeAll.Y = 13;
		cmdChangeAll.Width = 14;
		cmdChangeAll.Height = 1;
		cmdChangeAll.Text = "Change All";
		cmdChangeAll.AccessKeyIndex = 0;
		cmdChangeAll.Activated = cmdChangeAll_Activated;

		cmdCancel.X = 36;
		cmdCancel.Y = 13;
		cmdCancel.Width = 10;
		cmdCancel.Height = 1;
		cmdCancel.Text = "Cancel";
		cmdCancel.Activated = cmdCancel_Activated;

		cmdHelp.X = 47;
		cmdHelp.Y = 13;
		cmdHelp.Width = 8;
		cmdHelp.Height = 1;
		cmdHelp.Text = "Help";
		cmdHelp.AccessKeyIndex = 0;
		cmdHelp.Activated = cmdHelp_Activated;

		widgets.Add(cmdFindAndVerify);
		widgets.Add(cmdChangeAll);
		widgets.Add(cmdCancel);
		widgets.Add(cmdHelp);
	}

	private void cmdFindAndVerify_Activated()
	{
		Change?.Invoke(FindWhat, ChangeTo);
	}

	private void cmdChangeAll_Activated()
	{
		Close();
		ChangeAll?.Invoke(FindWhat, ChangeTo);
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
