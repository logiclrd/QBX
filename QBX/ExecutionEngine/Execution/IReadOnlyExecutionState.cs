using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution;

public interface IReadOnlyExecutionState
{
	IEnumerable<StackFrame> Stack { get; }
	bool IgnoreExplicitBreakFromNextStatement { get; }
	RuntimeException? CurrentError { get; }
	bool ChainExecution { get; }
	bool ReplaceRunningProgram { get; }
	StatementPath? StartingLineNumber { get; }
	bool IsTerminated { get; }
	event Func<StackFrame, bool>? CheckWatchpoints;
}
