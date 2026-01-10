using System;

using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution;

public class GoTo(StatementPath pathToStatement) : Exception
{
	public StatementPath StatementPath => pathToStatement;
}
