using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class PlayStatement(CodeModel.Statements.Statement source)
	: Executable(source)
{
	public Evaluable? CommandStringExpression;
	public Evaluable? DurationExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (CommandStringExpression == null)
			throw new Exception("PlayStatement with no CommandStringExpression");

		var commandStringValue = (StringVariable)CommandStringExpression.Evaluate(context, stackFrame);

		context.PlayProcessor.PlayCommandString(commandStringValue.ValueSpan, source);
	}
}
