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
	public IEditableUnit? EditableUnit;
	public IEditableElement? EditableElement;
	public HelpDatabaseTopic? HelpTopic;
	public bool IsEditable = true;
	public bool IsDirectMode = false;
	public bool ShowMaximize = true;
	public int Height; // Ignored for the first, which fills available space.
	public int ScrollX, ScrollY;
	public int CursorX, CursorY;
	public bool CurrentLineChanged;
	public StringBuilder? CurrentLineBuffer;
	public Clipboard Clipboard;

	public event Func<string, IEditableElement?>? GetElementByName;

	public Viewport()
	{
		Clipboard = new Clipboard(this);
	}

	public int CachedContentTopY;
	public int CachedContentHeight;

	public void UpdateHeading()
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
	}

	public int GetContentLineCount()
	{
		if (HelpTopic != null)
			return HelpTopic.Lines.Count;
		else if (EditableElement != null)
		{
			int count = EditableElement.Lines.Count;

			if ((CursorY >= count) && CurrentLineChanged)
				count++;

			return count;
		}
		else
			return 0;
	}

	public void RenderLine(int y, TextWriter writer)
	{
		if (HelpTopic != null)
		{
			if ((y >= 0) && (y < HelpTopic.Lines.Count))
				HelpTopic.Lines[y].RenderPlainText(writer);
		}
		else if (EditableElement != null)
		{
			if ((y >= 0) && (y < EditableElement.Lines.Count))
				EditableElement.Lines[y].Render(writer, includeCRLF: false);
		}
	}

	public void RenderLineAt(int y, TextWriter writer)
	{
		if (TryGetLineAt(y, out var line))
			line.Render(writer, includeCRLF: false);

		if (HelpTopic != null)
		{
			if ((y >= 0) && (y < HelpTopic.Lines.Count))
				HelpTopic.Lines[y].RenderPlainText(writer);
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

			CurrentLineChanged = false;
			CurrentLineBuffer = null;
		}
	}

	[MemberNotNull(nameof(CurrentLineBuffer))]
	public StringBuilder EditCurrentLine()
	{
		if (CurrentLineBuffer == null)
		{
			var writer = new StringWriter();

			RenderLine(CursorY, writer);

			CurrentLineBuffer = writer.GetStringBuilder();
		}

		return CurrentLineBuffer;
	}

	public void CancelEdit()
	{
		CurrentLineBuffer = null;
		CurrentLineChanged = false;
	}

	public bool CommitCurrentLine(StringBuilder? buffer = null)
	{
		if (!IsEditable || (EditableElement == null))
			return false;

		if (!CurrentLineChanged || (CursorY < 0))
		{
			CurrentLineBuffer = null;
			return false;
		}

		buffer ??= CurrentLineBuffer;

		if (buffer == null)
			return false;

		if (EditableUnit is not CompilationUnit unit)
		{
			EditableElement.ReplaceLine(CursorY, EditableElement.ConstructLine(buffer));
			return false;
		}

		try
		{
			var lexer = new Lexer(new StringBuilderReader(buffer), startingLineNumber: CursorY);

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

					IEditableElement? existingCodeElement = null;
					CodeLine? existingOpeningLine = null;

					bool isSubOrFunction =
						(compilationElement.Type == CompilationElementType.Sub) ||
						(compilationElement.Type == CompilationElementType.Function);

					if (isSubOrFunction)
					{
						existingCodeElement = GetElementByName?.Invoke(startScopeStatement.Name);

						existingOpeningLine = compilationElement.Lines.FirstOrDefault(
							line => line.Statements.OfType<SubroutineOpeningStatement>().Any());
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
					}

					return true; // reload viewport parameters, if we were in the middle of handling a text editor key
				}
			}

			ReplaceCurrentLine(parsedCodeLine ?? CodeLine.CreateEmpty());
			return false;

			// TODO: fancy code to rip out the statements for a modified line and replace them
			// => don't know if this is going to be possible, but if it is, it's probably
			// going to involve transplanting execution state and reconstructing call stacks
		}
		catch
		{
			ReplaceCurrentLine(CodeLine.CreateUnparsed(buffer.ToString()));

			if (!IsDirectMode)
				throw;

			return false;
		}
	}

	public void ScrollCursorIntoView(int newCursorX, int newCursorY, int newScrollX, int newScrollY, ViewportPositioningPriority priority, int viewportWidth, bool ignoreErrors = false)
	{
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

		if (newCursorY != CursorY)
		{
			try
			{
				CommitCurrentLine();
			}
			catch when (ignoreErrors)
			{
			}
		}

		CursorX = newCursorX;
		CursorY = newCursorY;
		ScrollX = newScrollX;
		ScrollY = newScrollY;
	}
}
