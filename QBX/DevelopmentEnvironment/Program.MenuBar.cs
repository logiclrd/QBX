using System.Diagnostics.CodeAnalysis;
using System.Linq;

using QBX.CodeModel;
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
	MenuItem mnuFileLoadFile;

	Menu mnuEdit;

	Menu mnuView;

	Menu mnuSearch;

	Menu mnuRun;

	Menu mnuDebug;
	MenuItem mnuDebugInstantWatch;
	MenuItem mnuDebugDeleteWatch;
	MenuItem mnuDebugDeleteAllWatch;

	Menu mnuCalls;

	Menu mnuUtility;

	Menu mnuOptions;

	Menu mnuHelp;

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
		nameof(mnuFileLoadFile),
		nameof(mnuEdit),
		nameof(mnuView),
		nameof(mnuSearch),
		nameof(mnuRun),
		nameof(mnuDebug),
		nameof(mnuDebugInstantWatch),
		nameof(mnuDebugDeleteWatch),
		nameof(mnuDebugDeleteAllWatch),
		nameof(mnuCalls),
		nameof(mnuUtility),
		nameof(mnuOptions),
		nameof(mnuHelp))]
	void InitializeMenuBar()
	{
		mnuFile =
			new Menu("&File", 16)
			{
				(mnuFileNew = new MenuItem("&New Program")),
				(mnuFileOpenProgram = new MenuItem("&Open Program...")),
				new MenuItem("&Merge..."),
				(mnuFileSave = new MenuItem("&Save")),
				(mnuFileSaveAs = new MenuItem("Save &As...")),
				(mnuFileSaveAll = new MenuItem("Sa&ve All")),
				MenuItem.Separator,
				new MenuItem("&Create File..."),
				(mnuFileLoadFile = new MenuItem("&Load File...")),
				new MenuItem("&Unload File..."),
				MenuItem.Separator,
				new MenuItem("&Print..."),
				new MenuItem("&DOS Shell"),
				MenuItem.Separator,
				new MenuItem("E&xit") { Clicked = mnuFileExit_Clicked },
			};

		mnuEdit =
			new Menu("&Edit", 17)
			{
				new MenuItem("&Undo     Alt+Bksp") { IsEnabled = false },
				new MenuItem("&Redo    Ctrl+Bksp") { IsEnabled = false },
				new MenuItem("Cu&t     Shift+Del") { IsEnabled = false },
				new MenuItem("&Copy     Ctrl+Ins") { IsEnabled = false },
				new MenuItem("&Paste   Shift+Ins") { IsEnabled = false },
				new MenuItem("Cl&ear         Del") { IsEnabled = false },
				MenuItem.Separator,
				new MenuItem("New &SUB..."),
				new MenuItem("New &FUNCTION..."),
			};

		mnuView =
			new Menu("&View", 21)
			{
				new MenuItem("&SUBs...            F2"),
				new MenuItem("N&ext SUB     Shift+F2"),
				new MenuItem("S&plit"),
				MenuItem.Separator,
				new MenuItem("&Next Statement"),
				new MenuItem("O&utput Screen      F4"),
				MenuItem.Separator,
				new MenuItem("&Included File") { IsEnabled = false },
				new MenuItem("Included &Lines"),
			};

		mnuSearch =
			new Menu("&Search", 24)
			{
				new MenuItem("&Find..."),
				new MenuItem("&Selected Text     Ctrl+\\"),
				new MenuItem("&Repeat Last Find      F3"),
				new MenuItem("&Change..."),
				new MenuItem("&Label..."),
			};

		mnuRun =
			new Menu("&Run", 19)
			{
				new MenuItem("&Start      Shift+F5"),
				new MenuItem("&Restart"),
				new MenuItem("Co&ntinue         F5"),
				new MenuItem("Modify &COMMAND$..."),
				MenuItem.Separator,
				new MenuItem("Make E&XE File..."),
				new MenuItem("Make &Library..."),
				MenuItem.Separator,
				new MenuItem("Set &Main Module..."),
			};

		mnuDebug =
			new Menu("&Debug", 27)
			{
				new MenuItem("&Add Watch..."),
				(mnuDebugInstantWatch = new MenuItem("&Instant Watch...   Shift+F9")),
				new MenuItem("&Watchpoint..."),
				(mnuDebugDeleteWatch = new MenuItem("&Delete Watch...") { IsEnabled = false }),
				(mnuDebugDeleteAllWatch = new MenuItem("De&lete All Watch") { IsEnabled = false }),
				MenuItem.Separator,
				new MenuItem("&Trace On"),
				new MenuItem("&History On"),
				MenuItem.Separator,
				new MenuItem("Toggle &Breakpoint        F9"),
				new MenuItem("&Clear All Breakpoints"),
				new MenuItem("Break on &Errors"),
				new MenuItem("&Set Next Statement") { IsEnabled = false },
			};

		mnuCalls =
			new Menu("&Calls", 15)
			{
				// Dynamically populated
			};

		mnuUtility =
			new Menu("&Utility", 18)
			{
				new MenuItem("&Run DOS Command..."),
				new MenuItem("&Customize Menu..."),
			};

		mnuOptions =
			new Menu("&Options", 15)
			{
				new MenuItem("&Display..."),
				new MenuItem("Set &Paths..."),
				new MenuItem("Right &Mouse..."),
				new MenuItem("&Syntax Checking") { IsChecked = true },
			};

		mnuHelp =
			new Menu("&Help", 25)
			{
				new MenuItem("&Index"),
				new MenuItem("&Contents"),
				new MenuItem("&Topic:                 F1") { IsEnabled = false },
				new MenuItem("Using &Help       Shift+F1"),
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
		mnuFileLoadFile.Clicked = mnuFileLoadFile_Clicked;

		mnuDebugInstantWatch.Clicked = mnuDebugInstantWatch_Clicked;
		mnuDebugDeleteAllWatch.Clicked = mnuDebugDeleteAllWatch_Clicked;
	}

	private void mnuFileNew_Clicked()
	{
		if (CommitViewportsOrPresentError())
			PromptToSaveChanges(StartNewProgram);
	}

	private void mnuFileSave_Clicked()
	{
		if (FocusedViewport?.CompilationUnit is CompilationUnit unit)
		{
			if (CommitViewportsOrPresentError())
				InteractiveSaveIfUnitHasNoFilePath(unit);
		}
	}

	private void mnuFileSaveAs_Clicked()
	{
		if (FocusedViewport?.CompilationUnit is CompilationUnit unit)
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

	private void mnuFileLoadFile_Clicked()
	{
		if (CommitViewportsOrPresentError())
			ShowOpenFileDialog(replaceExistingProgram: false);
	}

	private void mnuDebugInstantWatch_Clicked()
	{
		InstantWatchAtCurrentCursorLocation();
	}

	private void mnuDebugDeleteAllWatch_Clicked()
	{
		ClearWatches();
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
				case ScanCode.Alt:
					AltReleaseAction = AltRelease.DeactivateMenuBar;
					break;

				case ScanCode.Escape:
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
				case ScanCode.Alt:
					AltReleaseAction = AltRelease.CloseMenu;
					break;

				case ScanCode.Escape:
					Mode = UIMode.TextEditor;
					break;
				case ScanCode.Return:
					if (ActivateMenuItem(MenuBar[SelectedMenu].Items[SelectedMenuItem]))
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

		if (LoadedFiles.Any())
		{
			PushCall(
				LoadedFiles[0].Name,
				LoadedFiles[0].Elements[0],
				lineNumber: 0,
				column: 0);
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
		Machine.KeepRunning = false;
	}
}
