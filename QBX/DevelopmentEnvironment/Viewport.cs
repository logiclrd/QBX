using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

using QBX.CodeModel;
using QBX.LexicalAnalysis;
using QBX.Parser;
using QBX.Utility;

namespace QBX.DevelopmentEnvironment;

public class Viewport
{
	public string Heading = "Untitled";
	public CompilationUnit? CompilationUnit;
	public CompilationElement? CompilationElement;
	public HelpPage? HelpPage;
	public bool IsEditable = true;
	public bool IsFocused = false;
	public bool ShowMaximize = true;
	public int Height; // Ignored for the first, which fills available space.
	public int ScrollX, ScrollY;
	public int CursorX, CursorY;
	public bool CurrentLineChanged;
	public StringBuilder? CurrentLineBuffer;
	public Clipboard Clipboard;

	public BasicParser Parser;

	public Viewport(BasicParser parser)
	{
		Clipboard = new Clipboard(this);

		Parser = parser;
	}

	public int CachedContentTopY;
	public int CachedContentHeight;

	public void SwitchTo(CompilationElement element)
	{
		CompilationElement?.CachedCursorLine = CursorY;

		CompilationUnit = element.Owner;
		CompilationElement = element;

		if (element.Name == null)
			Heading = element.Owner.Name;
		else
			Heading = element.Owner.Name + ":" + element.Name;

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
		if (HelpPage != null)
			return HelpPage.Lines.Count;
		else if (CompilationElement != null)
		{
			int count = CompilationElement.Lines.Count;

			if ((CursorY >= count) && CurrentLineChanged)
				count++;

			return count;
		}
		else
			return 0;
	}

	public void RenderLine(int y, TextWriter writer)
	{
		if (HelpPage != null)
		{
			// TODO
		}
		else if (CompilationElement != null)
		{
			if ((y >= 0) && (y < CompilationElement.Lines.Count))
				CompilationElement.Lines[y].Render(writer, includeCRLF: false);
		}
	}

	public CodeLine GetCodeLineAt(int y)
	{
		if (TryGetCodeLineAt(y) is CodeLine line)
			return line;

		if (HelpPage != null)
		{
			if ((y >= 0) && (y < HelpPage.Lines.Count))
				return CodeLine.CreateUnparsed(HelpPage.Lines[y]);
		}

		return CodeLine.CreateEmpty();
	}

	public CodeLine? TryGetCodeLineAt(int y)
	{
		if (CompilationElement != null)
		{
			if ((y >= 0) && (y < CompilationElement.Lines.Count))
				return CompilationElement.Lines[y];
		}

		return null;
	}

	public void DeleteLine(int y)
	{
		if ((CompilationElement != null) && IsEditable)
		{
			if (y < CompilationElement.Lines.Count)
			{
				CompilationElement.RemoveLineAt(y);
				CompilationElement.Dirty();
			}
		}
	}

	public void InsertLine(int y, CodeLine newLine)
	{
		if ((CompilationElement != null) && IsEditable)
		{
			if (y < CompilationElement.Lines.Count)
				CompilationElement.InsertLine(y, newLine);
			else
				CompilationElement.AddLine(newLine);

			CompilationElement.Dirty();
		}
	}

	public void ReplaceCurrentLine(CodeLine newLine)
	{
		if ((CompilationElement != null) && IsEditable)
		{
			if (CursorY < CompilationElement.Lines.Count)
				CompilationElement.ReplaceLine(CursorY, newLine);
			else
				CompilationElement.AddLine(newLine);

			CompilationElement.Dirty();

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

	public void CommitCurrentLine(StringBuilder? buffer = null)
	{
		if (!CurrentLineChanged)
		{
			CurrentLineBuffer = null;
			return;
		}

		buffer ??= CurrentLineBuffer;

		if (buffer == null)
			return;

		try
		{
			var lexer = new Lexer(new StringBuilderReader(buffer), startingLineNumber: CursorY);

			var parsedCodeLine = Parser.ParseCodeLines(lexer).SingleOrDefault();

			ReplaceCurrentLine(parsedCodeLine ?? CodeLine.CreateEmpty());

			// TODO: the user alters the capitalization of an identifier, alter all others
			// in the program to match

			// TODO: fancy code to rip out the statements for a modified line and replace them
		}
		catch
		{
			ReplaceCurrentLine(CodeLine.CreateUnparsed(buffer.ToString()));
			throw;
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
