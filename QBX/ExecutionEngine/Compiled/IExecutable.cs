using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public interface IExecutable
{
	void Execute(Execution.ExecutionContext context, bool stepInto);
}
