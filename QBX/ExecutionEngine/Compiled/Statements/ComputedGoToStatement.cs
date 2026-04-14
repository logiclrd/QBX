using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ComputedGoToStatement : ComputedBranchStatement
{
	public ComputedGoToStatement(CodeModel.Statements.ComputedBranchStatement source)
		: base(source)
	{
		if ((source.BranchType != CodeModel.Statements.ComputedBranchType.GoTo)
		 && (this is not ComputedGoSubStatement))
			throw new Exception("A ComputedGoToStatement cannot represent a CodeModel ComputedBranchStatement of type GoSub");
	}

	protected override void ExecuteBranch(ComputedBranchTarget target, StackFrame stackFrame)
	{
		if (target.TargetPath == null)
			throw new Exception("Internal error: Executing an unresolved JumpStatement");

		throw new GoTo(target.TargetPath);
	}
}
