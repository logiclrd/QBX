using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class LineInputStatement(CodeModel.Statements.LineInputStatement source) : Executable(source)
{
	public Evaluable? TargetExpression;

	protected virtual void EmitPrompt(ExecutionContext context) { }
	protected abstract StringValue ReadLine(ExecutionContext context);

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (TargetExpression == null)
			throw new Exception("LineInputStatement with no TargetExpression");

		var target = (StringVariable)TargetExpression.Evaluate(context, stackFrame);

		EmitPrompt(context);

		var inputLine = ReadLine(context);

		target.Value.Set(inputLine);
	}
}
