using QBX.Hardware;
using System.Xml;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	MenuBar MenuBar =
		new MenuBar()
		{
			new Menu("&File", 16)
			{
				new MenuItem("&New Program"),
				new MenuItem("&Open Program..."),
				new MenuItem("&Merge..."),
				new MenuItem("&Save"),
				new MenuItem("Save &As..."),
				new MenuItem("Sa&ve All"),
				MenuItem.Separator,
				new MenuItem("&Create File..."),
				new MenuItem("&Load File..."),
				new MenuItem("&Unload File..."),
				MenuItem.Separator,
				new MenuItem("&Print..."),
				new MenuItem("&DOS Shell"),
				MenuItem.Separator,
				new MenuItem("E&xit"),
			},
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
			},
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
			},
			new Menu("&Search", 24)
			{
				new MenuItem("&Find..."),
				new MenuItem("&Selected Text     Ctrl+\\"),
				new MenuItem("&Repeat Last Find      F3"),
				new MenuItem("&Change..."),
				new MenuItem("&Label..."),
			},
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
			},
			new Menu("&Debug", 27)
			{
				new MenuItem("&Add Watch..."),
				new MenuItem("&Instanc Watch...   Shift+F9"),
				new MenuItem("&Watchpoint..."),
				new MenuItem("&Delete Watch...") { IsEnabled = false },
				new MenuItem("De&lete All Watch") { IsEnabled = false },
				MenuItem.Separator,
				new MenuItem("&Trace On"),
				new MenuItem("&History On"),
				MenuItem.Separator,
				new MenuItem("Toggle &Breakpoint        F9"),
				new MenuItem("&Clear All Breakpoints"),
				new MenuItem("Break on &Errors"),
				new MenuItem("&Set Next Statement") { IsEnabled = false },
			},
			new Menu("&Calls", 16)
			{
				// Dynamically populated
			},
			new Menu("&Utility", 18)
			{
				new MenuItem("&Run DOS Command..."),
				new MenuItem("&Customize Menu..."),
			},
			new Menu("&Options", 15)
			{
				new MenuItem("&Display..."),
				new MenuItem("Set &Paths..."),
				new MenuItem("Right &Mouse..."),
				new MenuItem("&Syntax Checking") { IsChecked = true },
			},
			new Menu("&Help", 25)
			{
				new MenuItem("&Index"),
				new MenuItem("&Contents"),
				new MenuItem("&Topic:                 F1") { IsEnabled = false },
				new MenuItem("Using &Help       Shift+F1"),
			}
		};

	public int SelectedMenu = -1;
	public int SelectedMenuItem = -1;
	public bool IgnoreAltRelease = false;

	void ActivateMenuItem(MenuItem item)
	{
		// TODO
	}

	void ProcessMenuBarKey(KeyEvent input)
	{
		if (input.IsRelease)
		{
			if (input.ScanCode == ScanCode.Alt)
			{
				if (IgnoreAltRelease)
					IgnoreAltRelease = false;
				else
					Mode = UIMode.TextEditor;
			}
		}
		else
		{
			switch (input.ScanCode)
			{
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
					string inkey = input.ToInKeyString();

					if (!string.IsNullOrEmpty(inkey))
					{
						MenuBar.EnsureAcceleratorLookUp();

						if (MenuBar.ItemByAccelerator.TryGetValue(inkey, out var menu))
						{
							Mode = UIMode.Menu;
							SelectedMenu = MenuBar.Items.IndexOf(menu);
							SelectedMenuItem = 0;
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
				Mode = UIMode.TextEditor;
		}
		else
		{
			switch (input.ScanCode)
			{
				case ScanCode.Escape:
					Mode = UIMode.TextEditor;
					break;
				case ScanCode.Return:
					Mode = UIMode.TextEditor;
					ActivateMenuItem(MenuBar[SelectedMenu].Items[SelectedMenuItem]);
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
					string inkey = input.ToInKeyString();

					if (!string.IsNullOrEmpty(inkey))
					{
						var menu = MenuBar[SelectedMenu];

						menu.EnsureAcceleratorLookUp();

						if (menu.ItemByAccelerator.TryGetValue(inkey, out var item))
						{
							Mode = UIMode.TextEditor;
							ActivateMenuItem(item);
						}
					}

					break;
				}
			}
		}
	}
}
