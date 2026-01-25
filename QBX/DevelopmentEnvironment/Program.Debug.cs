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
	public Statement? NextStatement => _nextStatement;
	public Token? RuntimeErrorToken => _runtimeErrorToken;
	public IReadOnlySet<CodeLine> Breakpoints => _breakpoints;

	Statement? _nextStatement = null;
	Token? _runtimeErrorToken = null;
	HashSet<CodeLine> _breakpoints = new HashSet<CodeLine>();

	private void ShowNextStatement(IEnumerable<StackFrame> stack)
	{
		var currentFrame = stack.FirstOrDefault();

		if (currentFrame != null)
		{
			_nextStatement = currentFrame.CurrentStatement;

			if (_nextStatement != null)
			{
				if ((_nextStatement.CodeLine is CodeLine line)
				 && (line.CompilationElement is CompilationElement element))
				{
					if (FocusedViewport!.CompilationElement != element)
					{
						if (PrimaryViewport.CompilationElement == element)
							FocusedViewport = PrimaryViewport;
						else if (SplitViewport?.CompilationElement == element)
							FocusedViewport = SplitViewport;

						if (FocusedViewport.CompilationElement != element)
							FocusedViewport.SwitchTo(element);
					}

					FocusedViewport.CursorX = _nextStatement.SourceColumn;
					FocusedViewport.CursorY = line.LineIndex;
				}

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

		_runtimeErrorToken = error.Context;
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
		if (!_breakpoints.Contains(codeLine))
			SetBreakpoint(codeLine);
		else
			ClearBreakpoint(codeLine);
	}

	public void SetBreakpoint(CodeLine codeLine)
	{
		foreach (var statement in codeLine.Statements)
			statement.IsBreakpoint = true;

		_breakpoints.Add(codeLine);
	}

	public void ClearBreakpoint(CodeLine codeLine)
	{
		foreach (var statement in codeLine.Statements)
			statement.IsBreakpoint = false;

		_breakpoints.Remove(codeLine);
	}
}
