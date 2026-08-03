using System;
using System.Collections.Generic;

namespace QBX.ExecutionEngine.Execution;

public interface IReadOnlyExecutionState
{
	IEnumerable<StackFrame> Stack { get; }
	bool IgnoreExplicitBreakFromNextStatement { get; }
	RuntimeException? CurrentError { get; }
	bool ChainExecution { get; }
	bool IsTerminated { get; }
	event Func<StackFrame, bool>? CheckWatchpoints;
}
