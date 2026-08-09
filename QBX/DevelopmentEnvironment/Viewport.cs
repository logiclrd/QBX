using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

using QBX.CodeModel;
using QBX.CodeModel.Statements;
using QBX.DevelopmentEnvironment.Help;
using QBX.ExecutionEngine;
using QBX.LexicalAnalysis;
using QBX.Parser;
using QBX.Utility;

namespace QBX.DevelopmentEnvironment;

public class Viewport
{
	const string DefaultHeading = "Untitled";

	public string Heading = DefaultHeading;
	public bool StaticHeading = false;
	public IEditableUnit? EditableUnit;
	public IEditableElement? EditableElement;
	public HelpDatabaseTopic? HelpTopic;
	public bool IsEditable = true;
	public bool IsDirectMode = false;
	public bool ShowMaximize = true;
	public int Height; // Ignored for the first, which fills available space.
	public bool HasHorizontalScrollBar = true;
	public int ScrollX, ScrollY;
	public int CursorX, CursorY;
	public bool CurrentLineEdited;
	public StringBuilder? CurrentLineBuffer;
	public SelectionManager SelectionManager;

	public bool RerenderCurrentLineAndCheckForActualChanges()
	{
		if (!CurrentLineEdited || (CurrentLineBuffer == null))
			return false;

		var writer = new StringWriter();

		RenderLine(CursorY, writer);

		var uneditedLine = writer.GetStringBuilder();

		if (CurrentLineBuffer.Length < uneditedLine.Length)
			return true;

		for (int i = 0; i < uneditedLine.Length; i++)
			if (CurrentLineBuffer[i] != uneditedLine[i])
				return true;

		for (int i = uneditedLine.Length; i < CurrentLineBuffer.Length; i++)
			if (CurrentLineBuffer[i] != ' ')
				return true;

		return false;
	}

	public event Func<string, IEditableElement?>? GetElementByName;

	public Viewport(Clipboard clipboard)
	{
		SelectionManager = new SelectionManager(this, clipboard);
	}

	public int CachedContentTopY;
	public int CachedContentHeight;

	public void UpdateHeading()
	{
		if (!StaticHeading)
		{
			if (HelpTopic != null)
				Heading = HelpTopic.TopicName;
			else if (EditableElement == null)
				Heading = DefaultHeading;
			else if (EditableElement.Name == null)
				Heading = EditableElement.Owner.Name;
			else
				Heading = EditableElement.Owner.Name + ":" + EditableElement.Name;
		}
	}

	public void SwitchTo(IEditableElement element)
	{
		EditableElement?.CachedCursorLine = CursorY;

		EditableUnit = element.Owner;
		EditableElement = element;

		UpdateHeading();

		CursorX = 0;
		CursorY = element.CachedCursorLine;

		if (CursorY >= element.Lines.Count)
			CursorY = element.Lines.Count - 1;
		if (CursorY < 0)
			CursorY = 0;

		ScrollX = 0;
		ScrollY = CursorY - Math.Max(1, CachedContentHeight) + 1;

		if (ScrollY < 0)
			ScrollY = 0;

		SelectionManager.StartSelection(CursorX, CursorY);
	}

	public int GetContentLineCount()
	{
		if (HelpTopic != null)
			return HelpTopic.Lines.Count;
		else if (EditableElement != null)
		{
			int count = EditableElement.Lines.Count;

			if ((CursorY >= count) && CurrentLineEdited)
				count++;

			return count;
		}
		else
			return 0;
	}

	public void RenderLine(int y, TextWriter writer)
		=> RenderLine(y, EditableLineState.Committed, writer);

	public void RenderLine(int y, EditableLineState lineState, TextWriter writer)
	{
		if (HelpTopic != null)
		{
			if ((y >= 0) && (y < HelpTopic.Lines.Count))
				HelpTopic.Lines[y].RenderPlainText(writer);
		}
		else if (EditableElement != null)
		{
			if ((lineState == EditableLineState.Uncommitted) && (y == CursorY) && (CurrentLineBuffer != null))
				writer.Write(CurrentLineBuffer);
			else if ((y >= 0) && (y < EditableElement.Lines.Count))
				EditableElement.Lines[y].Render(writer, includeCRLF: false);
		}
	}

	public bool TryGetLineAt(int y, [NotNullWhen(true)] out IEditableLine? line)
	{
		if (EditableElement != null)
		{
			if ((y >= 0) && (y < EditableElement.Lines.Count))
			{
				line = EditableElement.Lines[y];
				return true;
			}
		}

		line = null;
		return false;
	}

	public bool TryGetCodeLineAt(int y, [NotNullWhen(true)] out CodeLine? codeLine)
	{
		if (TryGetLineAt(y, out var line))
		{
			codeLine = line as CodeLine;
			return (codeLine != null);
		}

		codeLine = null;
		return false;
	}

	public void DeleteLine(int y)
	{
		if ((EditableElement != null) && IsEditable)
		{
			if (y < EditableElement.Lines.Count)
			{
				EditableElement.RemoveLineAt(y);
				EditableElement.Dirty();
			}
		}
	}

	public void InsertLine(int y, IEditableLine newLine)
	{
		if ((EditableElement != null) && IsEditable)
		{
			if (y < EditableElement.Lines.Count)
				EditableElement.InsertLine(y, newLine);
			else
				EditableElement.AddLine(newLine);

			EditableElement.Dirty();
		}
	}

	public void ReplaceCurrentLine(IEditableLine newLine)
	{
		if ((EditableElement != null) && IsEditable)
		{
			if (CursorY < EditableElement.Lines.Count)
				EditableElement.ReplaceLine(CursorY, newLine);
			else
				EditableElement.AddLine(newLine);

			EditableElement.Dirty();

			CurrentLineEdited = false;
			CurrentLineBuffer = null;
		}
	}

	[MemberNotNull(nameof(CurrentLineBuffer))]
	public StringBuilder EditCurrentLine()
	{
		if (CurrentLineBuffer == null)
		{
			var writer = new StringWriter();

			CurrentLineBuffer = writer.GetStringBuilder();

			if (EditableElement != null)
			{
				// Add empty line(s) if CursorY is past the end of the document.
				while (CursorY >= EditableElement.Lines.Count)
					EditableElement.AddLine(EditableElement.ConstructLine(CurrentLineBuffer));
			}

			RenderLine(CursorY, writer);
		}

		return CurrentLineBuffer;
	}

	public void CancelEdit()
	{
		CurrentLineBuffer = null;
		CurrentLineEdited = false;
	}

	public bool CommitCurrentLine(StringBuilder? buffer = null)
	{
		if (!IsEditable || (EditableElement == null))
			return false;

		if (!CurrentLineEdited || (CursorY < 0))
		{
			CurrentLineBuffer = null;
			return false;
		}

		buffer ??= CurrentLineBuffer;

		if (buffer == null)
			return false;

		if ((EditableUnit is not CompilationUnit unit)
		 || (EditableElement is not CompilationElement element))
		{
			ReplaceCurrentLine(EditableElement.ConstructLine(buffer));
			return false;
		}

		try
		{
			var lexer = new Lexer(new StringBuilderReader(buffer), element, startingLineNumber: CursorY);

			var parser = new BasicParser(unit.IdentifierRepository);

			var parsedCodeLine = parser.ParseCodeLines(lexer).SingleOrDefault();

			if (parsedCodeLine?.Statements.FirstOrDefault() is ProperSubroutineOpeningStatement startScopeStatement)
			{
				if (IsDirectMode)
					throw RuntimeException.IllegalInDirectMode(startScopeStatement);

				if ((EditableUnit is CompilationUnit compilationUnit)
				 && (EditableElement is CompilationElement compilationElement))
				{
					// The user has typed/edited a SUB or FUNCTION line. The question is, which one is it?
					// If the line being edited currently contains the old SubroutineOpeningStatement OR
					// the element _doesn't have a SubroutineOpeningStatement presently_, then this new
					// one applies to the current CompilationElement. Otherwise, treat this as a request
					// to create a new SUB/FUNCTION.

					CompilationElement? existingCodeElement = null;
					CodeLine? existingOpeningLine = null;
					ProperSubroutineOpeningStatement? existingOpeningStatement = null;

					bool isSubOrFunction =
						(compilationElement.Type == CompilationElementType.Sub) ||
						(compilationElement.Type == CompilationElementType.Function);

					if (isSubOrFunction)
					{
						existingCodeElement = (CompilationElement?)GetElementByName?.Invoke(startScopeStatement.Name);

						existingOpeningLine = compilationElement.Lines.FirstOrDefault(
							line => line.Statements.OfType<ProperSubroutineOpeningStatement>().Any());

						existingOpeningStatement = existingOpeningLine?.Statements.OfType<ProperSubroutineOpeningStatement>().FirstOrDefault();
					}

					TryGetLineAt(CursorY, out var currentLine);

					bool isForThisElement = isSubOrFunction && ((existingOpeningLine is null) || (existingOpeningLine == currentLine));

					if (isForThisElement)
					{
						if ((existingCodeElement is not null) && (existingCodeElement != compilationElement))
							throw RuntimeException.DuplicateDefinition(startScopeStatement.NameToken);

						ReplaceCurrentLine(parsedCodeLine);

						compilationElement.Name = startScopeStatement.Name;

						UpdateHeading();

						// If the user changes SUB<->FUNCTION in the opening
						// statement, update the end scope statement to match.
						if ((startScopeStatement.ScopeType != existingOpeningStatement?.ScopeType)
						 && (existingCodeElement != null))
						{
							switch (startScopeStatement.ScopeType)
							{
								case ScopeType.Sub: existingCodeElement.Type = CompilationElementType.Sub; break;
								case ScopeType.Function: existingCodeElement.Type = CompilationElementType.Function; break;
							}

							for (int i = existingCodeElement.Lines.Count - 1; i >= 0; i--)
							{
								if (existingCodeElement.Lines[i] is CodeLine line)
								{
									bool reparseLine = false;

									foreach (var statement in line.AllStatements)
									{
										if ((statement is EndScopeStatement endScopeStatement)
										 && (endScopeStatement.ScopeType != startScopeStatement.ScopeType))
										{
											reparseLine = true;
											break;
										}

										if ((statement is ExitScopeStatement exitScopeStatement)
										 && (exitScopeStatement.ScopeType != startScopeStatement.ScopeType))
										{
											if ((exitScopeStatement.ScopeType == ScopeType.Sub)
											 || (exitScopeStatement.ScopeType == ScopeType.Function))
											{
												reparseLine = true;
												break;
											}
										}
									}

									if (reparseLine)
									{
										var thisLineBuffer = new StringBuilder();

										var writer = new StringWriter(thisLineBuffer);

										line.Render(writer);

										lexer = new Lexer(new StringBuilderReader(thisLineBuffer), EditableElement as CompilationElement, startingLineNumber: i);

										try
										{
											var reparsedLine = parser.ParseCodeLines(lexer).SingleOrDefault();

											if (reparsedLine != null)
												existingCodeElement.ReplaceLine(i, reparsedLine);
										}
										catch
										{
											existingCodeElement.ReplaceLine(i, (CodeLine)existingCodeElement.ConstructLine(thisLineBuffer));
										}
									}
								}
							}
						}
					}
					else
					{
						if (existingCodeElement is not null)
							throw RuntimeException.DuplicateDefinition(startScopeStatement.NameToken);

						ReplaceCurrentLine(CodeLine.CreateEmpty());

						var endScopeLine = new CodeLine();

						endScopeLine.AppendStatement(
							new EndScopeStatement() { ScopeType = startScopeStatement.ScopeType });

						var newElement = new CompilationElement(compilationUnit);

						newElement.Name = startScopeStatement.Name;
						newElement.Type =
							startScopeStatement.Type switch
							{
								StatementType.Sub => CompilationElementType.Sub,
								StatementType.Function => CompilationElementType.Function,
								_ => CompilationElementType.Unknown,
							};

						newElement.AddLine(parsedCodeLine);
						newElement.AddLine(CodeLine.CreateEmpty());
						newElement.AddLine(endScopeLine);

						compilationUnit.AddElement(newElement);

						SwitchTo(newElement);

						CursorX = parsedCodeLine.ComputeLength(); // cursor at the end of the SUB/FUNCTION line
						CursorY = 0;

						ScrollX = 0;
						ScrollY = 0;

						SelectionManager.CancelSelection();
					}

					return true; // reload viewport parameters, if we were in the middle of handling a text editor key
				}
			}

			// Reparse the line to ensure that the tokens have accurate Column and Length values.
			if (parsedCodeLine != null)
			{
				try
				{
					buffer.Length = 0;

					var writer = new StringWriter(buffer);

					parsedCodeLine.Render(writer);

					lexer = new Lexer(new StringBuilderReader(buffer), EditableElement as CompilationElement, startingLineNumber: CursorY);

					parsedCodeLine = parser.ParseCodeLines(lexer).SingleOrDefault();
				}
				catch { }
			}

			ReplaceCurrentLine(parsedCodeLine ?? CodeLine.CreateEmpty());
			return false;

			// TODO: fancy code to rip out the statements for a modified line and replace them
			// => don't know if this is going to be possible, but if it is, it's probably
			// going to involve transplanting execution state and reconstructing call stacks
		}
		catch (Exception e)
		{
			if (e is SyntaxErrorException error)
			{
				// The error's context needs to link back to the CompilationElement for the IDE to highlight it.
				if (error.Token.OwnerElement == null)
					error.Token.OwnerElement = element;
			}

			ReplaceCurrentLine(CodeLine.CreateUnparsed(buffer.ToString()));

			if (!IsDirectMode)
				throw;

			return false;
		}
	}

	[ThreadStatic]
	static StringWriter? s_tempLineBufferWriter;

	public int GetLineIndentation(int y)
		=> GetLineIndentation(y, out _);

	public int GetLineIndentation(int y, out bool isEmpty)
	{
		StringBuilder tempLineBuffer;

		if ((y == CursorY) && (CurrentLineBuffer != null))
			tempLineBuffer = CurrentLineBuffer;
		else
		{
			s_tempLineBufferWriter ??= new StringWriter();

			tempLineBuffer = s_tempLineBufferWriter.GetStringBuilder();
			tempLineBuffer.Clear();

			RenderLine(y, s_tempLineBufferWriter);
		}

		isEmpty = true;

		for (int i=0; i < tempLineBuffer.Length; i++)
			if (tempLineBuffer[i] != ' ')
			{
				isEmpty = false;
				return i;
			}

		return 0;
	}

	public void ScrollCursorIntoView(Configuration configuration)
	{
		int contentHeight = Height;

		if (HasHorizontalScrollBar && configuration.ShowScrollBars)
			contentHeight--;

		if (CursorX >= ScrollX + 78)
			ScrollX = CursorX - 77;
		if (CursorX < ScrollX)
			ScrollX = CursorX;

		if (CursorY >= ScrollY + contentHeight)
			ScrollY = CursorY - contentHeight + 1;
		if (CursorY < ScrollY)
			ScrollY = CursorY;
	}

	public void ScrollCursorIntoView(int newCursorX, int newCursorY, int newScrollX, int newScrollY, ViewportPositioningPriority priority, int viewportWidth, Action<Action>? terminateToCommitEdit, bool ignoreErrors = false)
	{
		if (newScrollX < 0)
		{
			newCursorX -= newScrollX;
			newScrollX = 0;
		}

		if (newScrollY < 0)
		{
			newCursorY -= newScrollY;
			newScrollY = 0;
		}

		int contentLineCount = GetContentLineCount();

		int viewportHeight = CachedContentHeight;

		if (viewportHeight == 0)
			viewportHeight = Height - 2;

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

		if (priority == ViewportPositioningPriority.Scroll)
		{
			ClampCursorToViewportScroll();
			ClampCursorToDocument();
		}

		ClampViewportScrollToCursor();

		if (newScrollY < 0)
			newScrollY = 0;

		if (newCursorY == CursorY)
		{
			CursorX = newCursorX;
			CursorY = newCursorY;
			ScrollX = newScrollX;
			ScrollY = newScrollY;
		}
		else
		{
			terminateToCommitEdit ??= x => x();

			terminateToCommitEdit(
				() =>
				{
					try
					{
						CommitCurrentLine();
					}
					catch when (ignoreErrors)
					{
					}

					CursorX = newCursorX;
					CursorY = newCursorY;
					ScrollX = newScrollX;
					ScrollY = newScrollY;
				});
		}
	}
}
