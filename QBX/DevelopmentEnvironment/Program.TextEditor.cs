using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using QBX.CodeModel;
using QBX.CodeModel.Statements;
using QBX.DevelopmentEnvironment.Dialogs;
using QBX.Hardware;
using QBX.LexicalAnalysis;
using QBX.Parser;
using QBX.Utility;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	bool _alreadyPresentedError = false;
	TextEditorChordType _inTextEditorChord = TextEditorChordType.None;
	bool _performCutAfterRender;

	enum TextEditorChordType
	{
		None,

		CtrlP, // quote character
		CtrlQ, // editor shortcut
		CtrlK, // set bookmark
	}

	enum TextEditorAction
	{
		None,

		Backspace,
		Beep,
		BegLine,
		BegPgm,
		Cancel,
		Change,
		CharLeft,
		CharRight,
		Copy,
		CutSelected,
		CutCurrent,
		CutToEOL,
		Del,
		DelWord,
		DoQuoteCharacter,
		DoTab,
		EndLine,
		EndPgm,
		EndScn,
		Find,
		GotoBookMark0,
		GotoBookMark1,
		GotoBookMark2,
		GotoBookMark3,
		HomeLine,
		HomeScn,
		LineDown,
		LineUp,
		Menu,
		NewLine,
		NextLine,
		PageDown,
		PageLeft,
		PageRight,
		PageUp,
		Paste,
		Redo,
		ScrollDown,
		ScrollUp,
		SetBookMark0,
		SetBookMark1,
		SetBookMark2,
		SetBookMark3,
		SplitLine,
		ToggleInsertMode,
		Undo,
		WordLeft,
		WordRight,
	}

	void ProcessTextEditorKey(KeyEvent input)
	{
		if (input.IsRelease)
			return;

		if (_performCutAfterRender)
			TextEditorAfterRender();

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
					if (FocusedViewport.CurrentLineBuffer == null)
					{
						var writer = new StringWriter();

						FocusedViewport.RenderLine(newCursorY, writer);
						FocusedViewport.CurrentLineBuffer = writer.GetStringBuilder();
					}

					_alreadyPresentedError = false;

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

		bool isNormalText = input.IsNormalText; // can be overridden by ^P
		string? inputText = null;

		TextEditorAction action = TextEditorAction.None;

		if (input.ScanCode == ScanCode.Alt)
			action = TextEditorAction.Menu;

		var wasInChord = _inTextEditorChord;

		if (_inTextEditorChord != TextEditorChordType.None)
		{
			_inTextEditorChord = TextEditorChordType.None;

			isNormalText = false;

			switch (input.ScanCode)
			{
				// Some function keys do not cancel chord!
				case ScanCode.F1:
				case ScanCode.F2:
				case ScanCode.F3:
				case ScanCode.F4:
				case ScanCode.F6:
				case ScanCode.F9:
					_inTextEditorChord = wasInChord;
					break;
			}

			switch (wasInChord)
			{
				case TextEditorChordType.CtrlK:
				{
					switch (input.ScanCode)
					{
						case ScanCode._0: action = TextEditorAction.SetBookMark0; break;
						case ScanCode._1: action = TextEditorAction.SetBookMark1; break;
						case ScanCode._2: action = TextEditorAction.SetBookMark2; break;
						case ScanCode._3: action = TextEditorAction.SetBookMark3; break;
					}

					break;
				}
				case TextEditorChordType.CtrlP:
				{
					if (input.HasTextCharacter)
					{
						switch (input.TextCharacter)
						{
							case (char)10:
							case (char)13:
								_inTextEditorChord = TextEditorChordType.CtrlP;
								action = TextEditorAction.Beep;
								break;
							default:
								isNormalText = true;
								break;
						}
					}
					else
					{
						switch (input.ScanCode)
						{
							case ScanCode.Return:
							case ScanCode.Delete:
								_inTextEditorChord = TextEditorChordType.CtrlP;
								action = TextEditorAction.Beep;
								break;

							case ScanCode.F12: inputText = "{"; break;
							case ScanCode.Backspace: inputText = "\x08"; break;
							case ScanCode.Tab: inputText = "\x09"; break;
							case ScanCode.Insert: inputText = "-"; break;
							case ScanCode.Home: inputText = "$"; break;
							case ScanCode.PageUp: inputText = "!"; break;
							case ScanCode.End: inputText = "#"; break;
							case ScanCode.PageDown: inputText = "\""; break;
							case ScanCode.Up: inputText = "&"; break;
							case ScanCode.Down: inputText = "("; break;
							case ScanCode.Left: inputText = "%"; break;
							case ScanCode.Right: inputText = "'"; break;

							case ScanCode.Kp5:
								if (input.Modifiers.NumLock)
									inputText = "5";
								else
									inputText = "\x0C";
								break;

							default:
								_inTextEditorChord = wasInChord;
								break;
						}
					}

					if (inputText != null)
						isNormalText = true;

					break;
				}
				case TextEditorChordType.CtrlQ:
				{
					if (input.HasTextCharacter)
					{
						switch (input.TextCharacter)
						{
							case (char)('A' - 64): action = TextEditorAction.Change; break;
							case (char)('C' - 64): action = TextEditorAction.EndPgm; break;
							case (char)('D' - 64): action = TextEditorAction.EndLine; break;
							case (char)('E' - 64): action = TextEditorAction.HomeScn; break;
							case (char)('F' - 64): action = TextEditorAction.Find; break;
							case (char)('L' - 64): action = TextEditorAction.Undo; break;
							case (char)('R' - 64): action = TextEditorAction.BegPgm; break;
							case (char)('S' - 64): action = TextEditorAction.BegLine; break;
							case (char)('X' - 64): action = TextEditorAction.EndScn; break;
							case (char)('Y' - 64): action = TextEditorAction.CutToEOL; break;
						}
					}
					else
					{
						switch (input.ScanCode)
						{
							case ScanCode._0: action = TextEditorAction.GotoBookMark0; break;
							case ScanCode._1: action = TextEditorAction.GotoBookMark1; break;
							case ScanCode._2: action = TextEditorAction.GotoBookMark2; break;
							case ScanCode._3: action = TextEditorAction.GotoBookMark3; break;

							default:
								_inTextEditorChord = wasInChord;
								break;
						}
					}

					break;
				}
			}
		}

		if ((action == TextEditorAction.None) && (wasInChord == TextEditorChordType.None))
		{
			switch (input.TextCharacter)
			{
				case (char)('A' - 64): action = TextEditorAction.WordLeft; break;
				case (char)('B' - 64): action = TextEditorAction.Beep; break;
				case (char)('C' - 64): action = TextEditorAction.PageDown; break;
				case (char)('D' - 64): action = TextEditorAction.CharRight; break;
				case (char)('E' - 64): action = TextEditorAction.LineUp; break;
				case (char)('F' - 64): action = TextEditorAction.WordRight; break;
				case (char)('G' - 64): action = TextEditorAction.Del; break;
				case (char)('H' - 64): action = TextEditorAction.Backspace; break;
				case (char)('J' - 64): action = TextEditorAction.NextLine; break;
				case (char)('K' - 64): _inTextEditorChord = TextEditorChordType.CtrlK; break;
				case (char)('N' - 64): action = TextEditorAction.SplitLine; break;
				case (char)('P' - 64): _inTextEditorChord = TextEditorChordType.CtrlP; break;
				case (char)('Q' - 64): _inTextEditorChord = TextEditorChordType.CtrlQ; break;
				case (char)('R' - 64): action = TextEditorAction.PageUp; break;
				case (char)('S' - 64): action = TextEditorAction.CharLeft; break;
				case (char)('T' - 64): action = TextEditorAction.DelWord; break;
				case (char)('V' - 64): action = TextEditorAction.ToggleInsertMode; break;
				case (char)('W' - 64): action = TextEditorAction.ScrollUp; break;
				case (char)('X' - 64): action = TextEditorAction.LineDown; break;
				case (char)('Y' - 64): action = TextEditorAction.CutCurrent; break;
				case (char)('Z' - 64): action = TextEditorAction.ScrollDown; break;
			}
		}

		if (action == TextEditorAction.None)
		{
			if (isNormalText && (input.ScanCode != ScanCode.Backspace))
			{
				select = false;

				if (FocusedViewport.IsEditable)
				{
					if (FocusedViewport.SelectionManager.HasSelection)
					{
						FocusedViewport.SelectionManager.Delete();
						newCursorX = FocusedViewport.CursorX;
					}

					inputText ??= input.TextCharacter.ToString();

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

					FocusedViewport.CurrentLineEdited = true;
					FocusedViewport.CurrentLineBuffer = buffer;

					_alreadyPresentedError = false;
				}
			}
			else
			{
				if (action == TextEditorAction.None)
				{
					switch (input.ScanCode)
					{
						case ScanCode.F1:
						{
							if (!input.Modifiers.CtrlKey)
							{
								if (input.Modifiers.ShiftKey)
								{
									ShowHelpTopic("bas7qck.hlp!h.default");
									ReloadViewportParameters();
									select = false;
								}
								else if (TryShowHelpTopicForTokenUnderCursor())
									ReloadViewportParameters();
							}

							break;
						}
						case ScanCode.F2:
						{
							if (!input.Modifiers.CtrlKey && !input.Modifiers.AltKey)
							{
								PromptTerminateToCommitEdit(
									() =>
									{
										try
										{
											FocusedViewport.CommitCurrentLine();
										}
										catch { }

										if (input.Modifiers.ShiftKey)
											SwitchToNextElement();
										else
											ShowSubsDialog();
									});
							}

							break;
						}
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

							PromptTerminateToCommitEdit(
								() =>
								{
									if (CommitViewportsOrPresentError())
									{
										if (input.Modifiers.ShiftKey)
											Run();
										else
											Continue();

										ReloadViewportParameters();
									}
								});

							break;
						}
						case ScanCode.F6:
						{
							if (input.Modifiers.CtrlKey || input.Modifiers.AltKey)
								break;

							PromptTerminateToCommitEdit(
								() =>
								{
									try
									{
										FocusedViewport.CommitCurrentLine();
									}
									catch { }

									if (input.Modifiers.ShiftKey == false)
									{
										// Forward
										if (FocusedViewport == HelpViewport)
											FocusedViewport = PrimaryViewport;
										else if (FocusedViewport == PrimaryViewport)
											FocusedViewport = SplitViewport ?? ImmediateViewport;
										else if (FocusedViewport == SplitViewport)
											FocusedViewport = ImmediateViewport;
										else if (FocusedViewport == ImmediateViewport)
											FocusedViewport = HelpViewport ?? PrimaryViewport;
									}
									else
									{
										// Backward
										if (FocusedViewport == HelpViewport)
											FocusedViewport = ImmediateViewport;
										else if (FocusedViewport == PrimaryViewport)
											FocusedViewport = HelpViewport ?? ImmediateViewport;
										else if (FocusedViewport == SplitViewport)
											FocusedViewport = PrimaryViewport;
										else if (FocusedViewport == ImmediateViewport)
											FocusedViewport = SplitViewport ?? PrimaryViewport;
									}

									ReloadViewportParameters();
								});

							break;
						}
						case ScanCode.F8:
						{
							Machine.Keyboard.SuppressNextEventIf(isRelease: true, ScanCode.F8);

							PromptTerminateToCommitEdit(
								() =>
								{
									if (CommitViewportsOrPresentError())
									{
										Step();

										ReloadViewportParameters();
									}
								});

							break;
						}

						case ScanCode.F9:
						{
							if (input.Modifiers.CtrlKey || input.Modifiers.AltKey)
								break;

							if (input.Modifiers.ShiftKey)
								InstantWatchAtCurrentCursorLocation();
							else
							{
								if (FocusedViewport.TryGetCodeLineAt(FocusedViewport.CursorY, out var currentCodeLine))
									ToggleBreakpoint(currentCodeLine);
							}

							break;
						}

						case ScanCode.F11:
						{
							action = TextEditorAction.Menu;
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
									case ScanCode.Up: action = TextEditorAction.ScrollUp; break;
									case ScanCode.Down: action = TextEditorAction.ScrollDown; break;
									// Ctrl-Left, Ctrl-Right: previous/next word
									case ScanCode.Left: action = TextEditorAction.WordLeft; break;
									case ScanCode.Right: action = TextEditorAction.WordRight; break;
									// Ctrl-PageUp, Ctrl-PageDown: page left/right
									case ScanCode.PageUp: action = TextEditorAction.PageLeft; break;
									case ScanCode.PageDown: action = TextEditorAction.PageRight; break;
									// Ctrl-Home, Ctrl-End: start/end of document
									case ScanCode.Home: action = TextEditorAction.BegPgm; break;
									case ScanCode.End: action = TextEditorAction.EndPgm; break;
								}
							}
							else
							{
								switch (input.ScanCode)
								{
									// Up, Down, Left, Right: cursor movement
									case ScanCode.Up: action = TextEditorAction.LineUp; break;
									case ScanCode.Down: action = TextEditorAction.LineDown; break;
									case ScanCode.Left: action = TextEditorAction.CharLeft; break;
									case ScanCode.Right: action = TextEditorAction.CharRight; break;
									// PageUp, PageDown: page up/down
									case ScanCode.PageUp: action = TextEditorAction.PageUp; break;
									case ScanCode.PageDown: action = TextEditorAction.PageDown; break;
									// Home, End: start/end of line
									case ScanCode.End: action = TextEditorAction.EndLine; break;
									case ScanCode.Home: action = TextEditorAction.HomeLine; break;
								}
							}

							break;
						}
						case ScanCode.Return: action = TextEditorAction.NewLine; break;
						case ScanCode.Escape: action = TextEditorAction.Cancel; break;
						case ScanCode.Tab: action = TextEditorAction.DoTab; break;
						case ScanCode.Insert:
						case ScanCode.CtrlInsert:
						{
							if (input.Modifiers.CtrlKey && !input.Modifiers.ShiftKey && !input.Modifiers.AltKey)
								action = TextEditorAction.Copy;
							else if (input.Modifiers.ShiftKey && !input.Modifiers.CtrlKey && !input.Modifiers.AltKey)
								action = TextEditorAction.Paste;
							else
								action = TextEditorAction.ToggleInsertMode;

							break;
						}
						case ScanCode.Delete:
						case ScanCode.CtrlDelete:
						{
							if (FocusedViewport.IsEditable)
							{
								if (FocusedViewport.SelectionManager.HasSelection)
								{
									if (FocusedViewport.IsEditable)
									{
										if (input.Modifiers.ShiftKey && !input.Modifiers.CtrlKey && !input.Modifiers.AltKey)
											action = TextEditorAction.CutSelected;
										else
											action = TextEditorAction.Del;
									}
									else
										action = TextEditorAction.Cancel;
								}
								else
									action = TextEditorAction.Del;
							}

							break;
						}
						case ScanCode.Backspace:
						{
							if (FocusedViewport.IsEditable && !input.Modifiers.CtrlKey)
								action = TextEditorAction.Backspace;

							break;
						}
					}
				}
			}
		}

		switch (action)
		{
			case TextEditorAction.Beep:
				Machine.DOS.Beep();
				break;

			case TextEditorAction.Menu:
			{
				Mode = UIMode.MenuBar;
				AltReleaseAction = AltRelease.ActivateMenuBar;
				SelectedMenu = -1;

				return;
			}

			case TextEditorAction.Find:
				// TODO: activate find dialog
				break;
			case TextEditorAction.Change:
				// TODO: activate find/replace dialog
				break;

			case TextEditorAction.ToggleInsertMode: 
			{
				EnableOvertype = !EnableOvertype;
				break;
			}

			case TextEditorAction.CharLeft: newCursorX--; break;
			case TextEditorAction.CharRight: newCursorX++; break;
			case TextEditorAction.WordLeft: FindPreviousWord(); break;
			case TextEditorAction.WordRight: FindNextWord(); break;
			case TextEditorAction.LineUp: newCursorY--; break;
			case TextEditorAction.LineDown: newCursorY++; break;
			case TextEditorAction.EndLine: newCursorX = currentLine.Value.Length; break;
			case TextEditorAction.HomeLine: // Home: start of line, factoring in indentation
			{
				var buffer = currentLine.Value;

				newCursorX = 0;

				while ((newCursorX < buffer.Length) && (buffer[newCursorX] == ' '))
					newCursorX++;

				break;
			}
			case TextEditorAction.HomeScn: newCursorY = newScrollY; break;
			case TextEditorAction.EndScn: newCursorY = newScrollY + viewportHeight - 1; break;
			case TextEditorAction.BegPgm: newCursorX = 0; newCursorY = 0; break;
			case TextEditorAction.EndPgm: newCursorX = 0; newCursorY = contentLineCount; break;
			case TextEditorAction.PageLeft: newScrollX -= viewportWidth - 1; newCursorX -= viewportWidth - 1; break;
			case TextEditorAction.PageRight: newScrollX += viewportWidth - 1; newCursorX += viewportWidth - 1; break;
			case TextEditorAction.PageUp: newScrollY -= viewportHeight - 1; newCursorY -= viewportHeight - 1; break;
			case TextEditorAction.PageDown: newScrollY += viewportHeight - 1; newCursorY += viewportHeight - 1; break;
			case TextEditorAction.ScrollUp: newScrollY--; priority = ViewportPositioningPriority.Scroll; break;
			case TextEditorAction.ScrollDown: newScrollY++; priority = ViewportPositioningPriority.Scroll; break;

			case TextEditorAction.GotoBookMark0:
			case TextEditorAction.GotoBookMark1:
			case TextEditorAction.GotoBookMark2:
			case TextEditorAction.GotoBookMark3:
			case TextEditorAction.SetBookMark0:
			case TextEditorAction.SetBookMark1:
			case TextEditorAction.SetBookMark2:
			case TextEditorAction.SetBookMark3:
				// TODO
				break;

			case TextEditorAction.NewLine:
			case TextEditorAction.SplitLine:
			{
				select = false;

				bool moveCursor = (action != TextEditorAction.SplitLine);

				if (FocusedViewport.IsEditable)
				{
					bool savedAlreadyPresentedError = _alreadyPresentedError;

					var buffer = currentLine.Value;

					int indentation = 0;
					while ((indentation < buffer.Length) && (buffer[indentation] == ' '))
						indentation++;

					if ((indentation == buffer.Length) && (newCursorX > indentation))
						indentation = newCursorX;

					if ((FocusedViewport == ImmediateViewport) && moveCursor)
					{
						try
						{
							FocusedViewport.CommitCurrentLine();

							newCursorX = 0;

							if (ParseAndExecuteDirect(ImmediateTextElement.Lines[FocusedViewport.CursorY].Read()))
								newCursorY++;
						}
						catch (Exception ex)
						{
							PresentError(ex);
						}
					}
					else
					{
						PromptTerminateToCommitEdit(
							willMakeChanges: true,
							proceedAction:
								() =>
								{
									StringBuilder newLine = new StringBuilder();

									if (FocusedViewport.CursorX < buffer.Length)
									{
										// Enter mid-line: Split lines
										newLine = new StringBuilder(capacity: indentation + buffer.Length - FocusedViewport.CursorX);

										for (int i = 0; i < indentation; i++)
											newLine.Append(' ');

										newLine.Append(buffer, FocusedViewport.CursorX, buffer.Length - FocusedViewport.CursorX);

										while ((newLine.Length > 0) && char.IsWhiteSpace(newLine[newLine.Length - 1]))
											newLine.Length--;

										buffer.Remove(FocusedViewport.CursorX, buffer.Length - FocusedViewport.CursorX);

										FocusedViewport.CurrentLineBuffer = buffer;
										FocusedViewport.CurrentLineEdited = true;

										if (FocusedViewport.CursorX == 0)
											_alreadyPresentedError = savedAlreadyPresentedError;
									}

									// Step 1: Try to commit left part
									bool commitRightPart = false;

									try
									{
										bool reloadViewport = FocusedViewport.CommitCurrentLine();

										if (reloadViewport)
										{
											ReloadViewportParameters();
											commitRightPart = true;

											if (newLine.Length == 0)
											{
												FocusedViewport.CursorX = 0;
												FocusedViewport.CursorY = 1;

												return;
											}
										}
									}
									catch (Exception exception)
									{
										// No syntax checking applied when splitting an existing line,
										// and if the user tries twice in a row without altering the
										// line, they are allowed to keep it the second time.
										if ((newLine.Length == 0) && !_alreadyPresentedError
											&& Configuration.EnableSyntaxChecking)
										{
											_alreadyPresentedError = true;
											PresentError(exception);
											return;
										}
									}

									// Step 2: Insert right part as new line being edited
									newCursorY++;
									newCursorX = indentation;

									ApplyCursorMovement();

									FocusedViewport.InsertLine(newCursorY, new CodeLine());

									contentLineCount++;

									FocusedViewport.CurrentLineBuffer = newLine;
									FocusedViewport.CurrentLineEdited = true;

									if (commitRightPart)
									{
										try
										{
											FocusedViewport.CommitCurrentLine();
										}
										catch { }
									}
								});

						return;
					}
				}
				else if (FocusedViewport.HelpTopic != null)
				{
					if (!moveCursor)
						goto case TextEditorAction.Beep;

					// Check for a link under the cursor.
					var lineIndex = FocusedViewport.CursorY;

					if ((lineIndex >= 0) && (lineIndex < FocusedViewport.HelpTopic.Lines.Count))
					{
						int cursorX = FocusedViewport.CursorX;

						var line = FocusedViewport.HelpTopic.Lines[lineIndex];

						var link = line.Links?.Find(candidate => (candidate.StartIndex <= cursorX) && (cursorX <= candidate.EndIndex));

						if (link != null)
						{
							if (link.TargetContextString != null)
								ShowHelpTopic(link.TargetContextString);
							else if (link.TargetTopicIndex >= 0)
							{
								var database = FocusedViewport.HelpTopic.Database;

								if (link.TargetTopicIndex < database.Topics.Count)
									ShowHelpTopic(database.Topics[link.TargetTopicIndex]);
							}

							ReloadViewportParameters();
						}
					}
				}

				break;
			}
			case TextEditorAction.NextLine:
			{
				var buffer = currentLine.Value;

				int indentation = 0;
				while ((indentation < buffer.Length) && (buffer[indentation] == ' '))
					indentation++;

				// Ctrl-Enter: Do not insert newline.
				PromptTerminateToCommitEdit(
					() =>
					{
						try
						{
							FocusedViewport.CommitCurrentLine();
							FocusedViewport.CursorY = ++newCursorY;

							if (newCursorY >= FocusedViewport.GetContentLineCount())
								FocusedViewport.InsertLine(newCursorY, new CodeLine());

							currentLine = ResetCurrentLine();

							buffer = currentLine.Value;

							for (int i = 0; i < buffer.Length; i++)
								if (buffer[i] != ' ')
								{
									indentation = i;
									break;
								}

							newCursorX = indentation;
							newCursorY = FocusedViewport.CursorY;

							if (newCursorX < newScrollX)
							{
								newScrollX = newCursorX - 19;

								if (newScrollX < 0)
									newScrollX = 0;
							}

							ApplyCursorMovement();
						}
						catch (Exception e)
						{
							PresentError(e);
						}
					});

				break;
			}
			case TextEditorAction.Cancel:
			{
				if (FocusedViewport == HelpViewport)
				{
					FocusedViewport = PrimaryViewport;
					ReloadViewportParameters();
				}

				HelpViewport = null;

				FocusedViewport.SelectionManager.CancelSelection();

				break;
			}
			case TextEditorAction.Copy:
			{
				FocusedViewport.SelectionManager.Copy();
				select = true;
				break;
			}
			case TextEditorAction.Paste:
			{
				if (FocusedViewport.IsEditable)
				{
					void PerformPaste()
					{
						if (FocusedViewport.SelectionManager.HasSelection)
						{
							FocusedViewport.SelectionManager.Delete();
							newCursorX = FocusedViewport.CursorX;
						}

						FocusedViewport.SelectionManager.Paste();
						select = false;
						_alreadyPresentedError = false;
					}

					if (FocusedViewport.SelectionManager.HasMultilineSelection
						|| FocusedViewport.SelectionManager.HasMultilineClipboardContent)
					{
						PromptTerminateToCommitEdit(willMakeChanges: true, PerformPaste);
						return;
					}
					else
						PerformPaste();
				}

				break;
			}
			case TextEditorAction.CutSelected:
			{
				select = false;

				void PerformCut()
				{
					FocusedViewport.SelectionManager.Cut();

					newCursorX = FocusedViewport.CursorX;
					newCursorY = FocusedViewport.CursorY;

					_alreadyPresentedError = false;
				}

				if (FocusedViewport.SelectionManager.HasMultilineSelection)
				{
					PromptTerminateToCommitEdit(
						willMakeChanges: true,
						() =>
						{
							PerformCut();
							ApplyCursorMovement();
						});

					return;
				}
				else
					PerformCut();

				break;
			}
			case TextEditorAction.CutCurrent:
			{
				if (FocusedViewport.IsEditable && (newCursorY + 1 < FocusedViewport.EditableElement?.Lines.Count))
				{
					PromptTerminateToCommitEdit(
						() =>
						{
							FocusedViewport.SelectionManager.CancelSelection();
							FocusedViewport.SelectionManager.StartSelection(0, newCursorY + 1);
							FocusedViewport.SelectionManager.ExtendSelection(0, newCursorY);

							_performCutAfterRender = true;
						});

					return; // Prevent default cursor movement & selection handling
				}

				break;
			}
			case TextEditorAction.CutToEOL:
			{
				if (FocusedViewport.IsEditable)
				{
					var buffer = FocusedViewport.EditCurrentLine();

					PromptTerminateToCommitEdit(
						() =>
						{
							FocusedViewport.SelectionManager.CancelSelection();
							FocusedViewport.SelectionManager.StartSelection(buffer.Length, newCursorY);
							FocusedViewport.SelectionManager.ExtendSelection(newCursorX, newCursorY);

							_performCutAfterRender = true;
						});

					return; // Prevent default cursor movement & selection handling
				}

				break;
			}
			case TextEditorAction.Backspace:
			{
				select = false;

				if (FocusedViewport.IsEditable && !input.Modifiers.CtrlKey)
				{
					FocusedViewport.SelectionManager.CancelSelection();

					var buffer = currentLine.Value;

					if (FocusedViewport.CursorX > 0)
					{
						int thisLineIndentation = 0;

						while ((thisLineIndentation < buffer.Length) && (buffer[thisLineIndentation] == ' '))
							thisLineIndentation++;

						if ((newCursorX == thisLineIndentation) || (thisLineIndentation == buffer.Length))
						{
							// Backspace at start of line/on empty line: Find preceding indentation level.
							if (thisLineIndentation == buffer.Length)
								thisLineIndentation = newCursorX;

							int previousIndentation = 0;

							for (int i = newCursorY - 1; i >= 0; i--)
							{
								int lineIndent = FocusedViewport.GetLineIndentation(i, out var isEmpty);

								if (isEmpty)
									continue;

								if (lineIndent < thisLineIndentation)
								{
									previousIndentation = lineIndent;
									break;
								}
							}

							int difference = thisLineIndentation - previousIndentation;

							newCursorX -= difference;

							if (newCursorX + difference > buffer.Length)
								difference = buffer.Length - newCursorX;

							if (difference > 0)
								buffer.Remove(newCursorX, difference);
						}
						else
						{
							newCursorX--;

							if (newCursorX < buffer.Length)
								buffer.Remove(newCursorX, 1);
						}

						FocusedViewport.CurrentLineBuffer = buffer;
						FocusedViewport.CurrentLineEdited = true;
					}
					else if (FocusedViewport.CursorY > 0)
					{
						PromptTerminateToCommitEdit(
							willMakeChanges: true,
							() =>
							{
								// Backspace at start of line: join lines
								string lineToCollapse = buffer.ToString();

								newCursorY = newCursorY - 1;

								ApplyCursorMovement();

								FocusedViewport.CurrentLineBuffer = null;

								buffer = FocusedViewport.EditCurrentLine();

								newCursorX = buffer.Length;

								ApplyCursorMovement();

								buffer.Append(lineToCollapse);

								FocusedViewport.DeleteLine(FocusedViewport.CursorY);
								FocusedViewport.CurrentLineBuffer = buffer;
								FocusedViewport.CurrentLineEdited = true;
							});

						return;
					}

					_alreadyPresentedError = false;
				}

				break;
			}
			case TextEditorAction.Del:
			{
				select = false;

				if (FocusedViewport.SelectionManager.HasSelection)
				{
					void PerformDelete()
					{
						FocusedViewport.SelectionManager.Delete();

						newCursorX = FocusedViewport.CursorX;
						newCursorY = FocusedViewport.CursorY;
					}

					if (FocusedViewport.SelectionManager.HasMultilineSelection)
					{
						PromptTerminateToCommitEdit(
							willMakeChanges: true,
							() =>
							{
								PerformDelete();
								ApplyCursorMovement();
							});

						return;
					}
					else
						PerformDelete();
				}
				else
				{
					var buffer = currentLine.Value;

					if (FocusedViewport.CursorX < buffer.Length)
					{
						buffer.Remove(FocusedViewport.CursorX, 1);
						FocusedViewport.CurrentLineBuffer = buffer;
						FocusedViewport.CurrentLineEdited = true;
					}
					else
					{
						// Delete at end of line: join lines
						if (FocusedViewport.CursorY + 1 < contentLineCount)
						{
							PromptTerminateToCommitEdit(
								willMakeChanges: true,
								() =>
								{
									var nextLine = new StringWriter();

									FocusedViewport.RenderLine(FocusedViewport.CursorY + 1, nextLine);

									while (buffer.Length < FocusedViewport.CursorX)
										buffer.Append(' ');

									var nextLineBuffer = nextLine.GetStringBuilder();

									int firstNonSpace = 0;

									for (int i = 0; i < nextLineBuffer.Length; i++)
										if (nextLineBuffer[i] != ' ')
										{
											firstNonSpace = i;
											break;
										}

									buffer.Append(nextLineBuffer, firstNonSpace, nextLineBuffer.Length - firstNonSpace);

									FocusedViewport.DeleteLine(FocusedViewport.CursorY + 1);

									FocusedViewport.CurrentLineBuffer = buffer;
									FocusedViewport.CurrentLineEdited = true;
								});
						}
					}
				}

				break;
			}
			case TextEditorAction.DelWord:
			{
				// TODO

				break;
			}
			case TextEditorAction.DoTab:
			{
				if (!input.Modifiers.ShiftKey)
				{
					// Tab:
					// - If no block selection, insert spaces until CursorX is a multiple of 8.
					// - If block selection, indent all selected lines by the tab size.

					var element = FocusedViewport.EditableElement;

					if (element == null)
						break; // ?

					try
					{
						if (FocusedViewport.CurrentLineEdited)
							FocusedViewport.CommitCurrentLine();
					}
					catch { }

					if (!FocusedViewport.SelectionManager.HasMultilineSelection)
					{
						var buffer = currentLine.Value;

						int spacesToAdd = 0;
						int insertionPoint = FocusedViewport.CursorX;

						if (insertionPoint > buffer.Length)
						{
							spacesToAdd = insertionPoint - buffer.Length;
							insertionPoint = buffer.Length;
						}

						spacesToAdd += ((FocusedViewport.CursorX - 1) & 7) + 1;

						Span<char> spaces = stackalloc char[spacesToAdd];

						spaces.Fill(' ');

						buffer.Insert(insertionPoint, spaces);

						newCursorX += spacesToAdd;
					}
					else
					{
						var range = FocusedViewport.SelectionManager.GetSelectionRange();

						int y1 = Math.Min(range.StartY, range.EndY);
						int y2 = Math.Max(range.StartY, range.EndY);

						if (y2 > element.Lines.Count)
							y2 = element.Lines.Count;

						var buffer = new StringBuilder();
						var bufferWriter = new StringWriter(buffer);

						Span<char> spaces = stackalloc char[Configuration.TabSize];

						spaces.Fill(' ');

						buffer.Append(spaces);

						for (int y = y1; y < y2; y++)
						{
							var line = element.Lines[y];

							buffer.Length = Configuration.TabSize;
							line.Render(bufferWriter);

							element.ReplaceLine(y, element.ConstructLine(buffer));
						}

						// Stay selected
						select = true;
					}
				}
				else
				{
					// Shift-tab:
					// - If no block selection, punt over to backspace from start of line.
					// - If block selection, then:
					//   * If the cursor is on the first line, OR the first line's indent level
					//     matches the least indented line in the block, then deindent all selected
					//     lines so that the first line's indent level matches the preceding indent
					//     level.
					//   * Otherwise, deindent all the lines by the difference between the first
					//     line and the least indented line in the block.

					var element = FocusedViewport.EditableElement;

					if (element == null)
						break; // ?

					if (!FocusedViewport.SelectionManager.HasMultilineSelection)
					{
						newCursorX = FocusedViewport.CursorX = FocusedViewport.GetLineIndentation(FocusedViewport.CursorY);
						goto case TextEditorAction.Backspace;
					}

					try
					{
						if (FocusedViewport.CurrentLineEdited)
							FocusedViewport.CommitCurrentLine();
					}
					catch { }

					var range = FocusedViewport.SelectionManager.GetSelectionRange();

					int y1 = Math.Min(range.StartY, range.EndY);
					int y2 = Math.Max(range.StartY, range.EndY);

					if (y2 > element.Lines.Count)
						y2 = element.Lines.Count;

					int firstLineIndentation = FocusedViewport.GetLineIndentation(y1, out _);

					bool usePreviousIndentation = (FocusedViewport.CursorY == y1);

					int indentationDelta = 0;

					if (!usePreviousIndentation)
					{
						int blockMinimumIndentation = firstLineIndentation;

						for (int y = y1 + 1; y < y2; y++)
						{
							int indentation = FocusedViewport.GetLineIndentation(y);

							if (indentation < blockMinimumIndentation)
								blockMinimumIndentation = indentation;

							if (blockMinimumIndentation == 0)
								break;
						}

						if (firstLineIndentation == blockMinimumIndentation)
							usePreviousIndentation = true;
						else
							indentationDelta = firstLineIndentation - blockMinimumIndentation;
					}

					if (usePreviousIndentation && (firstLineIndentation > 0))
					{
						// Find preceding indentation level.
						int previousIndentation = 0;

						for (int i = y1 - 1; i >= 0; i--)
						{
							int lineIndent = FocusedViewport.GetLineIndentation(i, out var isEmpty);

							if (isEmpty)
								continue;

							if (lineIndent < firstLineIndentation)
							{
								previousIndentation = lineIndent;
								break;
							}
						}

						indentationDelta = firstLineIndentation - previousIndentation;
					}

					if (indentationDelta > 0)
					{
						// Now try to remove indentationDelta spaces from every line in the selection.
						var buffer = new StringBuilder();
						var bufferWriter = new StringWriter(buffer);

						for (int y = y1; y < y2; y++)
						{
							buffer.Length = 0;

							element.Lines[y].Render(bufferWriter);

							int thisLineIndentLevel = 0;

							while ((thisLineIndentLevel < buffer.Length) && (buffer[thisLineIndentLevel] == ' '))
								thisLineIndentLevel++;

							int thisLineSpacesToRemove = Math.Min(thisLineIndentLevel, indentationDelta);

							buffer.Remove(0, thisLineSpacesToRemove);

							element.ReplaceLine(y, element.ConstructLine(buffer));
						}

						// Stay selected
						select = true;
					}
				}

				break;
			}
		}

		ApplyCursorMovement();

		void ApplyCursorMovement()
		{
			try
			{
				FocusedViewport.ScrollCursorIntoView(
					newCursorX, newCursorY,
					newScrollX, newScrollY,
					priority,
					viewportWidth,
					PromptTerminateToCommitEdit,
					ignoreErrors: _alreadyPresentedError || !Configuration.EnableSyntaxChecking);
			}
			catch (Exception exception)
			{
				PresentError(exception);
				_alreadyPresentedError = true;
			}

			if (!select && !input.IsModifierKey)
				FocusedViewport.SelectionManager.StartSelection(FocusedViewport.CursorX, FocusedViewport.CursorY);
			else
				FocusedViewport.SelectionManager.ExtendSelection(FocusedViewport.CursorX, FocusedViewport.CursorY);
		}
	}

	bool TextEditorAfterRender()
	{
		if (_performCutAfterRender)
		{
			_performCutAfterRender = false;

			Thread.Sleep(20);

			FocusedViewport.SelectionManager.Cut();

			return true;
		}

		return false;
	}

	void PromptTerminateToCommitEdit(Action proceedAction)
		=> PromptTerminateToCommitEdit(false, proceedAction);

	void PromptTerminateToCommitEdit(bool willMakeChanges, Action proceedAction)
	{
		bool actualChanges = willMakeChanges || FocusedViewport.RerenderCurrentLineAndCheckForActualChanges();

		if (!actualChanges && FocusedViewport.CurrentLineEdited)
			FocusedViewport.CancelEdit();

		if (!actualChanges || (_executionContext == null))
			proceedAction();
		else
		{
			var dialog = new CannotContinueDialog(Machine, Configuration);

			dialog.Proceed +=
				() =>
				{
					Terminate();
					proceedAction();
				};

			ShowDialog(dialog);
		}
	}

	bool EnsureAllCodeIsParsed(bool presentErrors = true)
	{
		try
		{
			PrimaryViewport.CommitCurrentLine();
			SplitViewport?.CommitCurrentLine();

			var writer = new StringWriter();
			var buffer = writer.GetStringBuilder();

			foreach (var unit in LoadedFiles.OfType<CompilationUnit>().Where(u => u.IncludeInBuild))
			{
				var parser = new BasicParser(unit.IdentifierRepository);

				foreach (var element in unit.Elements)
				{
					for (int i = 0; i < element.Lines.Count; i++)
					{
						if (element.Lines[i] is CodeLine line)
						{
							if (line.AllStatements.OfType<UnparsedStatement>().Any())
							{
								buffer.Clear();

								line.Render(new StringWriter(buffer), includeCRLF: false);

								var lexer = new Lexer(new StringBuilderReader(buffer), element, startingLineNumber: i);

								try
								{
									var parsedCodeLine = parser.ParseCodeLines(lexer).SingleOrDefault();

									element.ReplaceLine(i, parsedCodeLine ?? CodeLine.CreateEmpty());
								}
								catch (SyntaxErrorException error)
								{
									// The error's context needs to link back to the CompilationElement for the IDE to highlight it.
									if (error.Token.OwnerElement == null)
										error.Token.OwnerElement = element;

									throw;
								}
							}
						}
					}
				}
			}

			return true;
		}
		catch (Exception exception)
		{
			if (presentErrors)
				PresentError(exception);
		}

		return false;
	}

	private void SwitchToNextElement()
	{
		IEditableElement? nextElement = null;

		var unit = FocusedViewport.EditableUnit;

		if (unit != null)
		{
			int elementIndex = -1;

			for (int i = 0; i < unit.Elements.Count; i++)
			{
				if (unit.Elements[i] == FocusedViewport.EditableElement)
				{
					elementIndex = i;
					break;
				}
			}

			// Advance to next element.
			elementIndex++;

			if (elementIndex >= unit.Elements.Count)
			{
				int unitIndex = LoadedFiles.IndexOf(unit);

				unitIndex++;

				if (unitIndex >= LoadedFiles.Count)
					unitIndex = 0;

				unit = LoadedFiles[unitIndex];
				elementIndex = 0;
			}

			nextElement = unit.Elements[elementIndex];
		}

		if (nextElement == null)
			nextElement = LoadedFiles[0].Elements[0];

		FocusedViewport.SwitchTo(nextElement);
	}

	private void ShowSubsDialog()
	{
		foreach (var unit in LoadedFiles)
			unit.SortElements();

		string? identifierUnderCursor = null;

		FocusedViewport.EditCurrentLine();

		var buffer = FocusedViewport.CurrentLineBuffer;

		if (buffer != null)
		{
			int startIndex = FocusedViewport.CursorX;

			FindIdentifierExtent(buffer, ref startIndex, out var endIndex);

			if ((startIndex >= 0) && (startIndex < endIndex))
				identifierUnderCursor = buffer.ToString(startIndex, endIndex - startIndex + 1);
		}

		FocusedViewport.CancelEdit();

		var dialog = new SubsDialog(LoadedFiles, Machine, Configuration);

		if ((identifierUnderCursor != null)
		 && (LoadedFiles
		     .SelectMany(unit => unit.Elements)
		     .Select(element => (Element: element, Name: element.DisplayName))
		     .Where(item => (item.Name != null) && item.Name.Value.Equals(identifierUnderCursor, StringComparison.OrdinalIgnoreCase))
		     .Select(item => item.Element)
		     .FirstOrDefault() is IEditableElement element))
			dialog.SelectedItem = element;

		dialog.EditInActive +=
			() =>
			{
				FocusedViewport.SwitchTo(dialog.SelectedItem);
			};

		dialog.EditInSplit +=
			() =>
			{
				if (SplitViewport == null)
					ShowSplitViewport();

				var unfocusedViewport =
					FocusedViewport == PrimaryViewport
					? SplitViewport
					: PrimaryViewport;

				unfocusedViewport.SwitchTo(dialog.SelectedItem);
			};

		dialog.ShowDialog +=
			dialog => ShowDialog(dialog);

		dialog.RemoveElement +=
			() =>
			{
				var element = dialog.SelectedItem;

				element.Owner.RemoveElement(element);
			};

		dialog.PromptToSaveChanges +=
			(configure, continuation, cancellation) =>
			{
				bool wasContinued = false;

				var prompt = PromptToSaveChanges(
					dialog.SelectedItem.Owner,
					() =>
					{
						wasContinued = true;
						continuation();
					});

				if (prompt != null)
				{
					prompt.Closed +=
						(_, _) =>
						{
							if (!wasContinued)
								cancellation();
						};
				}
			};

		dialog.UnloadFile +=
			() =>
			{
				RemoveFile(dialog.SelectedItem.Owner);
			};

		dialog.SelectNewMainModule +=
			(configure, continuation, cancellation) =>
			{
				var dialog = SetMainModule();

				bool wasContinued = false;

				dialog.ModuleSelected +=
					() =>
					{
						wasContinued = true;
						continuation();
					};

				dialog.Closed +=
					(_, _) =>
					{
						if (!wasContinued)
							cancellation();
					};

				configure(dialog);

				ShowDialog(dialog);
			};

		dialog.RestartDialog += ShowSubsDialog;

		ShowDialog(dialog);
	}

	Viewport AttachViewport(Viewport viewport)
	{
		viewport.GetElementByName += viewport_GetElementByName;

		return viewport;
	}

	IEditableElement? viewport_GetElementByName(string name)
	{
		var identifier = Identifier.Standalone(name);

		if (identifier is QualifiedIdentifier qualifiedIdentifier)
			identifier = qualifiedIdentifier.UnqualifiedIdentifier;

		foreach (var unit in LoadedFiles)
		{
			foreach (var element in unit.Elements)
				if (element.DisplayName == identifier)
					return element;
		}

		return null;
	}

	[MemberNotNull(nameof(SplitViewport))]
	void ShowSplitViewport()
	{
		if (SplitViewport != null)
			return;

		SplitViewport = AttachViewport(new Viewport(Clipboard));

		if (FocusedViewport.EditableElement is IEditableElement element)
			SplitViewport.SwitchTo(element);
	}

	private bool CommitViewportsAndSwallowError()
	{
		try
		{
			CommitViewports();
			return true;
		}
		catch
		{
			return false;
		}
	}

	private bool CommitViewportsOrPresentError()
	{
		try
		{
			CommitViewports();
			return true;
		}
		catch (Exception exception)
		{
			PresentError(exception);
			return false;
		}
	}

	private void CommitViewports()
	{
		PrimaryViewport?.CommitCurrentLine();
		SplitViewport?.CommitCurrentLine();
	}

	private void InstantWatchAtCurrentCursorLocation()
	{
		try
		{
			FocusedViewport.CommitCurrentLine();
		}
		catch (Exception exception)
		{
			PresentError(exception);
			return;
		}

		// If there is a selection, use the selection.
		// If there isn't a selection, walk backward until we find an
		// alphanumeric character or a close parenthesis. Then continue
		// until we find either the end of the alphanumeric sequence or
		// an open parenthesis, respectively, and Instant Watch on that.

		string subject = "";

		bool isValid = true;

		if (FocusedViewport.SelectionManager.HasSelection)
			subject = FocusedViewport.SelectionManager.GetSelectedText(multiline: false);
		else
		{
			FocusedViewport.EditCurrentLine();

			var buffer = FocusedViewport.CurrentLineBuffer;

			int endIndex = FocusedViewport.CursorX;

			if (endIndex >= buffer.Length)
				endIndex = buffer.Length - 1;

			while (endIndex >= 0)
			{
				char ch = buffer[endIndex];

				if ((ch == ')') || char.IsAsciiLetterOrDigit(ch))
					break;

				endIndex--;
			}

			if (endIndex < 0)
				isValid = false;
			else
			{
				int startIndex = endIndex;

				switch (buffer[endIndex])
				{
					case '(': // Find end parenthesis
						endIndex++;

						while (endIndex < buffer.Length)
						{
							if (buffer[endIndex] == ')')
								break;

							endIndex++;
						}

						if (endIndex >= buffer.Length)
							isValid = false;

						break;
					case ')': // Find start parenthesis
						startIndex = endIndex - 1;

						while (startIndex >= 0)
						{
							if (buffer[startIndex] == '(')
								break;

							startIndex--;
						}

						if (startIndex < 0)
							isValid = false;

						break;
					default: // Find identifier extent
						FindIdentifierExtent(buffer, ref startIndex, out endIndex);

						break;
				}

				if (isValid)
					subject = buffer.ToString(startIndex, endIndex - startIndex + 1);
			}
		}

		subject = subject.Trim();

		if (subject.Length == 0)
			isValid = false;

		if (isValid)
			ShowInstantWatch(_nextStatementRoutine?.Mapper, subject);
		else
			PresentError("Invalid expression for Instant Watch", 315, context: null, ErrorSource.Program, avoidContext: false);
	}

	void FindIdentifierExtent(StringBuilder buffer, ref int startIndex, out int endIndex)
	{
		// Find identifier
		if (startIndex >= buffer.Length)
			startIndex = buffer.Length - 1;

		if ((startIndex > 0)
		 && !char.IsAsciiLetterOrDigit(buffer[startIndex])
		 && char.IsAsciiLetterOrDigit(buffer[startIndex - 1]))
			startIndex--;

		endIndex = startIndex;

		// Grow left
		while (startIndex > 0)
		{
			char ch = buffer[startIndex - 1];

			if (!char.IsAsciiLetterOrDigit(ch))
				break;

			startIndex--;
		}

		// Grow right
		while (endIndex + 1 < buffer.Length)
		{
			char ch = buffer[endIndex + 1];

			if (!char.IsAsciiLetterOrDigit(ch)
				&& !"%&!#@$".Contains(ch))
				break;

			endIndex++;
		}
	}

	public void NavigateTo(CompilationElement element, int lineNumber, int column)
	{
		Viewport viewport;

		if (PrimaryViewport.EditableElement == element)
			viewport = PrimaryViewport;
		else if (SplitViewport?.EditableElement == element)
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

		viewport.SelectionManager.StartSelection(viewport.CursorX, viewport.CursorY);
	}
}
