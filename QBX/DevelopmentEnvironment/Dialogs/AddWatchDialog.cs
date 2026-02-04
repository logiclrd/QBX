using System;
using System.Diagnostics.CodeAnalysis;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class AddWatchDialog : Dialog
{
	public event Action? AddWatch;

	public string WatchExpression => txtExpression.Text.ToString();

	Label lblPrompt;
	Border bdrExpression;
	TextInput txtExpression;
	Button cmdOK;
	Button cmdCancel;
	Button cmdHelp;

	public AddWatchDialog(Machine machine, Configuration configuration)
		: base(machine, configuration)
	{
		InitializeComponent();
	}

	[MemberNotNull(nameof(lblPrompt))]
	[MemberNotNull(nameof(bdrExpression))]
	[MemberNotNull(nameof(txtExpression))]
	[MemberNotNull(nameof(cmdOK))]
	[MemberNotNull(nameof(cmdCancel))]
	[MemberNotNull(nameof(cmdHelp))]
	void InitializeComponent()
	{
		Width = 55;
		Height = 10;
		Title = "Add Watch";

		lblPrompt = new Label();
		bdrExpression = new Border();
		txtExpression = new TextInput();
		cmdOK = new Button();
		cmdCancel = new Button();
		cmdHelp = new Button();

		lblPrompt.X = 4;
		lblPrompt.Y = 1;
		lblPrompt.Text = "Enter expression to be added to the Watch window:";
		lblPrompt.AutoSize();

		txtExpression.X = 2;
		txtExpression.Y = 4;
		txtExpression.Width = 49;
		txtExpression.Height = 1;

		bdrExpression.Enclose(txtExpression);

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

		Widgets.Add(lblPrompt);
		Widgets.Add(bdrExpression);
		Widgets.Add(cmdOK);
		Widgets.Add(cmdCancel);
		Widgets.Add(cmdHelp);

		SetFocus(bdrExpression);
	}

	protected override void OnActivated()
	{
		cmdOK_Activated();
	}

	void cmdOK_Activated()
	{
		AddWatch?.Invoke();
		Close();
	}

	void cmdCancel_Activated()
	{
		Close();
	}
}
