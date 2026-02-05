using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class DisplayDialog : Dialog
{
	Border bdrColour;
	Label lblUserInterfaceElement;
	VerticalListBox<DisplayAttribute> lstUserInterfaceElement;
	Label lblForeground;
	VerticalListBox<DisplayAttribute> lstForeground;
	Label lblBackground;
	VerticalListBox<DisplayAttribute> lstBackground;
	Label lblSampleText;
	Border bdrDisplayOptions;
	CheckBox chkScrollBars;
	Label lblScrollBarsLabel;
	Label lblTabStops;
	TextInput txtTabStops;
	Button cmdOK;
	Button cmdCancel;
	Button cmdHelp;

	Configuration _configuration;

	Configuration.DisplayAttributeConfiguration _workCopy;

	public DisplayDialog(Machine machine, Configuration configuration)
		: base(machine, configuration)
	{
		InitializeComponent();

		_configuration = configuration;

		chkScrollBars.IsChecked = configuration.ShowScrollBars;

		txtTabStops.Text.Set(configuration.TabSize.ToString());

		_workCopy = new Configuration.DisplayAttributeConfiguration(
			clone: configuration.DisplayAttributes);

		lstUserInterfaceElement.Items.AddRange(
			_workCopy.AllItems.Select(item =>
				new ListBoxItem<DisplayAttribute>(item.Name, item)));

		lstForeground.Items.AddRange(
			_workCopy.Palette.Select(entry =>
				new ListBoxItem<DisplayAttribute>(entry.Name, entry)));
		lstBackground.Items.AddRange(
			_workCopy.Palette.Select(entry =>
				new ListBoxItem<DisplayAttribute>(entry.Name, entry)));

		lstUserInterfaceElement.SelectItem(0);
	}

	[MemberNotNull(nameof(bdrColour))]
	[MemberNotNull(nameof(lblUserInterfaceElement))]
	[MemberNotNull(nameof(lstUserInterfaceElement))]
	[MemberNotNull(nameof(lblForeground))]
	[MemberNotNull(nameof(lstForeground))]
	[MemberNotNull(nameof(lblBackground))]
	[MemberNotNull(nameof(lstBackground))]
	[MemberNotNull(nameof(lblSampleText))]
	[MemberNotNull(nameof(bdrDisplayOptions))]
	[MemberNotNull(nameof(chkScrollBars))]
	[MemberNotNull(nameof(lblScrollBarsLabel))]
	[MemberNotNull(nameof(lblTabStops))]
	[MemberNotNull(nameof(txtTabStops))]
	[MemberNotNull(nameof(cmdOK))]
	[MemberNotNull(nameof(cmdCancel))]
	[MemberNotNull(nameof(cmdHelp))]
	void InitializeComponent()
	{
		bdrColour = new Border();
		lblUserInterfaceElement = new Label();
		lstUserInterfaceElement = new VerticalListBox<DisplayAttribute>();
		lblForeground = new Label();
		lstForeground = new VerticalListBox<DisplayAttribute>();
		lblBackground = new Label();
		lstBackground = new VerticalListBox<DisplayAttribute>();
		lblSampleText = new Label();
		bdrDisplayOptions = new Border();
		chkScrollBars = new CheckBox();
		lblScrollBarsLabel = new Label();
		lblTabStops = new Label();
		txtTabStops = new TextInput();
		cmdOK = new Button();
		cmdCancel = new Button();
		cmdHelp = new Button();

		Width = 62;
		Height = 22;
		Title = "Display";

		bdrColour.X = 0;
		bdrColour.Y = 0;
		bdrColour.Width = 60;
		bdrColour.Height = 14;
		bdrColour.Title = "Color";

		lblUserInterfaceElement.X = 2;
		lblUserInterfaceElement.Y = 1;
		lblUserInterfaceElement.Text = "User-Interface Element:";
		lblUserInterfaceElement.AccessKeyIndex = 0;
		lblUserInterfaceElement.FocusTarget = lstUserInterfaceElement;
		lblUserInterfaceElement.AutoSize();

		// TODO: render border and unselected items using the pull-down menu normal text attribute
		lstUserInterfaceElement.X = 1;
		lstUserInterfaceElement.Y = 2;
		lstUserInterfaceElement.Width = 58;
		lstUserInterfaceElement.Height = 5;
		lstUserInterfaceElement.ShowSelectionWhenUnfocused = true;
		lstUserInterfaceElement.SelectionChanged += lstUserInterfaceElement_SelectionChanged;

		lblForeground.X = 2;
		lblForeground.Y = 9;
		lblForeground.Text = "Foreground:";
		lblForeground.AccessKeyIndex = 0;
		lblForeground.FocusTarget = lstForeground;
		lblForeground.AutoSize();

		lstForeground.X = 13;
		lstForeground.Y = 7;
		lstForeground.Width = 16;
		lstForeground.Height = 5;
		lstForeground.ShowSelectionWhenUnfocused = true;
		lstForeground.SelectionChanged += lstForeground_SelectionChanged;

		lblBackground.X = 32;
		lblBackground.Y = 9;
		lblBackground.Text = "Background:";
		lblBackground.AccessKeyIndex = 0;
		lblBackground.FocusTarget = lstBackground;
		lblBackground.AutoSize();

		lstBackground.X = 43;
		lstBackground.Y = 7;
		lstBackground.Width = 16;
		lstBackground.Height = 5;
		lstBackground.ShowSelectionWhenUnfocused = true;
		lstBackground.SelectionChanged += lstBackground_SelectionChanged;

		lblSampleText.X = 22;
		lblSampleText.Y = 12;
		lblSampleText.Text = " Sample Text ";
		lblSampleText.AutoSize();

		bdrDisplayOptions.X = 0;
		bdrDisplayOptions.Y = 15;
		bdrDisplayOptions.Width = 59;
		bdrDisplayOptions.Height = 3;
		bdrDisplayOptions.Title = "Display Options";

		chkScrollBars.X = 4;
		chkScrollBars.Y = 16;

		lblScrollBarsLabel.X = 8;
		lblScrollBarsLabel.Y = 16;
		lblScrollBarsLabel.Text = "Scroll Bars";
		lblScrollBarsLabel.AccessKeyIndex = 0;
		lblScrollBarsLabel.FocusTarget = chkScrollBars;
		lblScrollBarsLabel.AutoSize();

		lblTabStops.X = 37;
		lblTabStops.Y = 16;
		lblTabStops.Text = "Tab Stops:";
		lblTabStops.AccessKeyIndex = 0;
		lblTabStops.FocusTarget = txtTabStops;
		lblTabStops.AutoSize();

		txtTabStops.X = 48;
		txtTabStops.Y = 16;
		txtTabStops.Width = 4;
		txtTabStops.Height = 1;
		txtTabStops.GotFocus += txtTabStops_GotFocus;

		cmdOK.X = 9;
		cmdOK.Y = 19;
		cmdOK.Width = 6;
		cmdOK.Height = 1;
		cmdOK.Text = "OK";
		cmdOK.Activated += cmdOK_Activated;

		cmdCancel.X = 23;
		cmdCancel.Y = 19;
		cmdCancel.Width = 10;
		cmdCancel.Height = 1;
		cmdCancel.Text = "Cancel";
		cmdCancel.Activated += cmdCancel_Activated;

		cmdHelp.X = 41;
		cmdHelp.Y = 19;
		cmdHelp.Width = 8;
		cmdHelp.Height = 1;
		cmdHelp.Text = "Help";
		cmdHelp.AccessKeyIndex = 0;

		Widgets.Add(bdrColour);
		Widgets.Add(lblUserInterfaceElement);
		Widgets.Add(lstUserInterfaceElement);
		Widgets.Add(lblForeground);
		Widgets.Add(lstForeground);
		Widgets.Add(lblBackground);
		Widgets.Add(lstBackground);
		Widgets.Add(lblSampleText);
		Widgets.Add(bdrDisplayOptions);
		Widgets.Add(chkScrollBars);
		Widgets.Add(lblScrollBarsLabel);
		Widgets.Add(lblTabStops);
		Widgets.Add(txtTabStops);
		Widgets.Add(cmdOK);
		Widgets.Add(cmdCancel);
		Widgets.Add(cmdHelp);

		SetFocus(lstUserInterfaceElement);
	}

	bool _isLoading = false;

	void LoadSelection()
	{
		_isLoading = true;

		try
		{
			var selectedItem = lstUserInterfaceElement.SelectedValue;

			lblSampleText.DisplayAttribute = selectedItem;

			for (int i = 0; i < lstForeground.Items.Count; i++)
			{
				if (lstForeground.Items[i].Value.Foreground == selectedItem.Foreground)
				{
					lstForeground.SelectItem(i);
					break;
				}
			}

			for (int i = 0; i < lstBackground.Items.Count; i++)
			{
				if (lstBackground.Items[i].Value.Foreground == selectedItem.Background)
				{
					lstBackground.SelectItem(i);
					break;
				}
			}
		}
		finally
		{
			_isLoading = false;
		}
	}

	void lstUserInterfaceElement_SelectionChanged()
	{
		LoadSelection();
	}

	void lstForeground_SelectionChanged()
	{
		if (!_isLoading)
		{
			var selectedItem = lstUserInterfaceElement.SelectedValue;

			selectedItem.Foreground = lstForeground.SelectedValue.Foreground;
		}
	}

	void lstBackground_SelectionChanged()
	{
		if (!_isLoading)
		{
			var selectedItem = lstUserInterfaceElement.SelectedValue;

			selectedItem.Background = lstBackground.SelectedValue.Foreground;
		}
	}

	void txtTabStops_GotFocus()
	{
		// This is weird but it's what QuickBASIC does.
		txtTabStops.ScrollX = txtTabStops.Text.Length - 1;
		if (txtTabStops.ScrollX < 0)
			txtTabStops.ScrollX = 0;
	}

	protected override void OnActivated()
	{
		_configuration.DisplayAttributes.CopyFrom(_workCopy);
		_configuration.ShowScrollBars = chkScrollBars.IsChecked;
		if (int.TryParse(txtTabStops.Text.ToString(), out var tabSize))
			_configuration.TabSize = tabSize;

		Close();
	}

	void cmdOK_Activated()
	{
		OnActivated();
	}

	void cmdCancel_Activated()
	{
		Close();
	}
}
