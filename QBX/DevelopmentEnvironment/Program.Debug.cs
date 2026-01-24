using System.Collections.Generic;
using System.Linq;

using QBX.CodeModel;
using QBX.CodeModel.Statements;
using QBX.DevelopmentEnvironment.Dialogs;
using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using QBX.LexicalAnalysis;
using QBX.Parser;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	public Token? RuntimeErrorToken = null;
	public HashSet<CodeLine> Breakpoints = new HashSet<CodeLine>();

	private void ShowNextStatement(IEnumerable<StackFrame> stack)
	{
		var currentFrame = stack.FirstOrDefault();

		if (currentFrame != null)
		{
			var nextStatement = currentFrame.CurrentStatement;

			if ((nextStatement != null)
			 && _statementLocation.TryGetValue(nextStatement, out var location))
			{
				if (FocusedViewport!.CompilationElement != location.Element)
				{
					if (PrimaryViewport.CompilationElement == location.Element)
						FocusedViewport = PrimaryViewport;
					else if (SplitViewport?.CompilationElement == location.Element)
						FocusedViewport = SplitViewport;

					if (FocusedViewport.CompilationElement != location.Element)
						FocusedViewport.SwitchTo(location.Element);
				}

				FocusedViewport.CursorX = nextStatement.SourceColumn;
				FocusedViewport.CursorY = location.LineIndex;

				// TODO: move SourceLocation into the CodeModel, get rid of the hash table

				// We are invoked as part of a key handler in ProcessTextEditorKey.
				// The caller will ensure that the viewport scroll is adjusted as
				// necessary.
			}
		}
	}

	public void PresentError(SyntaxErrorException error)
	{
		PresentError(error.Message, error.Token);
	}

	public void PresentError(CompilerException error)
	{
		PresentError(error.Message, error.Context);
	}

	public void PresentError(RuntimeException error)
	{
		PresentError(error.Message, error.Context);

		RuntimeErrorToken = error.Context;
	}

	public void PresentError(string errorMessage, Token? context = null)
	{
		if ((context?.OwnerStatement is Statement statement)
		 && (statement.CodeLine is CodeLine line)
		 && (line.CompilationElement is CompilationElement element))
		{
			if (FocusedViewport!.CompilationElement != element)
				FocusedViewport.SwitchTo(element);

			FocusedViewport.ScrollCursorIntoView(
				newCursorX: context.Column, newCursorY: context.Line,
				FocusedViewport.ScrollX, FocusedViewport.ScrollY,
				ViewportPositioningPriority.Cursor,
				viewportWidth: TextLibrary.CharacterWidth - 2);
		}

		var dialog = ShowDialog(new ErrorDialog(Configuration, errorMessage));

		if ((TextLibrary.CursorY >= dialog.Y)
		 && (TextLibrary.CursorY <= dialog.Y + dialog.Height)) // include the shadow
			dialog.Y = TextLibrary.Height - dialog.Height - 1;
	}

	public void ToggleBreakpoint(CodeLine codeLine)
	{
		if (!Breakpoints.Contains(codeLine))
			SetBreakpoint(codeLine);
		else
			ClearBreakpoint(codeLine);
	}

	public void SetBreakpoint(CodeLine codeLine)
	{
		foreach (var statement in codeLine.Statements)
			statement.IsBreakpoint = true;

		Breakpoints.Add(codeLine);
	}

	public void ClearBreakpoint(CodeLine codeLine)
	{
		foreach (var statement in codeLine.Statements)
			statement.IsBreakpoint = false;

		Breakpoints.Remove(codeLine);
	}
}
