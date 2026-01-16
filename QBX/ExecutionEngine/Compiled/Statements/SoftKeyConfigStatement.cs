using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class SoftKeyConfigStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? KeyExpression;
	public Evaluable? MacroExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (KeyExpression == null)
			throw new Exception("SoftKeyConfigStatement with no KeyExpression");
		if (MacroExpression == null)
			throw new Exception("SoftKeyConfigStatement with no MacroExpression");

		var keyValue = KeyExpression.Evaluate(context, stackFrame);
		var macroValue = (StringVariable)MacroExpression.Evaluate(context, stackFrame);

		int key = keyValue.CoerceToInt();
		var macro = macroValue.Value;

		if (key == 30)
			key = 11;
		if (key == 31)
			key = 12;

		context.RuntimeState.SoftKeyMacros[key - 1] = macro;
	}
}
