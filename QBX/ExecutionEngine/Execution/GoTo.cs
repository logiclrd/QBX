using System;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution;

public class GoTo(StatementPath pathToStatement, StackFrame? targetFrame = null) : Exception
{
	public StatementPath StatementPath => pathToStatement;
	public StackFrame? TargetFrame => targetFrame;
}
