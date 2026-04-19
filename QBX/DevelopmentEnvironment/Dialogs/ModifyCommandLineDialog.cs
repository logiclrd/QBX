using System;
using System.Diagnostics.CodeAnalysis;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class ModifyCommandLineDialog : Dialog
{
	public event Action? UpdateCommandLine;

	public string CommandLine
	{
		get => txtCommandLine.Text.ToString();
		set
		{
			txtCommandLine.Text.Set(value);
			txtCommandLine.SelectAll();
		}
	}

	Label lblPrompt;
	Border bdrCommandLine;
	TextInput txtCommandLine;
	Button cmdOK;
	Button cmdCancel;
	Button cmdHelp;

	public ModifyCommandLineDialog(Machine machine, Configuration configuration)
		: base(machine, configuration)
	{
		InitializeComponent();

		HelpContextString = "-246";
	}

	[MemberNotNull(nameof(lblPrompt))]
	[MemberNotNull(nameof(bdrCommandLine))]
	[MemberNotNull(nameof(txtCommandLine))]
	[MemberNotNull(nameof(cmdOK))]
	[MemberNotNull(nameof(cmdCancel))]
	[MemberNotNull(nameof(cmdHelp))]
	void InitializeComponent()
	{
		Width = 55;
		Height = 10;
		Title = "Modify COMMAND$";

		lblPrompt = new Label();
		bdrCommandLine = new Border();
		txtCommandLine = new TextInput();
		cmdOK = new Button();
		cmdCancel = new Button();
		cmdHelp = new Button();

		lblPrompt.X = 4;
		lblPrompt.Y = 1;
		lblPrompt.Text = "Enter text for COMMAND$:";
		lblPrompt.AutoSize();

		txtCommandLine.X = 2;
		txtCommandLine.Y = 4;
		txtCommandLine.Width = 49;
		txtCommandLine.Height = 1;

		bdrCommandLine.Enclose(txtCommandLine);

		cmdOK.X = 9;
		cmdOK.Y = 7;
		cmdOK.Width = 6;
		cmdOK.Height = 1;
		cmdOK.Text = "OK";
		cmdOK.Activated += cmdOK_Activated;

		cmdCancel.X = 22;
		cmdCancel.Y = 7;
		cmdCancel.Width = 10;
		cmdCancel.Height = 1;
		cmdCancel.Text = "Cancel";
		cmdCancel.Activated += cmdCancel_Activated;

		cmdHelp.X = 39;
		cmdHelp.Y = 7;
		cmdHelp.Width = 8;
		cmdHelp.Height = 1;
		cmdHelp.Text = "Help";
		cmdHelp.Activated += cmdHelp_Activated;

		Widgets.Add(lblPrompt);
		Widgets.Add(bdrCommandLine);
		Widgets.Add(cmdOK);
		Widgets.Add(cmdCancel);
		Widgets.Add(cmdHelp);

		SetFocus(bdrCommandLine);
	}

	protected override void OnActivated()
	{
		cmdOK_Activated();
	}

	void cmdOK_Activated()
	{
		UpdateCommandLine?.Invoke();
		Close();
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
