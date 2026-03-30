using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ComputedGoToStatement(CodeModel.Statements.Statement source) : ComputedBranchStatement(source)
{
	protected override void ExecuteBranch(ComputedBranchTarget target, StackFrame stackFrame)
	{
		if (target.TargetPath == null)
			throw new Exception("Internal error: Executing an unresolved JumpStatement");

		throw new GoTo(target.TargetPath);
	}
}
