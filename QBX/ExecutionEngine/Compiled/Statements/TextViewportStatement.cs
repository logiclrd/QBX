using QBX.ExecutionEngine.Execution;
using QBX.Firmware;
using System;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class TextViewportStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? WindowStartExpression;
	public Evaluable? WindowEndExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (WindowStartExpression == null)
			throw new Exception("TextViewportStatement with no WindowStartExpression");
		if (WindowEndExpression == null)
			throw new Exception("TextViewportStatement with no WindowEndExpression");

		var windowStartValue = WindowStartExpression.Evaluate(context, stackFrame);
		var windowEndValue = WindowEndExpression.Evaluate(context, stackFrame);

		int windowStart;
		int windowEnd;

		try
		{
			windowStart = windowStartValue.CoerceToInt();
		}
		catch (CompilerException e) { throw e.AddContext(WindowStartExpression.SourceExpression?.Token); }

		try
		{
			windowEnd = windowEndValue.CoerceToInt();
		}
		catch (CompilerException e) { throw e.AddContext(WindowStartExpression.SourceExpression?.Token); }

		if (context.VisualLibrary is not TextLibrary textLibrary)
			throw RuntimeException.IllegalFunctionCall(Source);

		textLibrary.UpdateCharacterLineWindow(windowStart, windowEnd);
	}
}
