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
	MenuItem mnuRunStart;
	MenuItem mnuRunRestart;
	MenuItem mnuRunContinue;
	MenuItem mnuRunModifyCommandLine;
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

	const string CallsMenuItemReferenceBarText = "Display next statement to be executed in module or procedure";

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
		nameof(mnuRunStart),
		nameof(mnuRunRestart),
		nameof(mnuRunContinue),
		nameof(mnuRunModifyCommandLine),
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
				(mnuFileNew = new MenuItem("&New Program", "Remove currently loaded program from memory", "-324")),
				(mnuFileOpenProgram = new MenuItem("&Open Program...", "Load new program into memory", "-325")),
				new MenuItem("&Merge...", "Insert specified file into current module", "-326"),
				(mnuFileSave = new MenuItem("&Save", "Write current module to file on disk", "-327")),
				(mnuFileSaveAs = new MenuItem("Save &As...", "Save current module with specified name and format", "-328")),
				(mnuFileSaveAll = new MenuItem("Sa&ve All", "Write all currently loaded modules to files on disk", "-329")),
				MenuItem.Separator,
				(mnuFileCreateFile = new MenuItem("&Create File...", "Create a module, include file, or document; retain loaded modules", "-330")),
				(mnuFileLoadFile = new MenuItem("&Load File...", "Load a module, include file, or document; retain loaded modules", "-331")),
				new MenuItem("&Unload File...", "Remove a loaded module, include file, or document from memory", "-332"),
				MenuItem.Separator,
				new MenuItem("&Print...", "Send specified text or module to printer or file", "-333"),
				new MenuItem("&DOS Shell", "Temporary suspend QBX and invoke DOS shell", "-334"),
				MenuItem.Separator,
				(mnuFileExit = new MenuItem("E&xit", "Exit QBX and return to DOS", "-335")),
			};

		mnuEdit =
			new Menu("&Edit", 17, "m.e")
			{
				new MenuItem("&Undo     Alt+Bksp", "Undo last edit action", "-336") { IsEnabled = false },
				new MenuItem("&Redo    Ctrl+Bksp", "Redo last edit action that was undone", "-337") { IsEnabled = false },
				new MenuItem("Cu&t     Shift+Del", "Delete selected text and copy it to buffer", "-338") { IsEnabled = false },
				new MenuItem("&Copy     Ctrl+Ins", "Copy selected text to buffer", "-339") { IsEnabled = false },
				new MenuItem("&Paste   Shift+Ins", "Insert text from buffer at current location", "-341") { IsEnabled = false },
				new MenuItem("Cl&ear         Del", "Delete selected text without copying it to buffer", "-340") { IsEnabled = false },
				MenuItem.Separator,
				new MenuItem("New &SUB...", "Open a window for a new SUB", "-342"),
				new MenuItem("New &FUNCTION...", "Open a window for a new FUNCTION procedure", "-343"),
			};

		mnuView =
			new Menu("&View", 21, "m.v")
			{
				new MenuItem("&SUBs...            F2", "Display a loaded SUB, FUNCTION, module, include file, or document", "-344"),
				new MenuItem("N&ext SUB     Shift+F2", "Display next SUB or FUNCTION procedure in the active window", "-345"),
				(mnuViewSplit = new MenuItem("S&plit", "Divide screen into two View windows", "-346")),
				MenuItem.Separator,
				new MenuItem("&Next Statement", "Display next statement to be executed", "-347"),
				new MenuItem("O&utput Screen      F4", "Display output screen", "-348"),
				MenuItem.Separator,
				new MenuItem("&Included File", "Display include file for editing", "-349") { IsEnabled = false },
				new MenuItem("Included &Lines", "Display include file for viewing only (not for editing)", "-350"),
			};

		mnuSearch =
			new Menu("&Search", 24, "m.s")
			{
				new MenuItem("&Find...", "Find specified text", "-351"),
				new MenuItem("&Selected Text     Ctrl+\\", "Find selected text", "-352"),
				new MenuItem("&Repeat Last Find      F3", "Repeat last find", "-353"),
				new MenuItem("&Change...", "Find and change specified text", "-354"),
				new MenuItem("&Label...", "Find specified line label", "-355"),
			};

		mnuRun =
			new Menu("&Run", 19, "m.r")
			{
				(mnuRunStart = new MenuItem("&Start      Shift+F5", "Run current program", "-356")),
				(mnuRunRestart = new MenuItem("&Restart", "Clear variables in preparation for restarting single stepping", "-357")),
				(mnuRunContinue = new MenuItem("Co&ntinue         F5", "Continue execution after a break", "-358")),
				(mnuRunModifyCommandLine = new MenuItem("Modify &COMMAND$...", "Set string returned by COMMAND$ function", "-359")),
				MenuItem.Separator,
				new MenuItem("Make E&XE File...", "Create executable file on disk", "-360") { IsEnabled = false },
				new MenuItem("Make &Library...", "Create Quick Library and stand-alone library (.LIB) on disk", "-361") { IsEnabled = false },
				MenuItem.Separator,
				(mnuRunSetMainModule = new MenuItem("Set &Main Module...", "Make the specified module the main module", "-362")),
			};

		mnuDebug =
			new Menu("&Debug", 27, "m.d")
			{
				(mnuDebugAddWatch = new MenuItem("&Add Watch...", "Add specified expression to the Watch window", "-363")),
				(mnuDebugInstantWatch = new MenuItem("&Instant Watch...   Shift+F9", "Display the value of a variable or expression", "-364")),
				(mnuDebugWatchpoint = new MenuItem("&Watchpoint...", "Cause program to stop when specified expression is TRUE", "-365")),
				(mnuDebugDeleteWatch = new MenuItem("&Delete Watch...", "Delete specified entry from Watch window", "-366") { IsEnabled = false }),
				(mnuDebugDeleteAllWatch = new MenuItem("De&lete All Watch", "Delete all Watch window entries", "-367") { IsEnabled = false }),
				MenuItem.Separator,
				new MenuItem("&Trace On", "Highlight statement that is executing", "-368"),
				new MenuItem("&History On", "Record order of statement execution", "-369"),
				MenuItem.Separator,
				new MenuItem("Toggle &Breakpoint        F9", "Set or clear breakpoint at cursor location", "-370"),
				new MenuItem("&Clear All Breakpoints", "Remove all breakpoints", "-371"),
				new MenuItem("Break on &Errors", "Stop execution at first statement in error handler", "-372"),
				new MenuItem("&Set Next Statement", "Indicate next statement to be executed", "-373") { IsEnabled = false },
			};

		mnuCalls =
			new Menu("&Calls", 15, "m.c")
			{
				// Dynamically populated
			};

		mnuUtility =
			new Menu("&Utility", 18, "m.u")
			{
				new MenuItem("&Run DOS Command...", "Enter and run a DOS command", "-374"),
				new MenuItem("&Customize Menu...", "Create or edit a Utility menu command", "-375"),
			};

		mnuOptions =
			new Menu("&Options", 22, "m.o")
			{
				(mnuOptionsDisplay = new MenuItem("&Display...", "Change display attributes", "-384")),
				new MenuItem("Set &Paths...", "Set default search paths", "-385"),
				new MenuItem("Right &Mouse...", "Change action of right mouse click", "-386"),
				new MenuItem("&Syntax Checking", "Turn syntax checking on or off", "-387") { IsChecked = true },
				(mnuOptionsDetectDelayLoops = new MenuItem("Detect Delay &Loops", "QBX: Sleep thread for empty FOR loops") { IsChecked = DetectDelayLoops }),
				MenuItem.Separator,
				(mnuOptionsEventsEveryStatement = new MenuItem("Events Every S&tatement", "QBX: Check for events after every statement") { IsChecked = (EventCheckGranularity == EventCheckGranularity.EveryStatement) }),
				(mnuOptionsEventsOnLabels = new MenuItem("Events On &Labels", "QBX: Check for events at labels") { IsChecked = (EventCheckGranularity == EventCheckGranularity.EveryLabel) }),
			};

		mnuHelp =
			new Menu("&Help", 25, "m.h")
			{
				(mnuHelpIndex = new MenuItem("&Index", "Display index for online Help", "-389")),
				(mnuHelpContents = new MenuItem("&Contents", "Display table of contents for online Help", "-390")),
				// TODO: Topic should update dynamically when the menu is opened
				(mnuHelpTopic = new MenuItem("&Topic:                 F1", "Display information about the BASIC keyword the cursor is on", "-391") { IsEnabled = false }),
				(mnuHelpUsingHelp = new MenuItem("Using &Help       Shift+F1", "Display information about online Help", "-392")),
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

		mnuRunStart.Clicked += mnuRunStart_Clicked;
		mnuRunRestart.Clicked += mnuRunRestart_Clicked;
		mnuRunContinue.Clicked += mnuRunContinue_Clicked;
		mnuRunModifyCommandLine.Clicked += mnuRunModifyCommandLine_Clicked;
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
		CommitViewportsAndSwallowError();
		PromptToSaveChanges(StartNewProgram);
	}

	private void mnuFileSave_Clicked()
	{
		if (FocusedViewport?.EditableUnit is CompilationUnit unit)
		{
			CommitViewportsAndSwallowError();
			InteractiveSaveIfUnitHasNoFilePath(unit);
		}
	}

	private void mnuFileSaveAs_Clicked()
	{
		if (FocusedViewport?.EditableUnit is CompilationUnit unit)
		{
			CommitViewportsAndSwallowError();
			InteractiveSave(unit, title: DevelopmentEnvironment.Dialogs.SaveFileDialogTitle.SaveAs);
		}
	}

	private void mnuFileSaveAll_Clicked()
	{
		CommitViewportsAndSwallowError();
		SaveAll();
	}

	private void mnuFileOpenProgram_Clicked()
	{
		CommitViewportsAndSwallowError();
		ShowOpenFileDialog(replaceExistingProgram: true);
	}

	private void mnuFileCreateFile_Clicked()
	{
		CommitViewportsAndSwallowError();
		ShowCreateFileDialog();
	}

	private void mnuFileLoadFile_Clicked()
	{
		CommitViewportsAndSwallowError();
		ShowOpenFileDialog(replaceExistingProgram: false);
	}

	void mnuFileExit_Clicked()
	{
		ExitWithSavePrompt();
	}

	private void mnuViewSplit_Clicked()
	{
		ShowSplitViewport();
	}

	private void mnuRunStart_Clicked()
	{
		Run();
	}

	private void mnuRunRestart_Clicked()
	{
		Restart();
		UpdateAfterBreak();
	}

	private void mnuRunContinue_Clicked()
	{
		Continue();
	}

	private void mnuRunModifyCommandLine_Clicked()
	{
		var dialog = new ModifyCommandLineDialog(Machine, Configuration);

		dialog.CommandLine = ProgramCommandLine;

		dialog.UpdateCommandLine +=
			() =>
			{
				ProgramCommandLine = dialog.CommandLine;
			};

		ShowDialog(dialog);
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
		ShowUsingHelpTopic();
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
					case AltRelease.ActivateMenuBar:
						if (input.IsKeyPad) // Alt-NumPad character entry.
							Mode = UIMode.TextEditor;
						else
							SelectedMenu = 0;
						break;
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
					if (SelectedMenu < 0)
						break;

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
			new MenuItem("&" + routineName, CallsMenuItemReferenceBarText) // TODO: handling for duplicate access keys
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
}
