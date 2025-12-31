using QBX.Firmware;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment;

public class Program
{
	Menu[] MenuBar =
		[
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
		];

	public Configuration Configuration = new Configuration();

	public List<Watch> Watches = new List<Watch>();
	public Viewport? HelpViewport = null; // new Viewport() { HelpPage = new HelpPage() };
	public Viewport PrimaryViewport = new Viewport() { IsFocused = true };
	public Viewport? SplitViewport;
	public Viewport ImmediateViewport = new Viewport() { Heading = "Immediate", ShowMaximize = false, Height = 2 };
	public ReferenceBarAction[]? ReferenceBarActions;
	public int SelectedReferenceBarAction = -1;
	public string? ReferenceBarText;

	public Viewport? FocusedViewport;

	public int SelectedMenu = -1;
	public bool IsMenuActive, IsMenuOpen;
	public int SelectedMenuItem = -1;

	public TextLibrary TextLibrary;

	public Program(Machine machine, Video video)
	{
		if (machine.GraphicsArray.Sequencer.CharacterWidth == 9)
			video.SetCharacterWidth(8);

		TextLibrary = new TextLibrary(machine.GraphicsArray);
		TextLibrary.MovePhysicalCursor = false;

		FocusedViewport = PrimaryViewport;

		SelectedMenu = 0;
		IsMenuOpen = true;
		SelectedMenuItem = 2;

		Render();
	}

	void Render()
	{
		if (ImmediateViewport.Height > 10)
			ImmediateViewport.Height = 10;

		int height = TextLibrary.Height;

		height--; // menu bar
		height--; // status bar

		height -= (ImmediateViewport.Height + 1);

		if (SplitViewport != null)
		{
			if (Configuration.ShowScrollBars)
				height--; // horizontal scrollbar

			int maxSplitViewport = height - 1/*primary title*/ - 1/* split title*/;

			if (SplitViewport.Height > maxSplitViewport)
				SplitViewport.Height = maxSplitViewport;

			height -= (SplitViewport.Height + 1);
		}

		if (HelpViewport != null)
		{
			height--; // help title
			height -= HelpViewport.Height;
		}

		height--; // primary title

		PrimaryViewport.Height = height;

		int row = 0;

		row += RenderMenuBar(row);

		if (HelpViewport != null)
			row += RenderViewport(row, HelpViewport, connectUp: false, horizontalScrollBar: false);

		row += RenderViewport(row, PrimaryViewport, connectUp: false, connectDown: true);

		if (SplitViewport != null)
			row += RenderViewport(row, SplitViewport, connectUp: true, connectDown: true);

		row += RenderViewport(row, ImmediateViewport, connectUp: true, horizontalScrollBar: false);

		RenderReferenceBar(row);

		if (IsMenuOpen)
		{
			TextLibrary.HideCursor();
			RenderOpenMenu();
		}
		else if (FocusedViewport != null)
		{
			int cursorActualX = 1 + (FocusedViewport.CursorX - FocusedViewport.ScrollX);
			int cursorActualY = FocusedViewport.CachedContentTopY + (FocusedViewport.CursorY - FocusedViewport.ScrollY);

			TextLibrary.ShowCursor();
			TextLibrary.MoveCursor(cursorActualX, cursorActualY);
			TextLibrary.UpdatePhysicalCursor();
		}
	}

	int RenderMenuBar(int row)
	{
		bool showAccessKeys = IsMenuActive && !IsMenuOpen;

		TextLibrary.MoveCursor(0, row);

		var menuAttr = Configuration.DisplayAttributes.MenuBarNormalText;
		var menuAccessKeyAttr = Configuration.DisplayAttributes.MenuBarAndPullDownMenuAccessKeys;
		var menuSelectedAttr = Configuration.DisplayAttributes.MenuBarSelectedItem;
		var menuSelectedAccessKeyAttr = Configuration.DisplayAttributes.MenuBarSelectedItemAccessKey;

		if (!showAccessKeys)
			menuAccessKeyAttr = menuAttr;

		menuAttr.Set(TextLibrary);
		TextLibrary.Write("  ");

		for (int i = 0; i < MenuBar.Length; i++)
		{
			var thisAttr = (i == SelectedMenu) ? menuSelectedAttr : menuAttr;
			var thisAccessKeyAttr = (i == SelectedMenu) ? menuSelectedAccessKeyAttr : menuAccessKeyAttr;

			if (i + 1 >= MenuBar.Length)
			{
				thisAttr.Set(TextLibrary);
				TextLibrary.Write(
					"                                                                                ",
					0,
					TextLibrary.Width - TextLibrary.CursorX - GetTextLength(MenuBar[i].Label) - 3);
			}

			MenuBar[i].CachedX = TextLibrary.CursorX;

			RenderTextWithAccessKey(" ", thisAttr, thisAccessKeyAttr);
			RenderTextWithAccessKey(MenuBar[i].Label, thisAttr, thisAccessKeyAttr);
			RenderTextWithAccessKey(" ", thisAttr, thisAccessKeyAttr);
		}

		menuAttr.Set(TextLibrary);
		TextLibrary.Write(' ');

		return 1;
	}

	int GetTextLength(string str, bool recognizeAccessKey = true)
	{
		if (!recognizeAccessKey)
			return str.Length;
		else
		{
			int count = 0;

			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == '&')
					i++;

				count++;
			}

			return count;
		}
	}

	void RenderTextWithAccessKey(string label, DisplayAttribute regularAttr, DisplayAttribute accessKeyAttr)
	{
		int offset = 0;

		while (offset < label.Length)
		{
			int accessKeyIndex = label.IndexOf('&', offset);

			if (accessKeyIndex < 0)
			{
				regularAttr.Set(TextLibrary);
				TextLibrary.Write(label, offset, label.Length - offset);
				break;
			}

			if ((accessKeyIndex >= 0) && (accessKeyIndex + 1 < label.Length))
			{
				if (accessKeyIndex > offset)
				{
					regularAttr.Set(TextLibrary);
					TextLibrary.Write(label, offset, accessKeyIndex - offset);
				}

				if (label[accessKeyIndex + 1] == '&') // "&&" escaped
					regularAttr.Set(TextLibrary);
				else
					accessKeyAttr.Set(TextLibrary);

				TextLibrary.Write(label[accessKeyIndex + 1]);

				offset = accessKeyIndex + 2;
			}
		}
	}

	ResettableStringWriter _lineRenderBuffer = new ResettableStringWriter();

	int RenderViewport(int row, Viewport viewport, bool connectUp, bool connectDown = false, bool verticalScrollBar = true, bool horizontalScrollBar = true)
	{
		if (!Configuration.ShowScrollBars || !viewport.IsFocused || (viewport.Height <= 1))
		{
			verticalScrollBar = false;
			horizontalScrollBar = false;
		}

		var attr = (viewport.HelpPage != null)
			? Configuration.DisplayAttributes.HelpWindowNormalText
			: Configuration.DisplayAttributes.ProgramViewWindowNormalText;

		char topLeft = connectUp ? '├' : '┌';
		char topRight = connectUp ? '┤' : '┐';

		string top = "────────────────────────────────────────────────────────────────────────────────";

		char leftRight = '│';

		int middleChars = TextLibrary.Width - 2;

		int headingOffset = (middleChars - (viewport.Heading.Length + 2)) / 2;

		int headingLeft = headingOffset - 1;
		int headingRight = TextLibrary.Width - (headingOffset + viewport.Heading.Length + 3);

		int viewportContentWidth = TextLibrary.Width - 2;
		int viewportContentHeight = viewport.Height;

		if (horizontalScrollBar)
			viewportContentHeight--;

		// Characters between the arrows
		int horizontalScrollBarWidth = viewportContentWidth - 2;
		int verticalScrollBarHeight = viewportContentHeight - 2;

		int CalculateScrollBarPosition(int coordinate, int scrollBarWidth, int domain)
		{
			if (domain == 0)
				return 0;

			return coordinate * scrollBarWidth / domain;
		}

		int horizontalScrollBarPosition = CalculateScrollBarPosition(viewport.ScrollX, horizontalScrollBarWidth, 256);
		int verticalScrollBarPosition = CalculateScrollBarPosition(viewport.ScrollY, verticalScrollBarHeight, viewport.GetContentLineCount());

		if (viewport.ShowMaximize)
			headingRight -= 4;

		TextLibrary.MoveCursor(0, row);

		attr.Set(TextLibrary);
		TextLibrary.Write(topLeft);
		TextLibrary.Write(top, 0, headingLeft);
		if (viewport.IsFocused)
			attr.SetInverted(TextLibrary);
		TextLibrary.Write(' ');
		TextLibrary.Write(viewport.Heading);
		TextLibrary.Write(' ');
		attr.Set(TextLibrary);
		TextLibrary.Write(top, 0, headingRight);

		if (viewport.ShowMaximize)
		{
			TextLibrary.Write('┤');
			attr.SetInverted(TextLibrary);
			TextLibrary.Write('↑');
			attr.Set(TextLibrary);
			TextLibrary.Write("├─");
		}

		TextLibrary.Write(topRight);

		viewport.CachedContentTopY = row + 1;

		for (int y = 0; y < viewportContentHeight; y++)
		{
			TextLibrary.Write(leftRight);

			// TODO: current statement
			// TODO: show included lines

			int lineIndex = y + viewport.ScrollY;

			_lineRenderBuffer.Reset();

			viewport.CompilationElement?.Lines[lineIndex].Render(_lineRenderBuffer);

			var buffer = _lineRenderBuffer.GetStringBuilder();

			int chars = buffer.Length - viewport.ScrollX;

			if (chars > viewportContentWidth)
				chars = viewportContentWidth;

			TextLibrary.Write(buffer, viewport.ScrollX, chars);

			if (chars < viewportContentWidth)
			{
				TextLibrary.Write(
					"                                                                                ",
					0,
					viewportContentWidth - chars);
			}

			if (!verticalScrollBar)
				TextLibrary.Write(leftRight);
			else
			{
				if ((y == 0) || (y + 1 == viewportContentHeight))
				{
					Configuration.DisplayAttributes.ScrollBarsandScrollArrows.Set(TextLibrary);

					if (y == 0)
						TextLibrary.Write('↑');
					else
						TextLibrary.Write('↓');
				}
				else if (y - 1 == verticalScrollBarPosition)
				{
					Configuration.DisplayAttributes.ScrollBarPositionIndicatorBox.Set(TextLibrary);
					TextLibrary.Write(' ');
				}
				else
				{
					Configuration.DisplayAttributes.ScrollBarsandScrollArrows.Set(TextLibrary);
					TextLibrary.Write('░');
				}

				attr.Set(TextLibrary);
			}
		}

		if (!horizontalScrollBar)
			return viewportContentHeight + 1;
		else
		{
			TextLibrary.Write(leftRight);
			Configuration.DisplayAttributes.ScrollBarsandScrollArrows.Set(TextLibrary);
			TextLibrary.Write('←');

			for (int x = 0; x < horizontalScrollBarPosition; x++)
				TextLibrary.Write('░');

			Configuration.DisplayAttributes.ScrollBarPositionIndicatorBox.Set(TextLibrary);
			TextLibrary.Write(' ');
			Configuration.DisplayAttributes.ScrollBarsandScrollArrows.Set(TextLibrary);

			for (int x = horizontalScrollBarPosition + 1; x < horizontalScrollBarWidth; x++)
				TextLibrary.Write('░');

			TextLibrary.Write('→');

			attr.Set(TextLibrary);

			TextLibrary.Write(leftRight);

			return viewportContentHeight + 2;
		}
	}

	void RenderReferenceBar(int row)
	{
		int cursorX = (FocusedViewport?.CursorX ?? 0) + 1;
		int cursorY = (FocusedViewport?.CursorY ?? 0) + 1;

		int referenceBarRemainingChars = TextLibrary.Width
			- 10 // cursor position
			- 8; // status indicators

		Configuration.DisplayAttributes.ReferenceBarNormalText.Set(TextLibrary);
		TextLibrary.Write(' ');
		referenceBarRemainingChars--;

		if (ReferenceBarActions != null)
		{
			for (int i = 0; i < ReferenceBarActions.Length; i++)
			{
				if (i == SelectedReferenceBarAction)
					Configuration.DisplayAttributes.ReferenceBarNormalText.SetInverted(TextLibrary);

				TextLibrary.Write('<');
				TextLibrary.Write(ReferenceBarActions[i].Label);
				TextLibrary.Write('>');

				referenceBarRemainingChars -= 2 + ReferenceBarActions[i].Label.Length;

				if (i == SelectedReferenceBarAction)
					Configuration.DisplayAttributes.ReferenceBarNormalText.Set(TextLibrary);
			}

			TextLibrary.Write(' ');
			referenceBarRemainingChars--;
		}

		if (ReferenceBarText != null)
		{
			int textChars = ReferenceBarText.Length;

			if (textChars > referenceBarRemainingChars)
				textChars = referenceBarRemainingChars;

			TextLibrary.Write(ReferenceBarText, 0, textChars);

			referenceBarRemainingChars -= textChars;
		}

		if (referenceBarRemainingChars > 0)
		{
			TextLibrary.Write(
				"                                                                                ",
				0,
				referenceBarRemainingChars);
		}

		Configuration.DisplayAttributes.ReferenceBarStatusIndicators.Set(TextLibrary);
		TextLibrary.Write("│     N "); // TODO: what are these??

		Configuration.DisplayAttributes.ReferenceBarNormalText.Set(TextLibrary);
		TextLibrary.WriteNumber(cursorX, 5);
		TextLibrary.Write(':');
		TextLibrary.WriteNumber(cursorY, 3);
		TextLibrary.Write(' ');
	}

	void RenderOpenMenu()
	{
		if ((SelectedMenu < 0) || (SelectedMenu >= MenuBar.Length))
			return;

		var menu = MenuBar[SelectedMenu];

		int boxX = menu.CachedX - 1;

		char topLeft = '┌';
		char topRight = '┐';
		char bottomLeft = '└';
		char bottomRight = '┘';

		char separatorLeft = '├';
		char separatorRight = '┤';

		string horizontal = "────────────────────────────────────────────────────────────────────────────────";
		char vertical = '│';

		TextLibrary.MoveCursor(boxX, 1);
		Configuration.DisplayAttributes.PullDownMenuBorder.Set(TextLibrary);
		TextLibrary.Write(topLeft);
		TextLibrary.Write(horizontal, 0, menu.Width);
		TextLibrary.Write(topRight);

		for (int i = 0; i < menu.Items.Count; i++)
		{
			int y = i + 2;

			TextLibrary.MoveCursor(boxX, y);

			if (menu.Items[i].IsSeparator)
			{
				TextLibrary.Write(separatorLeft);
				TextLibrary.Write(horizontal, 0, menu.Width);
				TextLibrary.Write(separatorRight);
			}
			else
			{
				DisplayAttribute attr, accessKeyAttr;

				if (i == SelectedMenuItem)
				{
					attr = Configuration.DisplayAttributes.PullDownMenuSelectedItem;
					accessKeyAttr = Configuration.DisplayAttributes.PullDownMenuSelectedItemAccessKey;
				}
				else
				{
					attr = Configuration.DisplayAttributes.PullDownMenuandListBoxNormalText;
					accessKeyAttr = Configuration.DisplayAttributes.MenuBarAndPullDownMenuAccessKeys;
				}

				TextLibrary.Write(vertical);
				RenderTextWithAccessKey(menu.Items[i].Label, attr, accessKeyAttr);

				int labelWidth = GetTextLength(menu.Items[i].Label);

				if (labelWidth < menu.Width)
					TextLibrary.Write("                            ", 0, menu.Width - labelWidth);
				if (labelWidth > menu.Width)
					throw new Exception("Internal error");

				Configuration.DisplayAttributes.PullDownMenuBorder.Set(TextLibrary);
				TextLibrary.Write(vertical);
			}
		}

		TextLibrary.MoveCursor(boxX, 2 + menu.Items.Count);
		TextLibrary.Write(bottomLeft);
		TextLibrary.Write(horizontal, 0, menu.Width);
		TextLibrary.Write(bottomRight);

		// Shadow
		Configuration.DisplayAttributes.PullDownMenuandDialogBoxShadow.Set(TextLibrary);

		int shadowX = boxX + 1 + menu.Width + 1;

		for (int y = 0, yl = menu.Items.Count + 2; y < yl; y++)
			TextLibrary.WriteAttributesAt(shadowX, y + 2, charCount: 2);

		TextLibrary.WriteAttributesAt(boxX + 2, menu.Items.Count + 3, charCount: menu.Width);
	}
}
