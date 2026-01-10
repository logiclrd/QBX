using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public abstract class Statement : IExecutable
{
	public CodeModel.Statements.Statement? Source { get; set; }

	public Statement(CodeModel.Statements.Statement? source)
	{
		Source = source;
	}

	public virtual bool CanBreak { get; set; } = true;

	public abstract void Execute(ExecutionContext context, StackFrame stackFrame);
}
