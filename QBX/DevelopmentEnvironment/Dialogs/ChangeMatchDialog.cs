using System;
using System.Diagnostics.CodeAnalysis;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class ChangeMatchDialog : Dialog
{
	Button cmdChange;
	Button cmdSkip;
	Button cmdCancel;
	Button cmdHelp;

	public event Action? PerformChange;
	public event Action? FindNext;

	protected virtual void OnPerformChange() => PerformChange?.Invoke();
	protected virtual void OnFindNext() => FindNext?.Invoke();

	Machine _machine;

	public ChangeMatchDialog(Machine machine, Configuration configuration)
		: base(machine, configuration)
	{
		_machine = machine;

		HelpContextString = "-909";

		Width = 53;
		Height = 5;

		InitializeComponent();
	}

	protected override void OnShown()
	{
		Y = _machine.VideoFirmware.VisualLibrary.CharacterHeight - 6;
	}

	[MemberNotNull(nameof(cmdChange))]
	[MemberNotNull(nameof(cmdSkip))]
	[MemberNotNull(nameof(cmdCancel))]
	[MemberNotNull(nameof(cmdHelp))]
	void InitializeComponent()
	{
		cmdChange = new Button();
		cmdSkip = new Button();
		cmdCancel = new Button();
		cmdHelp = new Button();

		cmdChange.X = 3;
		cmdChange.Y = 2;
		cmdChange.Width = 10;
		cmdChange.Text = "Change";
		cmdChange.AccessKeyIndex = 0;
		cmdChange.Activated = cmdChange_Activated;

		cmdSkip.X = 15;
		cmdSkip.Y = 2;
		cmdSkip.Width = 8;
		cmdSkip.AccessKeyIndex = 0;
		cmdSkip.Text = "Skip";
		cmdSkip.Activated = cmdSkip_Activated;

		cmdCancel.X = 27;
		cmdCancel.Y = 2;
		cmdCancel.Width = 10;
		cmdCancel.Text = "Cancel";
		cmdCancel.Activated = cmdCancel_Activated;

		cmdHelp.X = 40;
		cmdHelp.Y = 2;
		cmdHelp.Width = 8;
		cmdHelp.Text = "Help";
		cmdHelp.AccessKeyIndex = 0;
		cmdHelp.Activated = cmdHelp_Activated;

		Widgets.Add(cmdChange);
		Widgets.Add(cmdSkip);
		Widgets.Add(cmdCancel);
		Widgets.Add(cmdHelp);

		SetFocus(cmdChange);
	}

	void cmdChange_Activated()
	{
		Close();

		OnPerformChange();
		OnFindNext();
	}

	void cmdSkip_Activated()
	{
		Close();

		OnFindNext();
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
