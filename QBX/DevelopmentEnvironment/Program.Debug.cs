using System.Collections.Generic;
using System.Linq;

using QBX.ExecutionEngine.Execution;

namespace QBX.DevelopmentEnvironment;

public partial class Program
{
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

	// TODO: come up with some fantastic scheme for reconstructing a desired
	// stack and selecting an arbitrary next instruction to implement
	// "Set Next Statement"
}
