using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class SoftKeyControlStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public bool Enable;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (context.RuntimeState.DisplaySoftKeyMacroLine != Enable)
		{
			context.RuntimeState.DisplaySoftKeyMacroLine = Enable;

			if (Enable)
			{
				context.RuntimeState.RenderSoftKeyMacroLine(context.VisualLibrary);

				if (context.VisualLibrary.CharacterLineWindowEnd + 1 >= context.VisualLibrary.CharacterHeight)
					context.VisualLibrary.CharacterLineWindowEnd = context.VisualLibrary.CharacterHeight - 2;
			}
			else
				context.VisualLibrary.CharacterLineWindowEnd = context.VisualLibrary.Height - 1;
		}
	}
}
