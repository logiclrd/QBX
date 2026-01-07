using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Execution;

public class StackFrame(Module module, ISequence? sequence, Variable[] variables)
{
	public Variable[] Variables = variables;
	public IExecutable? NextStatement;
	public int NextStatementIndex;
	public Module CurrentModule = module;
	public ISequence? CurrentSequence = sequence;

	public StackFrame Clone()
	{
		return
			new StackFrame(CurrentModule, CurrentSequence, Variables)
			{
				NextStatement = this.NextStatement,
				NextStatementIndex = this.NextStatementIndex,
			};
	}
}
