using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class LineInputStatement(string? promptString, bool echoNewline, CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? TargetExpression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (TargetExpression == null)
			throw new Exception("LineInputStatement with no TargetExpression");

		var target = (StringVariable)TargetExpression.Evaluate(context, stackFrame);

		if (promptString != null)
			context.VisualLibrary.WriteText(promptString);

		var inputLine = context.VisualLibrary.ReadLine(context.Machine.Keyboard, echoNewline);

		target.Value.Set(inputLine);
	}
}
