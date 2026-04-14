using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class DrawStatement(CodeModel.Statements.DrawStatement source)
	: Executable(source)
{
	public Evaluable? CommandStringExpression;
	public Evaluable? DurationExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (CommandStringExpression == null)
			throw new Exception("DrawStatement with no CommandStringExpression");

		var commandStringValue = (StringVariable)CommandStringExpression.Evaluate(context, stackFrame);

		context.DrawProcessor.DrawCommandString(commandStringValue.ValueSpan, context, source);
	}
}
