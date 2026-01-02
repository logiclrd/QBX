using QBX.CodeModel;
using QBX.Hardware;

using System.Text;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	enum Priority
	{
		Cursor,
		Scroll,
	}

	void ProcessTextEditorKey(KeyEvent input)
	{
		if (input.IsRelease)
			return;

		if ((input.ScanCode == ScanCode.Alt) && !input.IsRelease)
		{
			Mode = UIMode.MenuBar;
			AltReleaseAction = AltRelease.ActivateMenuBar;
			SelectedMenu = -1;

			return;
		}

		if (FocusedViewport == null)
			FocusedViewport = PrimaryViewport;

		int newCursorX = FocusedViewport.CursorX;
		int newCursorY = FocusedViewport.CursorY;
		int newScrollX = FocusedViewport.ScrollX;
		int newScrollY = FocusedViewport.ScrollY;

		var priority = Priority.Cursor;
		bool select = input.Modifiers.ShiftKey;

		int contentLineCount = FocusedViewport.GetContentLineCount();

		int viewportWidth = TextLibrary.Width - 2;
		int viewportHeight = FocusedViewport.CachedContentHeight;

		if (viewportHeight == 0)
			viewportHeight = FocusedViewport.Height - 2;

		var currentLine = new Lazy<StringBuilder>(
			() =>
			{
				if (FocusedViewport.CurrentLineBuffer != null)
					return FocusedViewport.CurrentLineBuffer;
				else
				{
					var writer = new StringWriter();

					FocusedViewport.RenderLine(FocusedViewport.CursorY, writer);

					return writer.GetStringBuilder();
				}
			});

		switch (input.ScanCode)
		{
			case ScanCode.Up:
			case ScanCode.Down:
			case ScanCode.Left:
			case ScanCode.Right:
			case ScanCode.PageUp:
			case ScanCode.PageDown:
			case ScanCode.Home:
			case ScanCode.End:
			{
				if (input.Modifiers.CtrlKey)
				{
					switch (input.ScanCode)
					{
						// Ctrl-Up, Ctrl-Down: scroll viewport
						case ScanCode.Up: newScrollY--; priority = Priority.Scroll; break;
						case ScanCode.Down: newScrollY++; priority = Priority.Scroll; break;
						// Ctrl-Left, Ctrl-Right: previous/next word
						case ScanCode.Left: break; // TODO: stash rendered buffer but keep Changed=false
						case ScanCode.Right: break; // TODO: cross multiple lines if necessary
						// Ctrl-PageUp, Ctrl-PageDown: page left/right
						case ScanCode.PageUp: newScrollX -= viewportWidth - 1; newCursorX -= viewportWidth - 1; break;
						case ScanCode.PageDown: newScrollX += viewportWidth - 1; newCursorX += viewportWidth - 1; break;
						// Ctrl-Home, Ctrl-End: start/end of document
						case ScanCode.Home: newCursorX = 0; newCursorY = 0; break;
						case ScanCode.End: newCursorX = 0; newCursorY = contentLineCount; break;
					}
				}
				else
				{
					switch (input.ScanCode)
					{
						// Up, Down, Left, Right: cursor movement
						case ScanCode.Up: newCursorY--; break;
						case ScanCode.Down: newCursorY++; break;
						case ScanCode.Left: newCursorX--; break;
						case ScanCode.Right: newCursorX++;  break;
						// PageUp, PageDown: page up/down
						case ScanCode.PageUp: newScrollY -= viewportHeight - 1; newCursorY -= viewportHeight - 1; break;
						case ScanCode.PageDown: newScrollY += viewportHeight - 1; newCursorY += viewportHeight - 1; break;
						// Home, End: start/end of line
						case ScanCode.Home: newCursorX = 0; break;
						case ScanCode.End: newCursorX = currentLine.Value.Length; break;
					}
				}

				break;
			}
			case ScanCode.Return:
			{
				select = false;

				if (input.Modifiers.CtrlKey)
				{
					// Ctrl-Enter: Do not insert newline.
					newCursorY++;
					newCursorX = 0;
				}
				else
				{
					var buffer = currentLine.Value;

					StringBuilder newLine = new StringBuilder();

					if (FocusedViewport.CursorX < buffer.Length)
					{
						// Enter mid-line: Split lines
						newLine = new StringBuilder();
						newLine.Append(buffer, FocusedViewport.CursorX, buffer.Length - FocusedViewport.CursorX);

						buffer.Remove(FocusedViewport.CursorX, buffer.Length - FocusedViewport.CursorX);
					}

					// Step 1: Try to commit left part
					try
					{
						FocusedViewport.CommitCurrentLine();
					}
					catch
					{
						// No syntax checking applied in this case
					}

					// Step 2: Insert right part as new line being edited
					FocusedViewport.CursorY++;
					FocusedViewport.CursorX = 0;
					FocusedViewport.InsertLine(FocusedViewport.CursorY, new CodeLine());

					contentLineCount++;

					FocusedViewport.CurrentLineBuffer = newLine;
					FocusedViewport.CurrentLineChanged = true;
				}

				newCursorX = 0;
				newCursorY = FocusedViewport.CursorY;

				break;
			}
			case ScanCode.Insert:
			case ScanCode.CtrlInsert:
			{
				if (input.Modifiers.CtrlKey && !input.Modifiers.ShiftKey && !input.Modifiers.AltKey)
				{
					FocusedViewport.Clipboard.Copy();
					select = true;
				}
				else if (input.Modifiers.ShiftKey && !input.Modifiers.CtrlKey && !input.Modifiers.AltKey)
				{
					if (FocusedViewport.Clipboard.HasSelection)
					{
						FocusedViewport.Clipboard.Delete();
						newCursorX = FocusedViewport.CursorX;
					}

					FocusedViewport.Clipboard.Paste();
					select = false;
				}
				else
					EnableOvertype = !EnableOvertype;

				break;
			}
			case ScanCode.Delete:
			case ScanCode.CtrlDelete:
			{
				select = false;

				if (FocusedViewport.Clipboard.HasSelection)
				{
					if (input.Modifiers.ShiftKey && !input.Modifiers.CtrlKey && !input.Modifiers.AltKey)
						FocusedViewport.Clipboard.Cut();
					else
						FocusedViewport.Clipboard.Delete();

					newCursorX = FocusedViewport.CursorX;
					newCursorY = FocusedViewport.CursorY;

					break;
				}

				var buffer = currentLine.Value;

				if (FocusedViewport.CursorX < buffer.Length)
				{
					buffer.Remove(FocusedViewport.CursorX, 1);
					FocusedViewport.CurrentLineChanged = true;
				}
				else
				{
					// Delete at end of line: join lines
					if (FocusedViewport.CursorY + 1 < contentLineCount)
					{
						var nextLine = new StringWriter();

						FocusedViewport.RenderLine(FocusedViewport.CursorY + 1, nextLine);

						buffer.Append(nextLine.ToString());

						FocusedViewport.DeleteLine(FocusedViewport.CursorY + 1);
					}
				}

				break;
			}
			case ScanCode.Backspace:
			{
				select = false;

				if (input.Modifiers.CtrlKey)
					goto case ScanCode.Delete;

				if (FocusedViewport.IsEditable)
				{
					FocusedViewport.Clipboard.CancelSelection();

					var buffer = currentLine.Value;

					if (FocusedViewport.CursorX > 0)
					{
						newCursorX--;
						buffer.Remove(newCursorX, 1);
						FocusedViewport.CurrentLineBuffer = buffer;
						FocusedViewport.CurrentLineChanged = true;
					}
					else if (FocusedViewport.CursorY > 0)
					{
						// Backspace at start of line: join lines
						string lineToCollapse = buffer.ToString();

						newCursorY--;

						FocusedViewport.CursorY = newCursorY;
						FocusedViewport.CurrentLineBuffer = null;

						buffer = FocusedViewport.EditCurrentLine();

						newCursorX = buffer.Length;

						buffer.Append(lineToCollapse);

						FocusedViewport.DeleteLine(FocusedViewport.CursorY);
						FocusedViewport.CurrentLineBuffer = buffer;
						FocusedViewport.CurrentLineChanged = true;
					}
				}

				break;
			}
			default:
			{
				if (input.IsNormalText && FocusedViewport.IsEditable)
				{
					select = false;

					string inputText = input.TextCharacter.ToString();

					var buffer = currentLine.Value;

					while (buffer.Length < FocusedViewport.CursorX)
						buffer.Append(' ');

					if (EnableOvertype)
					{
						int replaceCount = inputText.Length;

						if (FocusedViewport.CursorX + replaceCount > buffer.Length)
							replaceCount = buffer.Length - FocusedViewport.CursorX;

						buffer.Remove(FocusedViewport.CursorX, replaceCount);
					}

					buffer.Insert(FocusedViewport.CursorX, inputText);
					newCursorX += inputText.Length;

					FocusedViewport.CurrentLineChanged = true;
					FocusedViewport.CurrentLineBuffer = buffer;
				}

				break;
			}
		}

		void ClampCursorToDocument()
		{
			if (newCursorX < 0)
				newCursorX = 0;
			if (newCursorY < 0)
				newCursorY = 0;
			if (newCursorY > contentLineCount)
				newCursorY = contentLineCount;
		}

		void ClampCursorToViewportScroll()
		{
			if (newCursorX < newScrollX)
				newCursorX = newScrollX;
			if (newCursorX >= newScrollX + viewportWidth)
				newCursorX = newScrollX + viewportWidth - 1;
			if (newCursorY < newScrollY)
				newCursorY = newScrollY;
			if (newCursorY >= newScrollY + viewportHeight)
				newCursorY = newScrollY + viewportHeight - 1;
		}

		void ClampViewportScrollToCursor()
		{
			if (newCursorX < newScrollX)
				newScrollX = newCursorX;
			if (newCursorX >= newScrollX + viewportWidth)
				newScrollX = newCursorX - viewportWidth + 1;
			if (newCursorY < newScrollY)
				newScrollY = newCursorY;
			if (newCursorY >= newScrollY + viewportHeight)
				newScrollY = newCursorY - viewportHeight + 1;
		}

		ClampCursorToDocument();

		if (priority == Priority.Scroll)
		{
			ClampCursorToViewportScroll();
			ClampCursorToDocument();
		}

		ClampViewportScrollToCursor();

		if ((newCursorY != FocusedViewport.CursorY) && FocusedViewport.CurrentLineChanged)
		{
			try
			{
				FocusedViewport.CommitCurrentLine();
			}
			catch
			{
				if (Configuration.EnableSyntaxChecking)
				{
					// TODO: raise error
					newCursorY = FocusedViewport.CursorY;
				}
			}
		}

		if (!select && !input.IsModifierKey)
			FocusedViewport.Clipboard.StartSelection(newCursorX, newCursorY);
		else
			FocusedViewport.Clipboard.ExtendSelection(newCursorX, newCursorY);

		FocusedViewport.CursorX = newCursorX;
		FocusedViewport.CursorY = newCursorY;
		FocusedViewport.ScrollX = newScrollX;
		FocusedViewport.ScrollY = newScrollY;
	}
}
