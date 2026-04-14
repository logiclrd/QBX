using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class TextViewportStatement(CodeModel.Statements.TextViewportStatement source) : Executable(source)
{
	public Evaluable? WindowStartExpression;
	public Evaluable? WindowEndExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if ((WindowStartExpression == null) && (WindowEndExpression == null))
			context.VisualLibrary.ResetCharacterLineWindow();
		else
		{
			if (WindowStartExpression == null)
				throw new Exception("TextViewportStatement with no WindowStartExpression");
			if (WindowEndExpression == null)
				throw new Exception("TextViewportStatement with no WindowEndExpression");

			int windowStart = WindowStartExpression.EvaluateAndCoerceToInt(context, stackFrame);
			int windowEnd = WindowEndExpression.EvaluateAndCoerceToInt(context, stackFrame);

			if ((windowStart > windowEnd)
			 || (windowStart < 1)
			 || (windowEnd > context.VisualLibrary.CharacterHeight))
				throw RuntimeException.IllegalFunctionCall(Source);

			context.VisualLibrary.UpdateCharacterLineWindow(windowStart - 1, windowEnd - 1);
		}
	}
}
