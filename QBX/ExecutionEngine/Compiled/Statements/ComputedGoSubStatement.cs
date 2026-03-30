namespace QBX.ExecutionEngine.Compiled.Statements;

public class ComputedGoSubStatement(CodeModel.Statements.Statement source) : ComputedGoToStatement(source)
{
	protected override void ExecuteBranch(ComputedBranchTarget target, Execution.StackFrame stackFrame)
	{
		var returnPath = GetPathToStatement(offset: 1);

		stackFrame.PushReturnPath(returnPath);

		base.ExecuteBranch(target, stackFrame);
	}
}
