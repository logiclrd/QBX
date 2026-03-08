using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class UnformattedPrintStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public List<PrintArgument> Arguments = new List<PrintArgument>();

	protected virtual PrintEmitter CreateEmitter(ExecutionContext context, StackFrame stackFrame)
		=> new VisualPrintEmitter(context.VisualLibrary);

	[ThreadStatic]
	static byte[]? s_spaces;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		var emitter = CreateEmitter(context, stackFrame);

		foreach (var argument in Arguments)
		{
			if (argument.Expression == null)
				throw new Exception("Internal error: PrintArgument has no Expression");

			switch (argument.ArgumentType)
			{
				case PrintArgumentType.Value:
				{
					emitter.Emit(argument.Expression.Evaluate(context, stackFrame));
					break;
				}

				case PrintArgumentType.Tab:
				case PrintArgumentType.Space:
				{
					int newCursorX = argument.Expression.EvaluateAndCoerceToInt(context, stackFrame);

					newCursorX = (newCursorX - 1) % emitter.Width;

					if (newCursorX < emitter.CursorX)
						emitter.EmitNewLine();

					if (argument.ArgumentType == PrintArgumentType.Tab)
						emitter.CursorX = newCursorX;
					else
					{
						if ((s_spaces == null) || (s_spaces.Length < newCursorX))
						{
							s_spaces = new byte[newCursorX * 2];
							s_spaces.AsSpan().Fill((byte)' ');
						}

						emitter.Emit(s_spaces.AsSpan().Slice(0, newCursorX));
					}

					break;
				}
			}

			switch (argument.CursorAction)
			{
				case PrintCursorAction.NextZone: emitter.NextZone(); break;
				case PrintCursorAction.NextLine: emitter.EmitNewLine(); break;
			}
		}
	}
}
