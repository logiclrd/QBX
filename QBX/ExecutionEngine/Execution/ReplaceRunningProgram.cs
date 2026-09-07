using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution;

public class ReplaceRunningProgram : EndProgram
{
	StatementPath? _startingLineNumber;

	public StatementPath? StartingLineNumber => _startingLineNumber;

	public ReplaceRunningProgram()
	{
	}

	public ReplaceRunningProgram(StatementPath startingLineNumber)
	{
		_startingLineNumber = startingLineNumber;
	}
}
