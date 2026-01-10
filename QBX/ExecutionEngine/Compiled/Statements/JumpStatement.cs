using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class JumpStatement(string targetLabelName, CodeModel.Statements.Statement source) : Executable(source)
{
	public StatementPath? TargetPath;
	public string TargetLabelName => targetLabelName;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (TargetPath == null)
			throw new Exception("Internal error: Executing an unresolved JumpStatement");

		throw new GoTo(TargetPath);
	}
}
