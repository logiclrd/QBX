using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class UnformattedPrintStatement(CodeModel.Statements.PrintStatement source) : Executable(source)
{
	public List<PrintArgument> Arguments = new List<PrintArgument>();

	protected virtual PrintEmitter CreateEmitter(ExecutionContext context, StackFrame stackFrame)
		=> new VisualPrintEmitter(context.VisualLibrary);

	[ThreadStatic]
	static byte[]? s_spaces;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		var emitter = CreateEmitter(context, stackFrame);

		if (Arguments.Count == 0)
		{
			emitter.EmitNewLine();
			return;
		}

		foreach (var argument in Arguments)
		{
			switch (argument.ArgumentType)
			{
				case PrintArgumentType.Value:
				{
					if (argument.Expression != null)
						emitter.Emit(argument.Expression.Evaluate(context, stackFrame));
					break;
				}

				case PrintArgumentType.Space:
				{
					if (argument.Expression == null)
						throw new Exception("Internal error: PrintArgument has no Expression");

					int count = argument.Expression.EvaluateAndCoerceToInt(context, stackFrame);

					if ((s_spaces == null) || (s_spaces.Length < count))
					{
						s_spaces = new byte[count * 2];
						s_spaces.AsSpan().Fill((byte)' ');
					}

					emitter.Emit(s_spaces.AsSpan().Slice(0, count));

					break;
				}
				case PrintArgumentType.Tab:
				{
					if (argument.Expression == null)
						throw new Exception("Internal error: PrintArgument has no Expression");

					int newCursorX = argument.Expression.EvaluateAndCoerceToInt(context, stackFrame);

					newCursorX = (newCursorX - 1) % emitter.Width;

					if (newCursorX < emitter.CursorX)
						emitter.EmitNewLine();

					emitter.CursorX = newCursorX;

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
