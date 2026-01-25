using System;
using System.Linq;
using System.Text;

using QBX.CodeModel;
using QBX.Firmware;
using QBX.Utility;

namespace QBX.DevelopmentEnvironment;

public partial class Program : HostedProgram
{
	string _spaces = "                                                                                ";
	string _horizontalRule = "────────────────────────────────────────────────────────────────────────────────";

	void Render()
	{
		if (_spaces.Length < TextLibrary.Width)
			_spaces = new string(' ', TextLibrary.Width);
		if (_horizontalRule.Length < TextLibrary.Width)
			_horizontalRule = new string('─', TextLibrary.Width);

		bool isMenuActive = (Mode == UIMode.MenuBar) || (Mode == UIMode.Menu);
		bool isMenuOpen = (Mode == UIMode.Menu);

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

		row += RenderMenuBar(row, isMenuActive, isMenuOpen);

		if (HelpViewport != null)
			row += RenderViewport(row, HelpViewport, connectUp: false, horizontalScrollBar: false);

		row += RenderViewport(row, PrimaryViewport, connectUp: false);

		if (SplitViewport != null)
			row += RenderViewport(row, SplitViewport, connectUp: true);

		row += RenderViewport(row, ImmediateViewport, connectUp: true, horizontalScrollBar: false);

		RenderReferenceBar(row);

		if (isMenuActive || isMenuOpen)
		{
			TextLibrary.HideCursor();

			if (isMenuOpen)
				RenderOpenMenu();
		}
		else if (CurrentDialog != null)
			CurrentDialog.Render(TextLibrary);
		else if (FocusedViewport != null)
		{
			int cursorActualX = 1 + (FocusedViewport.CursorX - FocusedViewport.ScrollX);
			int cursorActualY = FocusedViewport.CachedContentTopY + (FocusedViewport.CursorY - FocusedViewport.ScrollY);

			TextLibrary.ShowCursor();
			TextLibrary.SetCursorScans(EnableOvertype ? 0 : 14, 15);
			TextLibrary.MoveCursor(cursorActualX, cursorActualY);
			TextLibrary.UpdatePhysicalCursor();
		}
	}

	int RenderMenuBar(int row, bool isMenuActive, bool isMenuOpen)
	{
		bool showAccessKeys = isMenuActive && !isMenuOpen;

		TextLibrary.MoveCursor(0, row);

		var menuAttr = Configuration.DisplayAttributes.MenuBarNormalText;
		var menuAccessKeyAttr = Configuration.DisplayAttributes.MenuBarAndPullDownMenuAccessKeys;
		var menuSelectedAttr = Configuration.DisplayAttributes.MenuBarSelectedItem;
		var menuSelectedAccessKeyAttr = Configuration.DisplayAttributes.MenuBarSelectedItemAccessKey;

		if (!showAccessKeys)
		{
			menuAccessKeyAttr = menuAttr;
			menuSelectedAccessKeyAttr = menuSelectedAttr;
		}

		if (!isMenuActive)
		{
			menuSelectedAttr = menuAttr;
			menuSelectedAccessKeyAttr = menuAccessKeyAttr;
		}

		menuAttr.Set(TextLibrary);
		TextLibrary.WriteText("  ");

		for (int i = 0; i < MenuBar.Count; i++)
		{
			var thisAttr = (i == SelectedMenu) ? menuSelectedAttr : menuAttr;
			var thisAccessKeyAttr = (i == SelectedMenu) ? menuSelectedAccessKeyAttr : menuAccessKeyAttr;

			if (i + 1 >= MenuBar.Count)
			{
				menuAttr.Set(TextLibrary);
				TextLibrary.WriteText(_spaces, 0, TextLibrary.Width - TextLibrary.CursorX - GetTextLength(MenuBar[i].Label) - 3);
			}

			MenuBar[i].CachedX = TextLibrary.CursorX;

			if (MenuBar[i].CachedX + MenuBar[i].Width + 2 >= TextLibrary.Width - 5)
				MenuBar[i].CachedX = TextLibrary.Width - MenuBar[i].Width - 5;

			RenderTextWithAccessKey(" ", thisAttr, thisAccessKeyAttr);
			RenderTextWithAccessKey(MenuBar[i].Label, thisAttr, thisAccessKeyAttr);
			RenderTextWithAccessKey(" ", thisAttr, thisAccessKeyAttr);
		}

		menuAttr.Set(TextLibrary);
		TextLibrary.WriteText(' ');

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
				TextLibrary.WriteText(label, offset, label.Length - offset);
				break;
			}

			if ((accessKeyIndex >= 0) && (accessKeyIndex + 1 < label.Length))
			{
				if (accessKeyIndex > offset)
				{
					regularAttr.Set(TextLibrary);
					TextLibrary.WriteText(label, offset, accessKeyIndex - offset);
				}

				if (label[accessKeyIndex + 1] == '&') // "&&" escaped
					regularAttr.Set(TextLibrary);
				else
					accessKeyAttr.Set(TextLibrary);

				TextLibrary.WriteText(label[accessKeyIndex + 1]);

				offset = accessKeyIndex + 2;
			}
		}
	}

	ResettableStringWriter _lineRenderBuffer = new ResettableStringWriter();

	int RenderViewport(int row, Viewport viewport, bool connectUp, bool verticalScrollBar = true, bool horizontalScrollBar = true)
	{
		int nextLineIndex = -1;
		int nextStartColumn = -1;
		int nextEndColumn = -1;

		if (IsExecuting && (viewport.CompilationUnit != null))
		{
			var nextStatement = _runtimeErrorToken?.OwnerStatement ?? _nextStatement;

			if ((nextStatement != null)
			 && (nextStatement.CodeLine is CodeLine codeLine)
			 && (viewport.CompilationElement == codeLine.CompilationElement))
			{
				nextLineIndex = codeLine.LineIndex;

				if (_runtimeErrorToken != null)
				{
					nextStartColumn = _runtimeErrorToken.Column;
					nextEndColumn = nextStartColumn + _runtimeErrorToken.Length - 1;
				}
				else
				{
					nextStartColumn = nextStatement.SourceColumn;
					nextEndColumn = nextStatement.SourceColumn + nextStatement.SourceLength - 1;
				}
			}
		}

		if (!Configuration.ShowScrollBars || !viewport.IsFocused || (viewport.Height <= 1))
		{
			verticalScrollBar = false;
			horizontalScrollBar = false;
		}

		var normalAttr = (viewport.HelpPage != null)
			? Configuration.DisplayAttributes.HelpWindowNormalText
			: Configuration.DisplayAttributes.ProgramViewWindowNormalText;

		var highlightAttr = Configuration.DisplayAttributes.ProgramViewWindowCurrentStatement;

		var breakpointAttr = Configuration.DisplayAttributes.ProgramViewWindowBreakpointLines;

		var breakpointHighlightAttr = new DisplayAttribute(
			highlightAttr.Foreground,
			breakpointAttr.Background,
			name: "Highlighted Breakpoint Line (Computed)");

		char topLeft = connectUp ? '├' : '┌';
		char topRight = connectUp ? '┤' : '┐';

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

			if (coordinate < 0)
				return 0;
			if (coordinate >= domain)
				return scrollBarWidth;

			return coordinate * scrollBarWidth / domain;
		}

		int horizontalScrollBarPosition = CalculateScrollBarPosition(viewport.ScrollX, horizontalScrollBarWidth, 306);
		int verticalScrollBarPosition = CalculateScrollBarPosition(viewport.ScrollY, verticalScrollBarHeight, viewport.GetContentLineCount());

		if (viewport.ShowMaximize)
			headingRight -= 4;

		TextLibrary.MoveCursor(0, row);

		normalAttr.Set(TextLibrary);
		TextLibrary.WriteText(topLeft);
		TextLibrary.WriteText(_horizontalRule, 0, headingLeft);
		if (viewport.IsFocused)
			normalAttr.SetInverted(TextLibrary);
		TextLibrary.WriteText(' ');
		TextLibrary.WriteText(viewport.Heading);
		TextLibrary.WriteText(' ');
		normalAttr.Set(TextLibrary);
		TextLibrary.WriteText(_horizontalRule, 0, headingRight);

		if (viewport.ShowMaximize)
		{
			TextLibrary.WriteText('┤');
			normalAttr.SetInverted(TextLibrary);
			TextLibrary.WriteText('↑');
			normalAttr.Set(TextLibrary);
			TextLibrary.WriteText("├─");
		}

		TextLibrary.WriteText(topRight);

		viewport.CachedContentTopY = row + 1;
		viewport.CachedContentHeight = viewportContentHeight;

		for (int y = 0; y < viewportContentHeight; y++)
		{
			TextLibrary.WriteText(leftRight);

			// TODO: show included lines

			int lineIndex = y + viewport.ScrollY;

			StringBuilder buffer;

			if ((lineIndex == viewport.CursorY) && (viewport.CurrentLineBuffer != null))
				buffer = viewport.CurrentLineBuffer;
			else
			{
				_lineRenderBuffer.Reset();

				viewport.RenderLine(lineIndex, _lineRenderBuffer);

				buffer = _lineRenderBuffer.GetStringBuilder();
			}

			int chars = buffer.Length - viewport.ScrollX;

			if (chars < 0)
				chars = 0;
			if (chars > viewportContentWidth)
				chars = viewportContentWidth;

			var (unselectedLeft, selected, unselectedRight) =
				CalculateSelectionHighlight(viewport.Clipboard, lineIndex, viewport.ScrollX, viewportContentWidth);

			var rowAttr = normalAttr;
			var rowHighlightAttr = highlightAttr;

			if ((viewport.TryGetCodeLineAt(lineIndex) is CodeLine currentCodeLine)
			 && _breakpoints.Contains(currentCodeLine))
			{
				rowAttr = breakpointAttr;
				rowHighlightAttr = breakpointHighlightAttr;
			}

			if (selected != 0)
			{
				// Draw line that contains at least some selected characters.

				if (unselectedLeft != 0)
				{
					rowAttr.Set(TextLibrary);

					if (chars >= unselectedLeft)
						TextLibrary.WriteText(buffer, viewport.ScrollX, unselectedLeft);
					else
					{
						int virtualChars = unselectedLeft - chars;
						int realChars = unselectedLeft - virtualChars;

						TextLibrary.WriteText(buffer, viewport.ScrollX, realChars);
						TextLibrary.WriteText(_spaces, 0, virtualChars);
					}
				}

				chars -= unselectedLeft;
				if (chars < 0)
					chars = 0;

				if (selected != 0)
				{
					rowAttr.SetInverted(TextLibrary);

					if (chars >= selected)
						TextLibrary.WriteText(buffer, viewport.ScrollX + unselectedLeft, selected);
					else
					{
						int virtualChars = selected - chars;
						int realChars = selected - virtualChars;

						TextLibrary.WriteText(buffer, viewport.ScrollX + unselectedLeft, realChars);
						TextLibrary.WriteText(_spaces, 0, virtualChars);
					}
				}

				rowAttr.Set(TextLibrary);

				chars -= selected;
				if (chars < 0)
					chars = 0;

				if (unselectedRight != 0)
				{
					rowAttr.Set(TextLibrary);

					if (chars >= unselectedRight)
						TextLibrary.WriteText(buffer, viewport.ScrollX + unselectedLeft + selected, unselectedLeft);
					else
					{
						int virtualChars = unselectedRight - chars;
						int realChars = unselectedRight - virtualChars;

						TextLibrary.WriteText(buffer, viewport.ScrollX + unselectedLeft + selected, realChars);
						TextLibrary.WriteText(_spaces, 0, virtualChars);
					}
				}
			}
			else
			{
				// Draw line that contains no selected characters but might have some highlighted characters.
				int highlightStart = -1;
				int highlightEnd = -1;

				if (lineIndex == nextLineIndex)
				{
					highlightStart = nextStartColumn - viewport.ScrollX;
					highlightEnd = nextEndColumn - viewport.ScrollX;
				}

				if ((highlightStart >= viewportContentWidth) || (highlightEnd < 0))
				{
					rowAttr.Set(TextLibrary);

					int virtualChars = viewportContentWidth - chars;

					TextLibrary.WriteText(buffer, viewport.ScrollX, chars);
					TextLibrary.WriteText(_spaces, 0, virtualChars);
				}
				else
				{
					if (viewport.ScrollX + highlightStart > buffer.Length)
						highlightStart = buffer.Length - viewport.ScrollX;

					int unhighlightedLeft = highlightStart;
					int highlightedChars = highlightEnd - highlightStart + 1;
					int unhighlightedRight = viewportContentWidth - highlightEnd - 1;

					if (unhighlightedRight < 0)
					{
						highlightedChars += unhighlightedRight;
						unhighlightedRight = 0;
					}

					// Unhighlighted portion to the left
					int realChars = unhighlightedLeft;

					if (viewport.ScrollX + realChars > buffer.Length)
						realChars = buffer.Length - viewport.ScrollX;

					int virtualChars = unhighlightedLeft - realChars;

					rowAttr.Set(TextLibrary);

					TextLibrary.WriteText(buffer, viewport.ScrollX, realChars);
					TextLibrary.WriteText(_spaces, 0, virtualChars);

					// Highlighted portion
					realChars = highlightedChars;

					if (viewport.ScrollX + highlightStart + realChars > buffer.Length)
					{
						realChars = buffer.Length - viewport.ScrollX - highlightStart;

						if (realChars < 0)
							realChars = 0;
					}

					virtualChars = highlightedChars - realChars;

					rowHighlightAttr.Set(TextLibrary);

					TextLibrary.WriteText(buffer, viewport.ScrollX + highlightStart, realChars);
					TextLibrary.WriteText(_spaces, 0, virtualChars);

					// Unhighlighted portion to the right
					rowAttr.Set(TextLibrary);

					realChars = unhighlightedRight;

					if (viewport.ScrollX + highlightEnd + 1 + realChars > buffer.Length)
					{
						realChars = buffer.Length - viewport.ScrollX - highlightEnd - 1;

						if (realChars < 0)
							realChars = 0;
					}

					virtualChars = unhighlightedRight - realChars;

					TextLibrary.WriteText(buffer, viewport.ScrollX + highlightEnd + 1, realChars);
					TextLibrary.WriteText(_spaces, 0, virtualChars);
				}
			}

			if (!verticalScrollBar)
			{
				normalAttr.Set(TextLibrary);
				TextLibrary.WriteText(leftRight);
			}
			else
			{
				if ((y == 0) || (y + 1 == viewportContentHeight))
				{
					Configuration.DisplayAttributes.ScrollBarsandScrollArrows.Set(TextLibrary);

					if (y == 0)
						TextLibrary.WriteText('↑');
					else
						TextLibrary.WriteText('↓');
				}
				else if (y - 1 == verticalScrollBarPosition)
				{
					Configuration.DisplayAttributes.ScrollBarPositionIndicatorBox.Set(TextLibrary);
					TextLibrary.WriteText(' ');
				}
				else
				{
					Configuration.DisplayAttributes.ScrollBarsandScrollArrows.Set(TextLibrary);
					TextLibrary.WriteText('░');
				}

				normalAttr.Set(TextLibrary);
			}
		}

		if (!horizontalScrollBar)
			return viewportContentHeight + 1;
		else
		{
			TextLibrary.WriteText(leftRight);
			Configuration.DisplayAttributes.ScrollBarsandScrollArrows.Set(TextLibrary);
			TextLibrary.WriteText('←');

			for (int x = 0; x < horizontalScrollBarPosition; x++)
				TextLibrary.WriteText('░');

			Configuration.DisplayAttributes.ScrollBarPositionIndicatorBox.Set(TextLibrary);
			TextLibrary.WriteText(' ');
			Configuration.DisplayAttributes.ScrollBarsandScrollArrows.Set(TextLibrary);

			for (int x = horizontalScrollBarPosition + 1; x < horizontalScrollBarWidth; x++)
				TextLibrary.WriteText('░');

			TextLibrary.WriteText('→');

			normalAttr.Set(TextLibrary);

			TextLibrary.WriteText(leftRight);

			return viewportContentHeight + 2;
		}
	}

	(int unselectedLeft, int selected, int unselectedRight) CalculateSelectionHighlight(
		Clipboard clipboard,
		int lineIndex, int scrollX, int chars)
	{
		// In a viewport showing line lineIndex, scrolled right by scrollX characters,
		// subdivide chars characters into unselected and selected regions.

		var (startX, startY, endX, endY) = clipboard.GetSelectionRange();

		int effectiveStartX = Math.Min(startX, endX);
		int effectiveEndX = Math.Max(startX, endX);

		int effectiveStartY = Math.Min(startY, endY);
		int effectiveEndY = Math.Max(startY, endY);

		if ((effectiveStartX == 0) && (effectiveEndX == 0))
			effectiveEndY--;

		// Bail if the line isn't involved in the selection range at all.
		if ((lineIndex < effectiveStartY) || (lineIndex > effectiveEndY))
			return (chars, 0, 0);

		// Select the whole line if we're in line mode.
		if (endY != startY)
			return (0, chars, 0);

		// Select nothing if the character range is entirely off the screen.
		if ((effectiveEndX <= scrollX) || (effectiveStartX - scrollX >= chars))
			return (chars, 0, 0);

		int left = effectiveStartX - scrollX;
		int selected = effectiveEndX - effectiveStartX;
		int right = chars - (effectiveEndX - scrollX);

		if (left < 0)
		{
			left = 0;
			selected = effectiveEndX - scrollX;
		}

		if (right < 0)
		{
			right = 0;
			selected = chars - left;
		}

		return (left, selected, right);
	}

	static byte[]? _statusCharBuffer;

	void RenderReferenceBar(int row)
	{
		int cursorX = (FocusedViewport?.CursorX ?? 0) + 1;
		int cursorY = (FocusedViewport?.CursorY ?? 0) + 1;

		int referenceBarRemainingChars = TextLibrary.Width
			- 10 // cursor position
			- 8; // status indicators

		Configuration.DisplayAttributes.ReferenceBarNormalText.Set(TextLibrary);
		TextLibrary.WriteText(' ');
		referenceBarRemainingChars--;

		if (ReferenceBarActions != null)
		{
			for (int i = 0; i < ReferenceBarActions.Length; i++)
			{
				if (i == SelectedReferenceBarAction)
					Configuration.DisplayAttributes.ReferenceBarNormalText.SetInverted(TextLibrary);

				TextLibrary.WriteText('<');
				TextLibrary.WriteText(ReferenceBarActions[i].Label);
				TextLibrary.WriteText('>');

				referenceBarRemainingChars -= 2 + ReferenceBarActions[i].Label.Length;

				if (i == SelectedReferenceBarAction)
					Configuration.DisplayAttributes.ReferenceBarNormalText.Set(TextLibrary);
			}

			TextLibrary.WriteText(' ');
			referenceBarRemainingChars--;
		}

		if (ReferenceBarText != null)
		{
			int textChars = ReferenceBarText.Length;

			if (textChars > referenceBarRemainingChars)
				textChars = referenceBarRemainingChars;

			TextLibrary.WriteText(ReferenceBarText, 0, textChars);

			referenceBarRemainingChars -= textChars;
		}

		if (referenceBarRemainingChars > 0)
		{
			TextLibrary.WriteText(_spaces, 0, referenceBarRemainingChars);
		}


		if ((_statusCharBuffer == null) || (_statusCharBuffer.Length < 8))
			_statusCharBuffer = new byte[8];

		_statusCharBuffer.AsSpan().Fill(32);

		_statusCharBuffer[0] = (byte)'|';
		_statusCharBuffer[5] = Machine.SystemMemory.KeyboardStatus_CapsLock ? (byte)'C' : (byte)' ';
		_statusCharBuffer[6] = Machine.SystemMemory.KeyboardStatus_NumLock ? (byte)'N' : (byte)' ';

		Configuration.DisplayAttributes.ReferenceBarStatusIndicators.Set(TextLibrary);
		TextLibrary.WriteText(_statusCharBuffer, 0, 8);

		if (cursorX > 99999)
			cursorX = 99999;
		if (cursorY > 999)
			cursorY = 999;

		Configuration.DisplayAttributes.ReferenceBarNormalText.Set(TextLibrary);
		TextLibrary.WriteNumber(cursorY, 5);
		TextLibrary.WriteText(':');
		TextLibrary.WriteNumber(cursorX, 3);
		TextLibrary.WriteText(' ');
	}

	void RenderOpenMenu()
	{
		if ((SelectedMenu < 0) || (SelectedMenu >= MenuBar.Count))
			return;

		var menu = MenuBar[SelectedMenu];

		int boxX = menu.CachedX - 1;

		char topLeft = '┌';
		char topRight = '┐';
		char bottomLeft = '└';
		char bottomRight = '┘';

		char separatorLeft = '├';
		char separatorRight = '┤';

		char vertical = '│';

		TextLibrary.MoveCursor(boxX, 1);
		Configuration.DisplayAttributes.PullDownMenuBorder.Set(TextLibrary);
		TextLibrary.WriteText(topLeft);
		TextLibrary.WriteText(_horizontalRule, 0, menu.Width + 2);
		TextLibrary.WriteText(topRight);

		for (int i = 0; i < menu.Items.Count; i++)
		{
			int y = i + 2;

			TextLibrary.MoveCursor(boxX, y);

			if (menu.Items[i].IsSeparator)
			{
				TextLibrary.WriteText(separatorLeft);
				TextLibrary.WriteText(_horizontalRule, 0, menu.Width + 2);
				TextLibrary.WriteText(separatorRight);
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
					if (menu.Items[i].IsEnabled)
						attr = Configuration.DisplayAttributes.PullDownMenuandListBoxNormalText;
					else
						attr = Configuration.DisplayAttributes.PullDownMenuandDialogBoxDisabledItems;

					accessKeyAttr = Configuration.DisplayAttributes.MenuBarAndPullDownMenuAccessKeys;
				}

				if (!menu.Items[i].IsEnabled)
					accessKeyAttr = attr;

				TextLibrary.WriteText(vertical);
				attr.Set(TextLibrary);
				TextLibrary.WriteText(' ');
				RenderTextWithAccessKey(menu.Items[i].Label, attr, accessKeyAttr);

				int labelWidth = GetTextLength(menu.Items[i].Label);

				if (labelWidth < menu.Width)
					TextLibrary.WriteText(_spaces, 0, menu.Width - labelWidth);
				if (labelWidth > menu.Width)
					throw new Exception("Internal error");

				TextLibrary.WriteText(' ');

				Configuration.DisplayAttributes.PullDownMenuBorder.Set(TextLibrary);
				TextLibrary.WriteText(vertical);
			}
		}

		TextLibrary.MoveCursor(boxX, 2 + menu.Items.Count);
		TextLibrary.WriteText(bottomLeft);
		TextLibrary.WriteText(_horizontalRule, 0, menu.Width + 2);
		TextLibrary.WriteText(bottomRight);

		// Shadow
		Configuration.DisplayAttributes.PullDownMenuandDialogBoxShadow.Set(TextLibrary);

		int shadowX = boxX + 1 + menu.Width + 3;

		for (int y = 0, yl = menu.Items.Count + 2; y < yl; y++)
			TextLibrary.WriteAttributesAt(shadowX, y + 2, charCount: 2);

		TextLibrary.WriteAttributesAt(boxX + 2, menu.Items.Count + 3, charCount: menu.Width + 2);
	}
}
