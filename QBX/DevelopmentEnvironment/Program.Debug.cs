using System.Collections.Generic;
using System.Linq;

using QBX.DevelopmentEnvironment.Dialogs;
using QBX.ExecutionEngine;
using QBX.ExecutionEngine.Execution;
using QBX.LexicalAnalysis;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
	public Token? ErrorToken = null;

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

				FocusedViewport.CursorY = location.LineIndex;

				// TODO: per-statement location
				// => renderer will need to take charge of rendering the individual
				//    statements in the CodeLine
				// => renderer can then update the source location for each statement
				// TODO: move SourceLocation into the CodeModel, get rid of the hash table

				// We are invoked as part of a key handler in ProcessTextEditorKey.
				// The caller will ensure that the viewport scroll is adjusted as
				// necessary.
			}
		}
	}

	public void PresentError(RuntimeException error)
	{
		PresentError(error.Message, error.Context);
	}

	public void PresentError(string errorMessage, Token? context = null)
	{
		// TODO: navigate viewport to context

		ShowDialog(new ErrorDialog(Configuration, errorMessage));
	}
}
