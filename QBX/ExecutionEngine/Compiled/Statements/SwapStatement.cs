using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class SwapStatement(CodeModel.Statements.SwapStatement source)
	: Executable(source)
{
	public Evaluable? Variable1Expression;
	public Evaluable? Variable2Expression;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (Variable1Expression == null)
			throw new Exception("SwapStatement with no Variable1Expression");
		if (Variable2Expression == null)
			throw new Exception("SwapStatement with no Variable2Expression");

		var variable1 = Variable1Expression.Evaluate(context, stackFrame);
		var variable2 = Variable2Expression.Evaluate(context, stackFrame);

		variable1.SwapValueWith(variable2);
	}
}
