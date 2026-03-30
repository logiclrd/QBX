using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class ComputedBranchStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public Evaluable? Expression;
	public List<ComputedBranchTarget> Targets = new List<ComputedBranchTarget>();

	protected abstract void ExecuteBranch(ComputedBranchTarget target, StackFrame stackFrame);

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (Expression == null)
			throw new Exception("Internal error: ComputedBranchStatement with no Expression");

		int targetIndex = Expression.EvaluateAndCoerceToInt(context, stackFrame);

		if ((targetIndex < 0) || (targetIndex > 255))
			throw RuntimeException.IllegalFunctionCall(Source);

		if ((targetIndex >= 1) && (targetIndex <= Targets.Count))
		{
			var target = Targets[targetIndex - 1];

			ExecuteBranch(target, stackFrame);
		}
	}
}
