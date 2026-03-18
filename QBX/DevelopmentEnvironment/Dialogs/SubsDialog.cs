using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Hardware;

using CompilationElement = QBX.CodeModel.CompilationElement;
using CompilationElementType = QBX.CodeModel.CompilationElementType;

namespace QBX.DevelopmentEnvironment.Dialogs;

public delegate void PromptAction<TDialog>(Action<TDialog> configure, Action continuation, Action cancellation)
	where TDialog : Dialog;

public class SubsDialog : Dialog
{
	Label lblChooseProgramItem;
	HorizontalListBox<IEditableElement> lstItems;
	Label lblSelectedItemDescription;
	Button cmdEditInActive;
	Button cmdEditInSplit;
	Button cmdCancel;
	Button cmdDelete;
	Button cmdMove;
	Button cmdHelp;

	public IEditableElement SelectedItem
	{
		get => lstItems.SelectedValue;
		set => lstItems.SelectItem(value);
	}

	public event Action? EditInActive;
	public event Action? EditInSplit;

	public event Action<Dialog>? ShowDialog;
	public event Action? RemoveElement;

	public event PromptAction<PromptToSaveDialog>? PromptToSaveChanges;
	public event Action? UnloadFile;

	public event PromptAction<SelectModuleDialog>? SelectNewMainModule;

	public event Action? RestartDialog;

	List<IEditableUnit> _units;
	IEditableUnit _mainModule;

	Machine _machine;
	Configuration _configuration;

	public SubsDialog(List<IEditableUnit> units, Machine machine, Configuration configuration)
		: base(machine, configuration)
	{
		_machine = machine;
		_configuration = configuration;

		InitializeComponent();

		_units = units;
		_mainModule = units[0];

		RefreshList();
	}

	void RefreshList()
	{
		lstItems.Items.Clear();

		foreach (var unit in _units)
		{
			var mainElement = unit.MainElement;

			var elements = unit.Elements
				.Where(e => e != mainElement)
				.OrderBy(e => e.DisplayName?.ToString(), StringComparer.OrdinalIgnoreCase);

			AddItem(mainElement, unit.Name);

			foreach (var element in elements)
				AddItem(element, "  " + (element.DisplayName ?? "<unknown>"));
		}

		lstItems.RecalculateColumns();
		lstItems.SelectItem(0);
	}

	[MemberNotNull(nameof(lblChooseProgramItem))]
	[MemberNotNull(nameof(lstItems))]
	[MemberNotNull(nameof(lblSelectedItemDescription))]
	[MemberNotNull(nameof(cmdEditInActive))]
	[MemberNotNull(nameof(cmdEditInSplit))]
	[MemberNotNull(nameof(cmdCancel))]
	[MemberNotNull(nameof(cmdDelete))]
	[MemberNotNull(nameof(cmdMove))]
	[MemberNotNull(nameof(cmdHelp))]
	void InitializeComponent()
	{
		lblChooseProgramItem = new Label();
		lstItems = new HorizontalListBox<IEditableElement>();
		lblSelectedItemDescription = new Label();
		cmdEditInActive = new Button();
		cmdEditInSplit = new Button();
		cmdCancel = new Button();
		cmdDelete = new Button();
		cmdMove = new Button();
		cmdHelp = new Button();

		Width = 74;
		Height = 20;
		Title = "SUBs";
		HelpContextString = "-911";

		lblChooseProgramItem.X = 1;
		lblChooseProgramItem.Y = 1;
		lblChooseProgramItem.Text = "Choose program item to edit:";
		lblChooseProgramItem.AccessKeyIndex = 0;
		lblChooseProgramItem.FocusTarget = lstItems;
		lblChooseProgramItem.AutoSize();

		lstItems.X = 1;
		lstItems.Y = 2;
		lstItems.Width = 70;
		lstItems.Height = 11;
		lstItems.ShowSelectionWhenUnfocused = true;
		lstItems.GotFocus = lstItems_GotFocus;
		lstItems.LostFocus = lstItems_LostFocus;
		lstItems.SelectionChanged = lstItems_SelectionChanged;

		lblSelectedItemDescription.X = 1;
		lblSelectedItemDescription.Y = 13;
		lblSelectedItemDescription.Text = "";
		lblSelectedItemDescription.Width = 70;

		cmdEditInActive.X = 3;
		cmdEditInActive.Y = 15;
		cmdEditInActive.Width = 18;
		cmdEditInActive.Text = "Edit in Active";
		cmdEditInActive.AccessKeyIndex = 8;
		cmdEditInActive.Activated = cmdEditInActive_Activated;

		cmdEditInSplit.X = 32;
		cmdEditInSplit.Y = 15;
		cmdEditInSplit.Width = 17;
		cmdEditInSplit.Text = "Edit in Split";
		cmdEditInSplit.AccessKeyIndex = 8;
		cmdEditInSplit.Activated = cmdEditInSplit_Activated;

		cmdCancel.X = 59;
		cmdCancel.Y = 15;
		cmdCancel.Width = 10;
		cmdCancel.Text = "Cancel";
		cmdCancel.Activated = cmdCancel_Activated;

		cmdDelete.X = 6;
		cmdDelete.Y = 17;
		cmdDelete.Width = 10;
		cmdDelete.Text = "Delete";
		cmdDelete.AccessKeyIndex = 0;
		cmdDelete.Activated = cmdDelete_Activated;

		cmdMove.X = 37;
		cmdMove.Y = 17;
		cmdMove.Width = 8;
		cmdMove.Text = "Move";
		cmdMove.AccessKeyIndex = 0;
		cmdMove.Activated = cmdMove_Activated;

		cmdHelp.X = 60;
		cmdHelp.Y = 17;
		cmdHelp.Width = 8;
		cmdHelp.Text = "Help";
		cmdHelp.AccessKeyIndex = 0;
		cmdHelp.Activated = cmdHelp_Activated;

		Widgets.Add(lblChooseProgramItem);
		Widgets.Add(lstItems);
		Widgets.Add(lblSelectedItemDescription);
		Widgets.Add(cmdEditInActive);
		Widgets.Add(cmdEditInSplit);
		Widgets.Add(cmdCancel);
		Widgets.Add(cmdDelete);
		Widgets.Add(cmdMove);
		Widgets.Add(cmdHelp);

		SetFocus(lstItems);
	}

	const int ColumnWidth = 21;

	void AddItem(IEditableElement element, string name)
	{
		int elementSize = element.SizeInBytes;

		int elementKilobytes = (elementSize + 1023) / 1024;

		if (elementKilobytes == 0)
			elementKilobytes = 1;

		string nameColumnText;
		string sizeColumnText = elementKilobytes.ToString();

		int remainingChars = ColumnWidth - sizeColumnText.Length - 1;

		if (name.Length <= remainingChars)
			nameColumnText = name;
		else
		{
			int keepChars = remainingChars - 3; // make room for the ellipsis

			int keepLeft = (keepChars + 1) / 2;
			int keepRight = keepChars - keepLeft;

			nameColumnText = name.Substring(0, keepLeft) + "..." + name.Substring(name.Length - keepRight);
		}

		string label = nameColumnText.PadRight(remainingChars + 1, ' ') + sizeColumnText;

		lstItems.Items.Add(new ListBoxItem<IEditableElement>(label, element));
	}

	string GetItemDescription(IEditableElement element)
	{
		var unit = element.Owner;

		if (unit.IncludeInBuild)
		{
			if (element == unit.MainElement)
			{
				if (unit == _mainModule)
					return unit.Name + " is the main module";
				else
					return unit.Name + " is a module";
			}
			else
			{
				string elementType = "routine";

				if (element is CompilationElement compilationElement)
				{
					switch (compilationElement.Type)
					{
						case CompilationElementType.Sub: elementType = "SUB"; break;
						case CompilationElementType.Function: elementType = "FUNCTION"; break;
					}
				}

				return element.DisplayName?.ToString() + " is a " + elementType + " in " + unit.Name;
			}
		}
		else if (unit.EnableSmartEditor)
			return unit.Name + " is an include file";
		else
			return unit.Name + " is a document";
	}

	private void lstItems_GotFocus()
	{
		cmdEditInActive.IsDefault = true;
	}

	private void lstItems_LostFocus()
	{
		cmdEditInActive.IsDefault = false;
	}

	private void lstItems_SelectionChanged()
	{
		lblSelectedItemDescription.Text = GetItemDescription(lstItems.SelectedValue);
	}

	private void cmdEditInActive_Activated()
	{
		EditInActive?.Invoke();
		Close();
	}

	private void cmdEditInSplit_Activated()
	{
		EditInSplit?.Invoke();
		Close();
	}

	private void cmdCancel_Activated()
	{
		Close();
	}

	private void cmdDelete_Activated()
	{
		var element = SelectedItem;

		var unit = element.Owner;

		if (element == unit.MainElement)
		{
			Action proceedWithRemoveModule =
				() =>
				{
					if (unit == _mainModule)
					{
						IsVisible = false;

						SelectNewMainModule?.Invoke(
							configure: dialog => dialog.AddExclusion(unit),
							continuation:
								() =>
								{
									UnloadFile?.Invoke();
									Close();
									RestartDialog?.Invoke();
								},
							cancellation:
								() =>
								{
									IsVisible = true;
								});
					}
					else
						RefreshList();
				};

			if (unit.IsPristine || (PromptToSaveChanges == null))
				proceedWithRemoveModule();
			else
			{
				IsVisible = false;

				PromptToSaveChanges.Invoke(
					configure: _ => { },
					continuation:
						() =>
						{
							IsVisible = true;
							proceedWithRemoveModule();
						},
					cancellation:
						() =>
						{
							IsVisible = true;
						});
			}
		}
		else
		{
			var promptDialog = new PromptToDeleteProcedureDialog(_machine, _configuration);

			if (ShowDialog != null)
			{
				IsVisible = false;

				promptDialog.Delete +=
					() =>
					{
						IsVisible = true;
						RemoveElement?.Invoke();
						RefreshList();
					};

				promptDialog.DoNotDelete +=
					() =>
					{
						IsVisible = true;
					};

				ShowDialog.Invoke(promptDialog);
			}
		}
	}

	private void cmdMove_Activated()
	{
		// TODO
	}

	private void cmdHelp_Activated()
	{
		OnShowHelpPopup();
	}

	protected override void OnActivated()
	{
		EditInActive?.Invoke();
		Close();
	}
}
