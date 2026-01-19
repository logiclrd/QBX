using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ErrorStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? ErrorNumberExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (ErrorNumberExpression == null)
			throw new Exception("ErrorStatement with no ErrorNumberExpression");

		int errorNumber = ErrorNumberExpression.Evaluate(context, stackFrame).CoerceToInt();

		if ((errorNumber < short.MinValue) || (errorNumber > short.MaxValue))
			throw RuntimeException.Overflow(source);
		else if ((errorNumber < 0) || (errorNumber > 255))
			throw RuntimeException.IllegalFunctionCall(source);
		else
			throw RuntimeException.ForErrorNumber(errorNumber, source);
	}
}
