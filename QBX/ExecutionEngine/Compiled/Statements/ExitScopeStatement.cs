using System;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class ExitScopeStatement(CodeModel.Statements.Statement? source) : Executable(source)
{
	public ExitScope? ScopeExitThrowable;

	public override void Execute(ExecutionContext context, StackFrame stackFrame)
	{
		throw ScopeExitThrowable ?? new Exception("Internal error: ExitScopeStatement with no throwable");
	}
}

public class ExitScope : Exception { }

public class ExitRoutine : ExitScope
{
	// If EXIT SUB/FUNCTION occurs inside an error handler,
	// it's possible that the handler is further up the
	// call stack than where the error occurred. In this
	// case, we need to unwind potentially multiple
	// intermediary subroutine calls to get to the one to
	// which the EXIT SUB/FUNCTION applies. If StackFrame
	// is set, then ExecutionContext.Call statements that
	// catch this ExitRoutine should check if it's for
	// them and rethrow it if not.
	public StackFrame? StackFrame;
}

public class ExitDo : ExitScope { }
public class ExitFor : ExitScope { }
