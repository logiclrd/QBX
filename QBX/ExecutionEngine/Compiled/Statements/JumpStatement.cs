using System;

using QBX.ExecutionEngine.Execution;
using QBX.Parser;

namespace QBX.ExecutionEngine.Compiled.Statements;

public abstract class JumpStatement(Identifier targetLabelName, CodeModel.Statements.Statement source) : Executable(source)
{
	public StatementPath? TargetPath;
	public Identifier TargetLabelName => targetLabelName;

	public virtual bool TargetIsInMainModule => false;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		if (TargetPath == null)
			throw new Exception("Internal error: Executing an unresolved JumpStatement");

		throw new GoTo(TargetPath);
	}
}
