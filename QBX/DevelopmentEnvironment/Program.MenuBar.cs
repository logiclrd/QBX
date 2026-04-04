using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.CodeModel;
using QBX.DevelopmentEnvironment.Dialogs;
using QBX.ExecutionEngine.Execution.Events;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	MenuBar MenuBar;

	Menu mnuFile;
	MenuItem mnuFileNew;
	MenuItem mnuFileSave;
	MenuItem mnuFileSaveAs;
	MenuItem mnuFileSaveAll;
	MenuItem mnuFileOpenProgram;
	MenuItem mnuFileCreateFile;
	MenuItem mnuFileLoadFile;
	MenuItem mnuFileExit;

	Menu mnuEdit;

	Menu mnuView;
	MenuItem mnuViewSplit;

	Menu mnuSearch;

	Menu mnuRun;
	MenuItem mnuRunSetMainModule;

	Menu mnuDebug;
	MenuItem mnuDebugAddWatch;
	MenuItem mnuDebugInstantWatch;
	MenuItem mnuDebugWatchpoint;
	MenuItem mnuDebugDeleteWatch;
	MenuItem mnuDebugDeleteAllWatch;

	Menu mnuCalls;

	Menu mnuUtility;

	Menu mnuOptions;
	MenuItem mnuOptionsDisplay;
	MenuItem mnuOptionsDetectDelayLoops;
	MenuItem mnuOptionsEventsEveryStatement;
	MenuItem mnuOptionsEventsOnLabels;

	Menu mnuHelp;
	MenuItem mnuHelpIndex;
	MenuItem mnuHelpContents;
	MenuItem mnuHelpTopic;
	MenuItem mnuHelpUsingHelp;

	public int SelectedMenu = -1;
	public int SelectedMenuItem = -1;
	public AltRelease AltReleaseAction;

	public enum AltRelease
	{
		DeactivateMenuBar,
		CloseMenu,
		ActivateMenuBar,
		Ignore,
	}

	[MemberNotNull(
		nameof(MenuBar),
		nameof(mnuFile),
		nameof(mnuFileNew),
		nameof(mnuFileSave),
		nameof(mnuFileSaveAs),
		nameof(mnuFileSaveAll),
		nameof(mnuFileOpenProgram),
		nameof(mnuFileCreateFile),
		nameof(mnuFileLoadFile),
		nameof(mnuFileExit),
		nameof(mnuEdit),
		nameof(mnuView),
		nameof(mnuViewSplit),
		nameof(mnuSearch),
		nameof(mnuRun),
		nameof(mnuRunSetMainModule),
		nameof(mnuDebug),
		nameof(mnuDebugAddWatch),
		nameof(mnuDebugInstantWatch),
		nameof(mnuDebugWatchpoint),
		nameof(mnuDebugDeleteWatch),
		nameof(mnuDebugDeleteAllWatch),
		nameof(mnuCalls),
		nameof(mnuUtility),
		nameof(mnuOptions),
		nameof(mnuOptionsDisplay),
		nameof(mnuOptionsDetectDelayLoops),
		nameof(mnuOptionsEventsEveryStatement),
		nameof(mnuOptionsEventsOnLabels),
		nameof(mnuHelp),
		nameof(mnuHelpIndex),
		nameof(mnuHelpContents),
		nameof(mnuHelpTopic),
		nameof(mnuHelpUsingHelp))]
	void InitializeMenuBar()
	{
		mnuFile =
			new Menu("&File", 16, "m.f")
			{
				(mnuFileNew = new MenuItem("&New Program", "-324")),
				(mnuFileOpenProgram = new MenuItem("&Open Program...", "-325")),
				new MenuItem("&Merge...", "-326"),
				(mnuFileSave = new MenuItem("&Save", "-327")),
				(mnuFileSaveAs = new MenuItem("Save &As...", "-328")),
				(mnuFileSaveAll = new MenuItem("Sa&ve All", "-329")),
				MenuItem.Separator,
				(mnuFileCreateFile = new MenuItem("&Create File...", "-330")),
				(mnuFileLoadFile = new MenuItem("&Load File...", "-331")),
				new MenuItem("&Unload File...", "-332"),
				MenuItem.Separator,
				new MenuItem("&Print...", "-333"),
				new MenuItem("&DOS Shell", "-334"),
				MenuItem.Separator,
				(mnuFileExit = new MenuItem("E&xit", "-335")),
			};

		mnuEdit =
			new Menu("&Edit", 17, "m.e")
			{
				new MenuItem("&Undo     Alt+Bksp", "-336") { IsEnabled = false },
				new MenuItem("&Redo    Ctrl+Bksp", "-337") { IsEnabled = false },
				new MenuItem("Cu&t     Shift+Del", "-338") { IsEnabled = false },
				new MenuItem("&Copy     Ctrl+Ins", "-339") { IsEnabled = false },
				new MenuItem("&Paste   Shift+Ins", "-341") { IsEnabled = false },
				new MenuItem("Cl&ear         Del", "-340") { IsEnabled = false },
				MenuItem.Separator,
				new MenuItem("New &SUB...", "-342"),
				new MenuItem("New &FUNCTION...", "-343"),
			};

		mnuView =
			new Menu("&View", 21, "m.v")
			{
				new MenuItem("&SUBs...            F2", "-344"),
				new MenuItem("N&ext SUB     Shift+F2", "-345"),
				(mnuViewSplit = new MenuItem("S&plit", "-346")),
				MenuItem.Separator,
				new MenuItem("&Next Statement", "-347"),
				new MenuItem("O&utput Screen      F4", "-348"),
				MenuItem.Separator,
				new MenuItem("&Included File", "-349") { IsEnabled = false },
				new MenuItem("Included &Lines", "-350"),
			};

		mnuSearch =
			new Menu("&Search", 24, "m.s")
			{
				new MenuItem("&Find...", "-351"),
				new MenuItem("&Selected Text     Ctrl+\\", "-352"),
				new MenuItem("&Repeat Last Find      F3", "-353"),
				new MenuItem("&Change...", "-354"),
				new MenuItem("&Label...", "-355"),
			};

		mnuRun =
			new Menu("&Run", 19, "m.r")
			{
				new MenuItem("&Start      Shift+F5", "-356"),
				new MenuItem("&Restart", "-357"),
				new MenuItem("Co&ntinue         F5", "-358"),
				new MenuItem("Modify &COMMAND$...", "-359"),
				MenuItem.Separator,
				new MenuItem("Make E&XE File...", "-360"),
				new MenuItem("Make &Library...", "-361"),
				MenuItem.Separator,
				(mnuRunSetMainModule = new MenuItem("Set &Main Module...", "-362")),
			};

		mnuDebug =
			new Menu("&Debug", 27, "m.d")
			{
				(mnuDebugAddWatch = new MenuItem("&Add Watch...", "-363")),
				(mnuDebugInstantWatch = new MenuItem("&Instant Watch...   Shift+F9", "-364")),
				(mnuDebugWatchpoint = new MenuItem("&Watchpoint...", "-365")),
				(mnuDebugDeleteWatch = new MenuItem("&Delete Watch...", "-366") { IsEnabled = false }),
				(mnuDebugDeleteAllWatch = new MenuItem("De&lete All Watch", "-367") { IsEnabled = false }),
				MenuItem.Separator,
				new MenuItem("&Trace On", "-368"),
				new MenuItem("&History On", "-369"),
				MenuItem.Separator,
				new MenuItem("Toggle &Breakpoint        F9", "-370"),
				new MenuItem("&Clear All Breakpoints", "-371"),
				new MenuItem("Break on &Errors", "-372"),
				new MenuItem("&Set Next Statement", "-373") { IsEnabled = false },
			};

		mnuCalls =
			new Menu("&Calls", 15, "m.c")
			{
				// Dynamically populated
			};

		mnuUtility =
			new Menu("&Utility", 18, "m.u")
			{
				new MenuItem("&Run DOS Command...", "-374"),
				new MenuItem("&Customize Menu...", "-375"),
			};

		mnuOptions =
			new Menu("&Options", 22, "m.o")
			{
				(mnuOptionsDisplay = new MenuItem("&Display...", "-384")),
				new MenuItem("Set &Paths...", "-385"),
				new MenuItem("Right &Mouse...", "-386"),
				new MenuItem("&Syntax Checking", "-387") { IsChecked = true },
				(mnuOptionsDetectDelayLoops = new MenuItem("Detect Delay &Loops") { IsChecked = DetectDelayLoops }),
				MenuItem.Separator,
				(mnuOptionsEventsEveryStatement = new MenuItem("Events Every S&tatement") { IsChecked = (EventCheckGranularity == EventCheckGranularity.EveryStatement) }),
				(mnuOptionsEventsOnLabels = new MenuItem("Events On &Labels") { IsChecked = (EventCheckGranularity == EventCheckGranularity.EveryLabel) }),
			};

		mnuHelp =
			new Menu("&Help", 25, "m.h")
			{
				(mnuHelpIndex = new MenuItem("&Index", "-389")),
				(mnuHelpContents = new MenuItem("&Contents", "-390")),
				(mnuHelpTopic = new MenuItem("&Topic:                 F1", "-391") { IsEnabled = false }),
				(mnuHelpUsingHelp = new MenuItem("Using &Help       Shift+F1", "-392")),
			};

		MenuBar =
			new MenuBar()
			{
				mnuFile,
				mnuEdit,
				mnuView,
				mnuSearch,
				mnuRun,
				mnuDebug,
				mnuCalls,
				mnuUtility,
				mnuOptions,
				mnuHelp
			};

		mnuFileNew.Clicked = mnuFileNew_Clicked;
		mnuFileSave.Clicked = mnuFileSave_Clicked;
		mnuFileSaveAs.Clicked = mnuFileSaveAs_Clicked;
		mnuFileSaveAll.Clicked = mnuFileSaveAll_Clicked;
		mnuFileOpenProgram.Clicked = mnuFileOpenProgram_Clicked;
		mnuFileCreateFile.Clicked = mnuFileCreateFile_Clicked;
		mnuFileLoadFile.Clicked = mnuFileLoadFile_Clicked;
		mnuFileExit.Clicked += mnuFileExit_Clicked;

		mnuViewSplit.Clicked += mnuViewSplit_Clicked;

		mnuRunSetMainModule.Clicked += mnuRunSetMainModule_Clicked;

		mnuDebugAddWatch.Clicked += mnuDebugAddWatch_Clicked;
		mnuDebugInstantWatch.Clicked = mnuDebugInstantWatch_Clicked;
		mnuDebugWatchpoint.Clicked = mnuDebugWatchpoint_Clicked;
		mnuDebugDeleteAllWatch.Clicked = mnuDebugDeleteAllWatch_Clicked;

		mnuOptionsDisplay.Clicked = mnuOptionsDisplay_Clicked;
		mnuOptionsDetectDelayLoops.Clicked = mnuOptionsDetectDelayLoops_Clicked;
		mnuOptionsEventsEveryStatement.Clicked = mnuOptionsEventsEveryStatement_Clicked;
		mnuOptionsEventsOnLabels.Clicked = mnuOptionsEventsOnLabels_Clicked;

		mnuHelpIndex.Clicked = mnuHelpIndex_Clicked;
		mnuHelpContents.Clicked = mnuHelpContent_Clicked;
		mnuHelpTopic.Clicked = mnuHelpTopic_Clicked;
		mnuHelpUsingHelp.Clicked = mnuHelpUsingHelp_Clicked;
	}

	private void mnuFileNew_Clicked()
	{
		if (CommitViewportsOrPresentError())
			PromptToSaveChanges(StartNewProgram);
	}

	private void mnuFileSave_Clicked()
	{
		if (FocusedViewport?.EditableUnit is CompilationUnit unit)
		{
			if (CommitViewportsOrPresentError())
				InteractiveSaveIfUnitHasNoFilePath(unit);
		}
	}

	private void mnuFileSaveAs_Clicked()
	{
		if (FocusedViewport?.EditableUnit is CompilationUnit unit)
		{
			if (CommitViewportsOrPresentError())
				InteractiveSave(unit, title: DevelopmentEnvironment.Dialogs.SaveFileDialogTitle.SaveAs);
		}
	}

	private void mnuFileSaveAll_Clicked()
	{
		if (CommitViewportsOrPresentError())
			SaveAll();
	}

	private void mnuFileOpenProgram_Clicked()
	{
		if (CommitViewportsOrPresentError())
			ShowOpenFileDialog(replaceExistingProgram: true);
	}

	private void mnuFileCreateFile_Clicked()
	{
		if (CommitViewportsOrPresentError())
			ShowCreateFileDialog();
	}

	private void mnuFileLoadFile_Clicked()
	{
		if (CommitViewportsOrPresentError())
			ShowOpenFileDialog(replaceExistingProgram: false);
	}

	private void mnuViewSplit_Clicked()
	{
		ShowSplitViewport();
	}

	private void mnuRunSetMainModule_Clicked()
	{
		SetMainModule();
	}

	private void mnuDebugAddWatch_Clicked()
	{
		InteractiveAddWatch();
	}

	private void mnuDebugInstantWatch_Clicked()
	{
		InstantWatchAtCurrentCursorLocation();
	}

	private void mnuDebugWatchpoint_Clicked()
	{
		InteractiveAddWatchpoint();
	}

	private void mnuDebugDeleteAllWatch_Clicked()
	{
		ClearWatches();
	}

	private void mnuOptionsDisplay_Clicked()
	{
		ShowDialog(new DisplayDialog(Machine, Configuration));
	}

	private void mnuOptionsDetectDelayLoops_Clicked()
	{
		DetectDelayLoops = !DetectDelayLoops;

		mnuOptionsDetectDelayLoops.IsChecked = DetectDelayLoops;
	}

	void UpdateEventsItems()
	{
		mnuOptionsEventsEveryStatement.IsChecked = (EventCheckGranularity == EventCheckGranularity.EveryStatement);
		mnuOptionsEventsOnLabels.IsChecked = (EventCheckGranularity == EventCheckGranularity.EveryLabel);
	}

	private void mnuOptionsEventsEveryStatement_Clicked()
	{
		EventCheckGranularity = EventCheckGranularity.EveryStatement;
		UpdateEventsItems();
	}

	private void mnuOptionsEventsOnLabels_Clicked()
	{
		EventCheckGranularity = EventCheckGranularity.EveryLabel;
		UpdateEventsItems();
	}

	private void mnuHelpIndex_Clicked()
	{
		ShowHelpTopic("bas7qck.hlp!blang.index");
	}

	private void mnuHelpContent_Clicked()
	{
		ShowHelpTopic("bas7qck.hlp!blang.contents");
	}

	private void mnuHelpTopic_Clicked()
	{
		TryShowHelpTopicForTokenUnderCursor();
	}

	private void mnuHelpUsingHelp_Clicked()
	{
		ShowHelpTopic("bas7qck.hlp!h.default");
	}

	bool ActivateMenuItem(MenuItem item)
	{
		if (!item.IsEnabled)
			return false;

		item.Clicked?.Invoke();

		return true;
	}

	void ProcessMenuBarKey(KeyEvent input)
	{
		if (input.IsRelease)
		{
			if (input.ScanCode == ScanCode.Alt)
			{
				switch (AltReleaseAction)
				{
					case AltRelease.Ignore: break;
					case AltRelease.ActivateMenuBar: SelectedMenu = 0; break;
					case AltRelease.DeactivateMenuBar: Mode = UIMode.TextEditor; break;
				}

				AltReleaseAction = AltRelease.DeactivateMenuBar;
			}
		}
		else
		{
			switch (input.ScanCode)
			{
				case ScanCode.F1:
					if (SelectedMenu < 0)
						TryShowHelpTopicForTokenUnderCursor();
					else if (MenuBar[SelectedMenu].HelpContextString != null)
						ShowHelpTopicPopup(EnvironmentHelpFilePrefix + MenuBar[SelectedMenu].HelpContextString);

					break;

				case ScanCode.Alt:
					AltReleaseAction = AltRelease.DeactivateMenuBar;
					break;

				case ScanCode.Escape:
					if (input.Modifiers.AltKey)
						AltReleaseAction = AltRelease.DeactivateMenuBar;
					else
						Mode = UIMode.TextEditor;
					break;
				case ScanCode.Return:
				case ScanCode.Up:
				case ScanCode.Down:
					Mode = UIMode.Menu;
					SelectedMenuItem = 0;
					break;
				case ScanCode.Left:
				case ScanCode.Right:
				{
					SelectedMenu = (SelectedMenu + MenuBar.Count +
						(input.ScanCode == ScanCode.Left ? -1 : +1)) % MenuBar.Count;
					break;
				}
				default:
				{
					string inkey = "";

					if (!input.Modifiers.AltKey)
						inkey = input.ToInKeyString();
					else
						inkey = input.ScanCode.ToCharacterString();

					if (!string.IsNullOrEmpty(inkey))
					{
						MenuBar.EnsureAccessKeyLookUp();

						if (MenuBar.ItemByAccessKey.TryGetValue(inkey, out var menu))
						{
							Mode = UIMode.Menu;
							SelectedMenu = MenuBar.Items.IndexOf(menu);
							SelectedMenuItem = 0;
							AltReleaseAction = AltRelease.Ignore;
						}
						else if ((SelectedMenu < 0) && input.Modifiers.AltGrKey)
						{
							AltReleaseAction = AltRelease.DeactivateMenuBar;
							ProcessTextEditorKey(input);
						}
					}

					break;
				}
			}
		}
	}

	void ProcessMenuKey(KeyEvent input)
	{
		if (input.IsRelease)
		{
			if (input.ScanCode == ScanCode.Alt)
			{
				switch (AltReleaseAction)
				{
					case AltRelease.Ignore: break;
					case AltRelease.ActivateMenuBar: SelectedMenu = 0; break;
					case AltRelease.CloseMenu: Mode = UIMode.MenuBar; break;
					case AltRelease.DeactivateMenuBar: Mode = UIMode.TextEditor; break;
				}

				AltReleaseAction = AltRelease.CloseMenu;
			}
		}
		else
		{
			switch (input.ScanCode)
			{
				case ScanCode.F1:
					if ((SelectedMenu >= 0)
					 && (SelectedMenu < MenuBar.Count)
					 && (SelectedMenuItem >= 0)
					 && (SelectedMenuItem < MenuBar[SelectedMenu].Items.Count))
					{
						var menuItem = MenuBar[SelectedMenu].Items[SelectedMenuItem];

						if (menuItem.HelpContextString != null)
							ShowHelpTopicPopup(EnvironmentHelpFilePrefix + menuItem.HelpContextString);
					}

					break;

				case ScanCode.Alt:
					AltReleaseAction = AltRelease.CloseMenu;
					break;

				case ScanCode.Escape:
					if (!input.Modifiers.AltKey)
						Mode = UIMode.TextEditor;
					break;
				case ScanCode.Return:
					if ((SelectedMenu >= 0)
					 && (SelectedMenu < MenuBar.Count)
					 && ActivateMenuItem(MenuBar[SelectedMenu].Items[SelectedMenuItem]))
						SetUIModeAfterMenuItemActivation();
					break;
				case ScanCode.Left:
				case ScanCode.Right:
				{
					SelectedMenu = (SelectedMenu + MenuBar.Count +
						(input.ScanCode == ScanCode.Left ? -1 : +1)) % MenuBar.Count;
					SelectedMenuItem = 0;
					break;
				}
				case ScanCode.Up:
				case ScanCode.Down:
				{
					var menu = MenuBar[SelectedMenu];

					int delta = input.ScanCode == ScanCode.Down ? 1 : menu.Items.Count - 1;

					do
					{
						SelectedMenuItem = (SelectedMenuItem + delta) % menu.Items.Count;
					} while (menu.Items[SelectedMenuItem].IsSeparator);

					break;
				}
				default:
				{
					string inkey = "";

					if (!input.Modifiers.AltKey)
						inkey = input.ToInKeyString();
					else
						inkey = input.ScanCode.ToCharacterString();

					if (!string.IsNullOrEmpty(inkey))
					{
						var menu = MenuBar[SelectedMenu];

						menu.EnsureAccessKeyLookUp();

						if (menu.ItemByAccessKey.TryGetValue(inkey, out var item))
						{
							SelectedMenuItem = menu.IndexOf(item);

							if (ActivateMenuItem(item))
								SetUIModeAfterMenuItemActivation();
						}
					}

					break;
				}
			}
		}
	}

	void SetUIModeAfterMenuItemActivation()
	{
		if (Dialogs.Count == 0)
			Mode = UIMode.TextEditor;
		else
		{
			Mode = UIMode.MenuBar;

			Dialogs.Last().Closed +=
				(_, _) =>
				{
					Mode = UIMode.TextEditor;
				};
		}
	}

	void ResetCallsMenu()
	{
		mnuCalls.Items.Clear();

		foreach (var editable in LoadedFiles)
		{
			if (editable is CompilationUnit unit)
			{
				PushCall(
					unit.Name,
					unit.Elements[0],
					lineNumber: 0,
					column: 0);

				break;
			}
		}
	}

	void PushCall(string routineName, CodeModel.CompilationElement element, int lineNumber, int column)
	{
		int availableChars = mnuCalls.Width;

		if (routineName.Length > availableChars)
		{
			string diaresis = "...";

			availableChars -= diaresis.Length;

			int left = availableChars / 2;
			int right = availableChars - left;

			routineName =
				routineName.Substring(0, left) +
				diaresis +
				routineName.Substring(routineName.Length - right);
		}

		mnuCalls.Insert(
			0,
			new MenuItem("&" + routineName) // TODO: handling for duplicate access keys
			{
				Clicked =
					() =>
					{
						NavigateTo(element, lineNumber, column);
					}
			});
	}

	void PopCall()
	{
		if (mnuCalls.Count > 1)
			mnuCalls.RemoveAt(0);
	}

	void mnuFileExit_Clicked()
	{
		ExitWithSavePrompt();
	}
}
