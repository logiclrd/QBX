using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ReturnStatement(CodeModel.Statements.Statement source) : Executable(source)
{
	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		var returnPath = stackFrame.PopReturnPath(Source);

		throw new GoTo(returnPath);
	}
}
