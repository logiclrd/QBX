using System;
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
		if (CompilationElement != null)
		{
			if ((y >= 0) && (y < CompilationElement.Lines.Count))
				return CompilationElement.Lines[y];
		}
		else if (HelpPage != null)
		{
			if ((y >= 0) && (y < HelpPage.Lines.Count))
				return CodeLine.CreateUnparsed(HelpPage.Lines[y]);
		}

		return CodeLine.CreateEmpty();
	}

	public void DeleteLine(int y)
	{
		if ((CompilationElement != null) && IsEditable)
		{
			if (y < CompilationElement.Lines.Count)
			{
				CompilationElement.Lines.RemoveAt(y);
				CompilationElement.Dirty();
			}
		}
	}

	public void InsertLine(int y, CodeLine newLine)
	{
		if ((CompilationElement != null) && IsEditable)
		{
			if (y < CompilationElement.Lines.Count)
				CompilationElement.Lines.Insert(y, newLine);
			else
				CompilationElement.Lines.Add(newLine);

			CompilationElement.Dirty();
		}
	}

	public void ReplaceCurrentLine(CodeLine newLine)
	{
		if ((CompilationElement != null) && IsEditable)
		{
			if (CursorY < CompilationElement.Lines.Count)
				CompilationElement.Lines[CursorY] = newLine;
			else
				CompilationElement.Lines.Add(newLine);

			CompilationElement.Dirty();

			CurrentLineChanged = false;
			CurrentLineBuffer = null;
		}
	}

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
			var lexer = new Lexer(new StringBuilderReader(buffer));

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
}
