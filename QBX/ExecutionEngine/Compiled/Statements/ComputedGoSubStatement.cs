using System;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ComputedGoSubStatement : ComputedGoToStatement
{
	public ComputedGoSubStatement(CodeModel.Statements.ComputedBranchStatement source)
		: base(source)
	{
		if (source.BranchType != CodeModel.Statements.ComputedBranchType.GoSub)
			throw new Exception("A ComputedGoSubStatement cannot represent a CodeModel ComputedBranchStatement of type GoTo");
	}

	protected override void ExecuteBranch(ComputedBranchTarget target, Execution.StackFrame stackFrame)
	{
		var returnPath = GetPathToStatement(offset: 1);

		stackFrame.PushReturnPath(returnPath);

		base.ExecuteBranch(target, stackFrame);
	}
}
