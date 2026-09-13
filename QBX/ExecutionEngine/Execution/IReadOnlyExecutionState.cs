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
	string? ReplacementProgramFilePath { get; }
	CodeModel.Statements.Statement? ReplaceErrorContext { get; }
	StatementPath? StartingLineNumber { get; }
	bool IsTerminated { get; }
	event Func<StackFrame, bool>? CheckWatchpoints;
	bool CollectDirectSequenceCompletedFlag(); // Because of this, "read-only" applies specifically to the execution state itself.
}
