using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public class SelectModuleDialog : Dialog
{
	Label lblChooseNewMainModule;
	VerticalListBox<IEditableUnit> lstItems;
	Button cmdOK;
	Button cmdCancel;
	Button cmdHelp;

	public IEditableUnit SelectedItem
	{
		get => lstItems.SelectedValue;
		set => lstItems.SelectItem(value);
	}

	public event Action? ModuleSelected;

	public void AddExclusion(IEditableUnit exclusion)
	{
		_exclusions ??= new List<IEditableUnit>();
		_exclusions.Add(exclusion);

		if (IsVisible)
			RefreshList();
		else
			_needRefreshList = true;
	}

	List<IEditableUnit> _units;
	List<IEditableUnit>? _exclusions;

	bool _needRefreshList;

	public SelectModuleDialog(List<IEditableUnit> units, Machine machine, Configuration configuration)
		: base(machine, configuration)
	{
		InitializeComponent();

		_units = units;
		_needRefreshList = true;
	}

	void RefreshList()
	{
		_needRefreshList = false;

		lstItems.Items.Clear();

		IEnumerable<IEditableUnit> filteredUnits = _units;

		if (_exclusions != null)
			filteredUnits = filteredUnits.Except(_exclusions);

		foreach (var unit in filteredUnits)
			lstItems.Items.Add(new ListBoxItem<IEditableUnit>(unit.Name, unit));

		lstItems.SelectItem(0);
	}

	[MemberNotNull(nameof(lblChooseNewMainModule))]
	[MemberNotNull(nameof(lstItems))]
	[MemberNotNull(nameof(cmdOK))]
	[MemberNotNull(nameof(cmdCancel))]
	[MemberNotNull(nameof(cmdHelp))]
	void InitializeComponent()
	{
		lblChooseNewMainModule = new Label();
		lstItems = new VerticalListBox<IEditableUnit>();
		cmdOK = new Button();
		cmdCancel = new Button();
		cmdHelp = new Button();

		Width = 34;
		Height = 16;
		Title = "Set Main Module";
		HelpContextString = "-266";

		lblChooseNewMainModule.X = 2;
		lblChooseNewMainModule.Y = 0;
		lblChooseNewMainModule.Text = "Choose new main module:";
		lblChooseNewMainModule.AutoSize();

		lstItems.X = 2;
		lstItems.Y = 1;
		lstItems.Width = 28;
		lstItems.Height = 10;
		lstItems.ShowSelectionWhenUnfocused = true;
		lstItems.GotFocus = lstItems_GotFocus;
		lstItems.LostFocus = lstItems_LostFocus;

		cmdOK.X = 2;
		cmdOK.Y = 13;
		cmdOK.Width = 6;
		cmdOK.Text = "OK";
		cmdOK.Activated = cmdOK_Activated;

		cmdCancel.X = 10;
		cmdCancel.Y = 13;
		cmdCancel.Width = 10;
		cmdCancel.Text = "Cancel";
		cmdCancel.Activated = cmdCancel_Activated;

		cmdHelp.X = 22;
		cmdHelp.Y = 13;
		cmdHelp.Width = 8;
		cmdHelp.Text = "Help";
		cmdHelp.AccessKeyIndex = 0;
		cmdHelp.Activated = cmdHelp_Activated;

		Widgets.Add(lblChooseNewMainModule);
		Widgets.Add(lstItems);
		Widgets.Add(cmdOK);
		Widgets.Add(cmdCancel);
		Widgets.Add(cmdHelp);

		SetFocus(lstItems);
	}

	private void lstItems_GotFocus()
	{
		cmdOK.IsDefault = true;
	}

	private void lstItems_LostFocus()
	{
		cmdOK.IsDefault = false;
	}

	private void cmdOK_Activated()
	{
		ModuleSelected?.Invoke();
		Close();
	}

	private void cmdCancel_Activated()
	{
		Close();
	}

	private void cmdHelp_Activated()
	{
		OnShowHelpPopup();
	}

	protected override void OnShown()
	{
		if (_needRefreshList)
			RefreshList();
	}

	protected override void OnActivated()
	{
		ModuleSelected?.Invoke();
		Close();
	}
}
