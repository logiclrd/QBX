using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Events;
using QBX.ExecutionEngine.Execution.Variables;
using QBX.Hardware;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class KeyConfigStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? KeyExpression;
	public Evaluable? ArgumentExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (KeyExpression == null)
			throw new Exception("KeyConfigStatement with no KeyExpression");
		if (ArgumentExpression == null)
			throw new Exception("KeyConfigStatement with no ArgumentExpression");

		var keyValue = KeyExpression.Evaluate(context, stackFrame);
		var argumentValue = (StringVariable)ArgumentExpression.Evaluate(context, stackFrame);

		int key = keyValue.CoerceToInt(context: KeyExpression);
		var argument = argumentValue.CloneValue();

		// Soft Key configuration when KeyExpression evaluates to 1-10, 30 or 31
		// Key event configuration when KeyExpression evaluates to 15-25

		if ((key >= 15) && (key <= 25))
		{
			if (argument.Length != 2)
				throw RuntimeException.IllegalFunctionCall(ArgumentExpression.Source);

			var definition = new KeyEventKeyDefinition(
				(KeyEventKeyModifiers)argument[0],
				(ScanCode)argument[1]);

			var change = EventConfigurationChange.ConfigureKey(key, definition);

			context.EventHub.DispatchConfigurationChange(change);
		}
		else
		{
			if ((key < 1) && (key > 10))
			{
				if (key == 30)
					key = 11;
				else if (key == 31)
					key = 12;
				else
					throw RuntimeException.IllegalFunctionCall(KeyExpression.Source);
			}

			context.RuntimeState.SoftKeyMacros[key - 1] = argument;
		}
	}
}
