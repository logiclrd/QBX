using QBX.CodeModel.Statements;
using QBX.ExecutionEngine.Compiled;

namespace QBX.ExecutionEngine.Execution;

public class ReplaceRunningProgram : EndProgram
{
	string? _replacementFilePath;
	StatementPath? _startingLineNumber;
	Statement? _errorContext;

	public string? ReplacementFilePath => _replacementFilePath;
	public StatementPath? StartingLineNumber => _startingLineNumber;
	public Statement? ErrorContext => _errorContext;

	public ReplaceRunningProgram()
	{
	}

	public ReplaceRunningProgram(StatementPath startingLineNumber, Statement? errorContext)
	{
		_startingLineNumber = startingLineNumber;
		_errorContext = errorContext;
	}

	public ReplaceRunningProgram(string replacementFilePath, Statement? errorContext)
	{
		_replacementFilePath = replacementFilePath;
		_errorContext = errorContext;
	}
}
