using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public interface IExecutable
{
	CodeModel.Statements.Statement Source { get; }

	void Execute(Execution.ExecutionContext context);
	void Step(Execution.ExecutionContext context, bool stepInto);
}
