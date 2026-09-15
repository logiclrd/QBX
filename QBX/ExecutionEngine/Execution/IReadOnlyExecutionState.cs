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
	ReplaceRunningProgram? ReplaceRunningProgram { get; }
	StatementPath? PeekStartingLineNumber();
	bool IsTerminated { get; }
	event Func<StackFrame, bool>? CheckWatchpoints;
	bool CollectDirectSequenceCompletedFlag(); // Because of this, "read-only" applies specifically to the execution state itself.
}
