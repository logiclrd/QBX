using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class FormattedPrintStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public Evaluable? Format;
	public List<PrintArgument> Arguments = new List<PrintArgument>();
	public bool EmitNewLine = true;

	public CodeModel.Statements.PrintStatement? Statement;

	[ThreadStatic]
	static byte[]? s_spaces;

	protected virtual PrintEmitter CreateEmitter(ExecutionContext context, StackFrame stackFrame)
		=> new VisualPrintEmitter(context.VisualLibrary);

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (Format == null)
			throw new Exception("FormattedPrintStatement does not have a Format expression");

		var emitter = CreateEmitter(context, stackFrame);

		var formatVariable = (StringVariable)Format.Evaluate(context, stackFrame);
		string format = formatVariable.ValueString;

		int nextArgumentIndex = 0;

		var dummyVariable = formatVariable;

		while (nextArgumentIndex < Arguments.Count)
		{
			FormatDirective.SplitString(
				format, Statement,
				directive =>
				{
					Variable argumentValue = dummyVariable;

					if (directive.UsesArgument)
					{
						while (true)
						{
							if (nextArgumentIndex >= Arguments.Count)
								return false; // break out of loop

							var argument = Arguments[nextArgumentIndex++];

							if (argument.Expression == null)
								throw new Exception("PrintArgument with no Expression");

							if (argument.ArgumentType == PrintArgumentType.Value)
							{
								argumentValue = argument.Expression.Evaluate(context, stackFrame);

								break;
							}

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
						}
					}

					directive.Emit(argumentValue, Statement, emitter);

					return true; // continue loop
				});

			if (nextArgumentIndex == 0)
				throw RuntimeException.IllegalFunctionCall(Statement);
		}

		if (EmitNewLine)
			emitter.EmitNewLine();
	}
}
