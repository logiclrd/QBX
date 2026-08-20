using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ReturnStatement(CodeModel.Statements.ReturnStatement source) : Executable(source)
{
	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		var returnPath = stackFrame.PopReturnPath(Source);

		throw new GoTo(returnPath);
	}
}
