using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class UnformattedPrintStatement : IExecutable
{
	public List<PrintArgument> Arguments = new List<PrintArgument>();

	[ThreadStatic]
	static string? s_spaces;

	public void Execute(ExecutionContext context, bool stepInto)
	{
		var emitter = new PrintEmitter(context);

		foreach (var argument in Arguments)
		{
			if (argument.Expression == null)
				throw new Exception("Internal error: PrintArgument has no Expression");

			switch (argument.ArgumentType)
			{
				case PrintArgumentType.Value:
				{
					emitter.Emit(argument.Expression.Evaluate(context));
					break;
				}

				case PrintArgumentType.Tab:
				case PrintArgumentType.Space:
				{
					int newCursorX = argument.Expression.Evaluate(context).CoerceToInt();

					newCursorX = (newCursorX - 1) % context.VisualLibrary.CharacterWidth;

					if (newCursorX < context.VisualLibrary.CursorX)
						context.VisualLibrary.NewLine();

					if (argument.ArgumentType == PrintArgumentType.Tab)
						context.VisualLibrary.MoveCursor(newCursorX, context.VisualLibrary.CursorY);
					else
					{
						if ((s_spaces == null) || (s_spaces.Length < newCursorX))
							s_spaces = new string(' ', newCursorX * 2);

						context.VisualLibrary.WriteText(s_spaces, 0, newCursorX);
					}

					break;
				}
			}

			switch (argument.CursorAction)
			{
				case PrintCursorAction.NextZone: emitter.NextZone(); break;
				case PrintCursorAction.NextLine: emitter.NextLine(); break;
			}
		}
	}
}
