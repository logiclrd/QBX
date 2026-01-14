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

public class ExitRoutine : ExitScope { }
public class ExitDo : ExitScope { }
public class ExitFor : ExitScope { }
