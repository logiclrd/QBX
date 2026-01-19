using System;

namespace QBX.ExecutionEngine.Execution;

public class Resume : Exception
{
	public bool RetryStatement;
	public GoTo? GoTo;
}
