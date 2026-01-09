using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public interface IExecutable
{
	CodeModel.Statements.Statement? Source { get; }

	bool CanBreak { get; }

	void Execute(ExecutionContext context, StackFrame stackFrame);
}
