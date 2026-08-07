using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.ExecutionEngine.Execution;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public abstract class SearchDialogBase : Dialog
{
	protected Label lblFindWhat;
	protected Border bdrFindWhat;
	protected TextInput txtFindWhat;

	protected Label lblChangeTo;
	protected Border bdrChangeTo;
	protected TextInput txtChangeTo;

	protected CheckBox chkMatchUpperLowercase;
	protected Label lblMatchUpperLowercaseLabel;
	protected CheckBox chkWholeWord;
	protected Label lblWholeWordLabel;

	protected Border bdrSearch;

	protected Canvas cnvSearch;

	protected RadioButton optSearchActiveWindow;
	protected Label lblSearchActiveWindowLabel;
	protected RadioButton optSearchCurrentModule;
	protected Label lblSearchCurrentModuleLabel;
	protected RadioButton optSearchAllModules;
	protected Label lblSearchAllModulesLabel;
	protected RadioButton optSearchHelpFile;
	protected Label lblSearchHelpFileLabel;

	public StringValue FindWhat
	{
		get => txtFindWhat.Text;
		set => txtFindWhat.Text = value;
	}

	public StringValue ChangeTo
	{
		get => txtChangeTo.Text;
		set => txtChangeTo.Text = value;
	}

	public SearchScopeMode SearchScopeMode
	{
		get;
		init;
	}

	public SearchScope SearchScope
	{
		get
		{
			if (optSearchActiveWindow.IsSelected)
				return SearchScope.ActiveWindow;
			if (optSearchCurrentModule.IsSelected)
				return SearchScope.CurrentModule;
			if (optSearchAllModules.IsSelected)
				return SearchScope.AllModules;
			if (optSearchHelpFile.IsSelected)
				return SearchScope.HelpFile;

			return SearchScope.ActiveWindow;
		}
		set
		{
			var newScope = value;

			switch (newScope)
			{
				case SearchScope.CurrentModule:
				case SearchScope.AllModules:
					if (SearchScopeMode == SearchScopeMode.HelpFile)
						newScope = SearchScope.HelpFile;
					break;
				case SearchScope.HelpFile:
					if (SearchScopeMode == SearchScopeMode.TextEditor)
						newScope = SearchScope.CurrentModule;
					break;
			}

			optSearchActiveWindow.IsSelected = (newScope == SearchScope.ActiveWindow);
			optSearchCurrentModule.IsSelected = (newScope == SearchScope.CurrentModule);
			optSearchAllModules.IsSelected = (newScope == SearchScope.AllModules);
			optSearchHelpFile.IsSelected = (newScope == SearchScope.HelpFile);
		}
	}

	public SearchDialogBase(int width, SearchScopeMode searchScopeMode, Machine machine, Configuration configuration)
		: base(machine, configuration)
	{
		SearchScopeMode = searchScopeMode;

		InitializeComponent(width);
	}

	protected abstract void AddDialogButtons(List<Widget> widgets);

	protected virtual void ConfigureDialog(List<Widget> widgets) { }

	[MemberNotNull(nameof(lblFindWhat))]
	[MemberNotNull(nameof(bdrFindWhat))]
	[MemberNotNull(nameof(txtFindWhat))]
	[MemberNotNull(nameof(lblChangeTo))]
	[MemberNotNull(nameof(bdrChangeTo))]
	[MemberNotNull(nameof(txtChangeTo))]
	[MemberNotNull(nameof(chkMatchUpperLowercase))]
	[MemberNotNull(nameof(lblMatchUpperLowercaseLabel))]
	[MemberNotNull(nameof(chkWholeWord))]
	[MemberNotNull(nameof(lblWholeWordLabel))]
	[MemberNotNull(nameof(bdrSearch))]
	[MemberNotNull(nameof(cnvSearch))]
	[MemberNotNull(nameof(optSearchActiveWindow))]
	[MemberNotNull(nameof(lblSearchActiveWindowLabel))]
	[MemberNotNull(nameof(optSearchCurrentModule))]
	[MemberNotNull(nameof(lblSearchCurrentModuleLabel))]
	[MemberNotNull(nameof(optSearchAllModules))]
	[MemberNotNull(nameof(lblSearchAllModulesLabel))]
	[MemberNotNull(nameof(optSearchHelpFile))]
	[MemberNotNull(nameof(lblSearchHelpFileLabel))]
	void InitializeComponent(int width)
	{
		Width = width;
		Height = 16;

		lblFindWhat = new Label();
		bdrFindWhat = new Border();
		txtFindWhat = new TextInput();
		lblChangeTo = new Label();
		bdrChangeTo = new Border();
		txtChangeTo = new TextInput();
		chkMatchUpperLowercase = new CheckBox();
		lblMatchUpperLowercaseLabel = new Label();
		chkWholeWord = new CheckBox();
		lblWholeWordLabel = new Label();
		bdrSearch = new Border();
		cnvSearch = new Canvas();
		optSearchActiveWindow = new RadioButton();
		lblSearchActiveWindowLabel = new Label();
		optSearchCurrentModule = new RadioButton();
		lblSearchCurrentModuleLabel = new Label();
		optSearchAllModules = new RadioButton();
		lblSearchAllModulesLabel = new Label();
		optSearchHelpFile = new RadioButton();
		lblSearchHelpFileLabel = new Label();

		var scopeGroup = new RadioButtonGroup() { optSearchActiveWindow, optSearchCurrentModule, optSearchAllModules };

		lblFindWhat.X = 1;
		lblFindWhat.Y = 1;
		lblFindWhat.Text = "Find What:";
		lblFindWhat.AccessKeyIndex = 0;
		lblFindWhat.FocusTarget = bdrFindWhat;
		lblFindWhat.AutoSize();

		txtFindWhat.X = 14;
		txtFindWhat.Y = 1;
		txtFindWhat.Width = 41;
		txtFindWhat.Height = 1;
		txtFindWhat.GotFocus = txtFindWhat_GotFocus;

		bdrFindWhat.Enclose(txtFindWhat);

		lblChangeTo.X = 1;
		lblChangeTo.Y = 4;
		lblChangeTo.Text = "Change To:";
		lblChangeTo.AccessKeyIndex = 7;
		lblChangeTo.FocusTarget = bdrChangeTo;
		lblChangeTo.AutoSize();

		txtChangeTo.X = 14;
		txtChangeTo.Y = 4;
		txtChangeTo.Width = 41;
		txtChangeTo.Height = 1;
		txtChangeTo.GotFocus = txtChangeTo_GotFocus;

		bdrChangeTo.Enclose(txtChangeTo);

		chkMatchUpperLowercase.X = 1;
		chkMatchUpperLowercase.Y = 8;

		lblMatchUpperLowercaseLabel.X = 5;
		lblMatchUpperLowercaseLabel.Y = 8;
		lblMatchUpperLowercaseLabel.Text = "Match Upper/Lowercase";
		lblMatchUpperLowercaseLabel.AccessKeyIndex = 0;
		lblMatchUpperLowercaseLabel.FocusTarget = chkMatchUpperLowercase;
		lblMatchUpperLowercaseLabel.AutoSize();

		chkWholeWord.X = 1;
		chkWholeWord.Y = 9;

		lblWholeWordLabel.X = 5;
		lblWholeWordLabel.Y = 9;
		lblWholeWordLabel.Text = "Whole Word";
		lblWholeWordLabel.AccessKeyIndex = 0;
		lblWholeWordLabel.FocusTarget = chkWholeWord;
		lblWholeWordLabel.AutoSize();

		int searchFrameX = width - 28;

		bdrSearch.X = searchFrameX;
		bdrSearch.Y = 7;
		bdrSearch.Width = 25;
		bdrSearch.Height = 5;
		bdrSearch.Title = "Search";
		bdrSearch.Child = cnvSearch;
		bdrSearch.IsTabStop = true;

		cnvSearch.X = searchFrameX + 1;
		cnvSearch.Y = 8;
		cnvSearch.Width = 23;
		cnvSearch.Height = 3;

		if (SearchScopeMode == SearchScopeMode.TextEditor)
		{
			cnvSearch.Children.Add(optSearchActiveWindow);
			cnvSearch.Children.Add(lblSearchActiveWindowLabel);
			cnvSearch.Children.Add(optSearchCurrentModule);
			cnvSearch.Children.Add(lblSearchCurrentModuleLabel);
			cnvSearch.Children.Add(optSearchAllModules);
			cnvSearch.Children.Add(lblSearchAllModulesLabel);
		}
		else
		{
			cnvSearch.Children.Add(optSearchActiveWindow);
			cnvSearch.Children.Add(lblSearchActiveWindowLabel);
			cnvSearch.Children.Add(optSearchHelpFile);
			cnvSearch.Children.Add(lblSearchHelpFileLabel);
		}

		optSearchActiveWindow.X = searchFrameX + 2;
		optSearchActiveWindow.Y = 8;
		optSearchActiveWindow.RadioButtonGroup = scopeGroup;

		lblSearchActiveWindowLabel.X = searchFrameX + 6;
		lblSearchActiveWindowLabel.Y = 8;
		lblSearchActiveWindowLabel.Text = "1. Active Window";
		lblSearchActiveWindowLabel.AccessKeyIndex = 0;
		lblSearchActiveWindowLabel.FocusTarget = optSearchActiveWindow;
		lblSearchActiveWindowLabel.AutoSize();

		optSearchCurrentModule.X = searchFrameX + 2;
		optSearchCurrentModule.Y = 9;
		optSearchCurrentModule.RadioButtonGroup = scopeGroup;

		lblSearchCurrentModuleLabel.X = searchFrameX + 6;
		lblSearchCurrentModuleLabel.Y = 9;
		lblSearchCurrentModuleLabel.Text = "2. Current Module";
		lblSearchCurrentModuleLabel.AccessKeyIndex = 0;
		lblSearchCurrentModuleLabel.FocusTarget = optSearchCurrentModule;
		lblSearchCurrentModuleLabel.AutoSize();

		optSearchAllModules.X = searchFrameX + 2;
		optSearchAllModules.Y = 10;
		optSearchAllModules.RadioButtonGroup = scopeGroup;

		lblSearchAllModulesLabel.X = searchFrameX + 6;
		lblSearchAllModulesLabel.Y = 10;
		lblSearchAllModulesLabel.Text = "3. All Modules";
		lblSearchAllModulesLabel.AccessKeyIndex = 0;
		lblSearchAllModulesLabel.FocusTarget = optSearchAllModules;
		lblSearchAllModulesLabel.AutoSize();

		optSearchHelpFile.X = searchFrameX + 2;
		optSearchHelpFile.Y = 9;
		optSearchHelpFile.RadioButtonGroup = scopeGroup;

		lblSearchHelpFileLabel.X = searchFrameX + 6;
		lblSearchHelpFileLabel.Y = 9;
		lblSearchHelpFileLabel.Text = "2. Help File";
		lblSearchHelpFileLabel.AccessKeyIndex = 0;
		lblSearchHelpFileLabel.FocusTarget = optSearchHelpFile;
		lblSearchHelpFileLabel.AutoSize();

		var widgets = new List<Widget>();

		widgets.Add(lblFindWhat);
		widgets.Add(bdrFindWhat);
		widgets.Add(lblChangeTo);
		widgets.Add(bdrChangeTo);
		widgets.Add(chkMatchUpperLowercase);
		widgets.Add(lblMatchUpperLowercaseLabel);
		widgets.Add(chkWholeWord);
		widgets.Add(lblWholeWordLabel);
		widgets.Add(bdrSearch);

		ConfigureDialog(widgets);

		AddDialogButtons(widgets);

		Widgets.AddRange(widgets);

		SetFocus(bdrFindWhat);
	}

	private void txtFindWhat_GotFocus()
	{
		txtFindWhat.SelectAll();
	}

	private void txtChangeTo_GotFocus()
	{
		txtChangeTo.SelectAll();
	}
}
