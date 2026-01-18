using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Firmware;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class FormattedPrintStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public Evaluable? Format;
	public List<PrintArgument> Arguments = new List<PrintArgument>();
	public bool EmitNewLine = true;

	public CodeModel.Statements.PrintStatement? Statement;

	[ThreadStatic]
	static string? s_spaces;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (Format == null)
			throw new Exception("FormattedPrintStatement does not have a Format expression");

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

							int newCursorX = argument.Expression.Evaluate(context, stackFrame).CoerceToInt();

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
						}
					}

					directive.Emit(argumentValue, context, Statement);

					return true; // continue loop
				});

			if (nextArgumentIndex == 0)
				throw RuntimeException.IllegalFunctionCall(Statement);
		}

		if (EmitNewLine)
			context.VisualLibrary.NewLine();
	}
}
