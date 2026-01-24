using System;
using System.IO;
using System.Text;

using QBX.CodeModel;
using QBX.ExecutionEngine;
using QBX.Hardware;
using QBX.Parser;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
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

		int newCursorX, newCursorY;
		int newScrollX, newScrollY;

		var priority = ViewportPositioningPriority.Cursor;
		bool select = input.Modifiers.ShiftKey;

		int contentLineCount;

		int viewportWidth;
		int viewportHeight;

		Lazy<StringBuilder> ResetCurrentLine() =>
			new Lazy<StringBuilder>(
				() =>
				{
					if (FocusedViewport == null)
						throw new Exception("Internal error: No focused viewport");

					if (FocusedViewport.CurrentLineBuffer == null)
					{
						var writer = new StringWriter();

						FocusedViewport.RenderLine(newCursorY, writer);
						FocusedViewport.CurrentLineBuffer = writer.GetStringBuilder();
					}

					return FocusedViewport.CurrentLineBuffer;
				});

		Lazy<StringBuilder> currentLine;

		void ReloadViewportParameters()
		{
			newCursorX = FocusedViewport.CursorX;
			newCursorY = FocusedViewport.CursorY;
			newScrollX = FocusedViewport.ScrollX;
			newScrollY = FocusedViewport.ScrollY;

			contentLineCount = FocusedViewport.GetContentLineCount();

			viewportWidth = TextLibrary.Width - 2;
			viewportHeight = FocusedViewport.CachedContentHeight;

			if (viewportHeight == 0)
				viewportHeight = FocusedViewport.Height - 2;

			currentLine = ResetCurrentLine();
		}

		ReloadViewportParameters();

		bool CursorLeftWithWrap()
		{
			newCursorX--;

			if (newCursorX < 0)
			{
				if (newCursorY == 0)
				{
					newCursorX = 0;
					return false;
				}

				newCursorY--;

				try
				{
					FocusedViewport.CommitCurrentLine();
				}
				catch
				{
					if (Configuration.EnableSyntaxChecking)
					{
						// TODO: raise error
						newCursorY++;
						throw;
					}
				}

				currentLine = ResetCurrentLine();
				newCursorX = currentLine.Value.Length;
			}

			return true;
		}

		bool CursorRightWithWrap()
		{
			newCursorX++;

			if (newCursorX > currentLine.Value.Length)
			{
				if (newCursorY >= contentLineCount)
				{
					newCursorX--;
					return false;
				}

				newCursorY++;

				try
				{
					FocusedViewport.CommitCurrentLine();
				}
				catch
				{
					if (Configuration.EnableSyntaxChecking)
					{
						// TODO: raise error
						newCursorY--;
						throw;
					}
				}

				currentLine = ResetCurrentLine();
				newCursorX = 0;
			}

			return true;
		}

		bool NewCharacterIsWordCharacter()
		{
			var buffer = currentLine.Value;

			return
				(newCursorX >= 0) && (newCursorX < buffer.Length) &&
				(char.IsLetterOrDigit(buffer[newCursorX]) || buffer[newCursorX] == '.');
		}

		void FindPreviousWord()
		{
			FocusedViewport.CurrentLineBuffer = currentLine.Value;

			try
			{
				CursorLeftWithWrap();

				while (!NewCharacterIsWordCharacter())
				{
					if (!CursorLeftWithWrap())
						return;
				}

				while (NewCharacterIsWordCharacter())
				{
					if (!CursorLeftWithWrap())
						return;
				}

				FindNextWord();
			}
			catch { }
		}

		void FindNextWord()
		{
			FocusedViewport.CurrentLineBuffer = currentLine.Value;

			try
			{
				while (NewCharacterIsWordCharacter())
				{
					if (!CursorRightWithWrap())
						return;
				}

				while (!NewCharacterIsWordCharacter())
				{
					if (!CursorRightWithWrap())
						return;
				}
			}
			catch { }
		}

		input = input.NormalizeModifierCombinationKey();

		switch (input.ScanCode)
		{
			case ScanCode.F4:
			{
				RestoreOutput();

				WaitForKey();

				SetIDEVideoMode();

				break;
			}
			case ScanCode.F5:
			{
				Machine.Keyboard.SuppressNextEventIf(isRelease: true, ScanCode.F5);

				try
				{
					PrimaryViewport.CommitCurrentLine();
					SplitViewport?.CommitCurrentLine();

					if (input.Modifiers.ShiftKey)
						Run();
					else
						Continue();

					ReloadViewportParameters();
				}
				catch (SyntaxErrorException error)
				{
					PresentError(error);
				}
				catch (CompilerException error)
				{
					PresentError(error);
				}
				catch (RuntimeException error)
				{
					PresentError(error);
				}
				catch
				{
					// TODO: present error
				}

				break;
			}
			case ScanCode.F8:
			{
				Machine.Keyboard.SuppressNextEventIf(isRelease: true, ScanCode.F8);

				try
				{
					PrimaryViewport.CommitCurrentLine();
					SplitViewport?.CommitCurrentLine();

					Step();

					ReloadViewportParameters();
				}
				catch (RuntimeException error)
				{
					PresentError(error);
				}
				catch
				{
					// TODO: present error
				}

				break;
			}

			case ScanCode.F9:
			{
				if (input.Modifiers.CtrlKey || input.Modifiers.AltKey)
					break;

				if (input.Modifiers.ShiftKey)
				{
					// TODO: quick watch
				}
				else
				{
					if (FocusedViewport.TryGetCodeLineAt(FocusedViewport.CursorY) is CodeLine currentCodeLine)
						ToggleBreakpoint(currentCodeLine);
				}

				break;
			}

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
						case ScanCode.Up: newScrollY--; priority = ViewportPositioningPriority.Scroll; break;
						case ScanCode.Down: newScrollY++; priority = ViewportPositioningPriority.Scroll; break;
						// Ctrl-Left, Ctrl-Right: previous/next word
						case ScanCode.Left: FindPreviousWord(); break;
						case ScanCode.Right: FindNextWord(); break;
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

				var buffer = currentLine.Value;

				int indentation = 0;
				while ((indentation < buffer.Length) && (buffer[indentation] == ' '))
					indentation++;

				if (input.Modifiers.CtrlKey)
				{
					// Ctrl-Enter: Do not insert newline.
					FocusedViewport.CursorY++;
				}
				else
				{
					StringBuilder newLine = new StringBuilder();

					if (FocusedViewport.CursorX < buffer.Length)
					{
						// Enter mid-line: Split lines
						newLine = new StringBuilder();
						newLine.Append(buffer, FocusedViewport.CursorX, buffer.Length - FocusedViewport.CursorX);

						buffer.Remove(FocusedViewport.CursorX, buffer.Length - FocusedViewport.CursorX);

						FocusedViewport.CurrentLineBuffer = buffer;
						FocusedViewport.CurrentLineChanged = true;
					}

					// Step 1: Try to commit left part
					try
					{
						FocusedViewport.CommitCurrentLine();
					}
					catch
					{
						// No syntax checking applied in this case
						FocusedViewport.ReplaceCurrentLine(CodeLine.CreateUnparsed(buffer));
					}

					// Step 2: Insert right part as new line being edited
					FocusedViewport.CursorY++;
					FocusedViewport.InsertLine(FocusedViewport.CursorY, new CodeLine());

					contentLineCount++;

					FocusedViewport.CurrentLineBuffer = newLine;
					FocusedViewport.CurrentLineChanged = true;
				}

				newCursorX = indentation;
				newCursorY = FocusedViewport.CursorY;

				if (newCursorX < newScrollX)
					newScrollX = 0;

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
					FocusedViewport.CurrentLineBuffer = buffer;
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

						FocusedViewport.CurrentLineBuffer = buffer;
						FocusedViewport.CurrentLineChanged = true;
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
						if (buffer.Length == 0)
							newCursorX = 0;
						else
						{
							newCursorX--;

							if (newCursorX < buffer.Length)
								buffer.Remove(newCursorX, 1);
						}

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
				if ((input.ScanCode == ScanCode.N)
				 && input.Modifiers.CtrlKey)
				{
					PrimaryViewport.CancelEdit();
					SplitViewport?.CancelEdit();
					StartNewProgram();

					newCursorX = FocusedViewport.CursorX;
					newCursorY = FocusedViewport.CursorY;
					newScrollX = FocusedViewport.ScrollX;
					newScrollY = FocusedViewport.ScrollY;
				}

				if ((input.ScanCode == ScanCode.S)
				 && input.Modifiers.CtrlKey)
				{
					// HAX: scan until we find the DebugCases folder off of a parent
					//      folder. only needed until we have a save dialog.
					string? FindDebugCasesPath()
					{
						string? path = Environment.CurrentDirectory;

						while (path != null)
						{
							string testPath = Path.Combine(path, "DebugCases");

							if (Directory.Exists(testPath))
								return testPath;

							path = Path.GetDirectoryName(path);
						}

						return null;
					}

					SaveFile(
						LoadedFiles[0],
						Path.Combine(
							FindDebugCasesPath() ?? ".",
							"NEWCASE.BAS"));
					break;
				}

				if (input.IsNormalText && FocusedViewport.IsEditable)
				{
					select = false;

					if (FocusedViewport.Clipboard.HasSelection)
					{
						FocusedViewport.Clipboard.Delete();
						newCursorX = FocusedViewport.CursorX;
					}

					string inputText = input.TextCharacter.ToString();

					var buffer = currentLine.Value;

					while (buffer.Length < newCursorX)
						buffer.Append(' ');

					if (EnableOvertype)
					{
						int replaceCount = inputText.Length;

						if (newCursorX + replaceCount > buffer.Length)
							replaceCount = buffer.Length - newCursorX;

						buffer.Remove(newCursorX, replaceCount);
					}

					buffer.Insert(newCursorX, inputText);
					newCursorX += inputText.Length;

					FocusedViewport.CurrentLineChanged = true;
					FocusedViewport.CurrentLineBuffer = buffer;
				}

				break;
			}
		}

		try
		{
			FocusedViewport.ScrollCursorIntoView(newCursorX, newCursorY, newScrollX, newScrollY, priority, viewportWidth);
		}
		catch (SyntaxErrorException error)
		{
			if (Configuration.EnableSyntaxChecking)
				PresentError(error);
		}

		if (!select && !input.IsModifierKey)
			FocusedViewport.Clipboard.StartSelection(FocusedViewport.CursorX, FocusedViewport.CursorY);
		else
			FocusedViewport.Clipboard.ExtendSelection(FocusedViewport.CursorX, FocusedViewport.CursorY);
	}

	public void NavigateTo(CompilationElement element, int lineNumber, int column)
	{
		Viewport viewport;

		if (PrimaryViewport.CompilationElement == element)
			viewport = PrimaryViewport;
		else if (SplitViewport?.CompilationElement == element)
			viewport = SplitViewport;
		else
			viewport = PrimaryViewport;

		FocusedViewport = viewport;

		FocusedViewport.SwitchTo(element);

		if (lineNumber >= element.Lines.Count)
			lineNumber = element.Lines.Count - 1;
		if (lineNumber < 0)
			lineNumber = 0;

		viewport.CursorX = column;
		viewport.CursorY = lineNumber;
	}
}
