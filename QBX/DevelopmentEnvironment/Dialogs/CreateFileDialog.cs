using System;
using System.Diagnostics.CodeAnalysis;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class CreateFileDialog : Dialog
{
	Label lblFileName;
	Border bdrFileName;
	TextInput txtFileName;

	Canvas cnvFileTypes;

	RadioButton optModule;
	Label lblModule;

	RadioButton optInclude;
	Label lblInclude;

	RadioButton optDocument;
	Label lblDocument;

	Button cmdOK;
	Button cmdCancel;
	Button cmdHelp;

	public string FileName => txtFileName.Text.ToString();

	public event Action? CreateFile;

	public CreateFileDialog(Machine machine, Configuration configuration)
		: base(machine, configuration)
	{
		InitializeComponent();

		HelpContextString = "-901";
	}

	[MemberNotNull(nameof(lblFileName))]
	[MemberNotNull(nameof(bdrFileName))]
	[MemberNotNull(nameof(txtFileName))]
	[MemberNotNull(nameof(cnvFileTypes))]
	[MemberNotNull(nameof(optModule))]
	[MemberNotNull(nameof(lblModule))]
	[MemberNotNull(nameof(optInclude))]
	[MemberNotNull(nameof(lblInclude))]
	[MemberNotNull(nameof(optDocument))]
	[MemberNotNull(nameof(lblDocument))]
	[MemberNotNull(nameof(cmdOK))]
	[MemberNotNull(nameof(cmdCancel))]
	[MemberNotNull(nameof(cmdHelp))]
	void InitializeComponent()
	{
		lblFileName = new Label();
		bdrFileName = new Border();
		txtFileName = new TextInput();
		cnvFileTypes = new Canvas();
		optModule = new RadioButton();
		lblModule = new Label();
		optInclude = new RadioButton();
		lblInclude = new Label();
		optDocument = new RadioButton();
		lblDocument = new Label();
		cmdOK = new Button();
		cmdCancel = new Button();
		cmdHelp = new Button();

		var formatGroup = new RadioButtonGroup() { optModule, optInclude, optDocument };

		Width = 48;
		Height = 9;
		Title = "Create File";

		lblFileName.X = 1;
		lblFileName.Y = 1;
		lblFileName.Text = "Name:";
		lblFileName.AccessKeyIndex = 0;
		lblFileName.FocusTarget = bdrFileName;
		lblFileName.AutoSize();

		txtFileName.X = 9;
		txtFileName.Y = 1;
		txtFileName.Width = 35;
		txtFileName.Height = 1;
		txtFileName.GotFocus = txtFileName_GotFocus;
		txtFileName.LostFocus = txtFileName_LostFocus;

		bdrFileName.Enclose(txtFileName);

		cnvFileTypes.X = 1;
		cnvFileTypes.Y = 4;
		cnvFileTypes.Width = 44;
		cnvFileTypes.Height = 1;
		cnvFileTypes.IsTabStop = true;
		cnvFileTypes.Children.Add(optModule);
		cnvFileTypes.Children.Add(lblModule);
		cnvFileTypes.Children.Add(optInclude);
		cnvFileTypes.Children.Add(lblInclude);
		cnvFileTypes.Children.Add(optDocument);
		cnvFileTypes.Children.Add(lblDocument);

		optModule.X = 3;
		optModule.Y = 4;
		optModule.RadioButtonGroup = formatGroup;

		lblModule.X = 7;
		lblModule.Y = 4;
		lblModule.Text = "Module";
		lblModule.AccessKeyIndex = 0;
		lblModule.FocusTarget = optModule;
		lblModule.AutoSize();

		optInclude.X = 17;
		optInclude.Y = 4;
		optInclude.RadioButtonGroup = formatGroup;

		lblInclude.X = 21;
		lblInclude.Y = 4;
		lblInclude.Text = "Include";
		lblInclude.AccessKeyIndex = 0;
		lblInclude.FocusTarget = optInclude;
		lblInclude.AutoSize();

		optDocument.X = 31;
		optDocument.Y = 4;
		optDocument.RadioButtonGroup = formatGroup;

		lblDocument.X = 35;
		lblDocument.Y = 4;
		lblDocument.Text = "Document";
		lblDocument.AccessKeyIndex = 7;
		lblDocument.FocusTarget = optDocument;
		lblDocument.AutoSize();

		cmdOK.X = 7;
		cmdOK.Y = 6;
		cmdOK.Width = 6;
		cmdOK.Height = 1;
		cmdOK.Text = "OK";
		cmdOK.Activated = cmdOK_Activated;

		cmdCancel.X = 19;
		cmdCancel.Y = 6;
		cmdCancel.Width = 10;
		cmdCancel.Height = 1;
		cmdCancel.Text = "Cancel";
		cmdCancel.Activated = cmdCancel_Activated;

		cmdHelp.X = 33;
		cmdHelp.Y = 6;
		cmdHelp.Width = 8;
		cmdHelp.Height = 1;
		cmdHelp.Text = "Help";
		cmdHelp.AccessKeyIndex = 0;
		cmdHelp.Activated = cmdHelp_Activated;

		Widgets.Add(lblFileName);
		Widgets.Add(bdrFileName);
		Widgets.Add(cnvFileTypes);
		Widgets.Add(cmdOK);
		Widgets.Add(cmdCancel);
		Widgets.Add(cmdHelp);

		formatGroup.Select(optModule, cnvFileTypes);

		SetFocus(bdrFileName);
	}

	private void txtFileName_GotFocus()
	{
		txtFileName.SelectAll();
		cmdOK.IsDefault = true;
	}

	private void txtFileName_LostFocus()
	{
		cmdOK.IsDefault = false;
	}

	private void cmdOK_Activated()
	{
		OnActivated();
	}

	private void cmdCancel_Activated()
	{
		Close();
	}

	private void cmdHelp_Activated()
	{
		OnShowHelpPopup();
	}

	protected override void OnActivated()
	{
		if (!txtFileName.Text.Contains((byte)'.'))
			txtFileName.Text.Append(".BAS");

		CreateFile?.Invoke();

		Close();
	}
}
