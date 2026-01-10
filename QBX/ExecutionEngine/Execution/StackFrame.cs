using System.Collections.Generic;

using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Execution;

public class StackFrame(Variable[] variables)
{
	public Variable[] Variables = variables;
	public CodeModel.Statements.Statement? CurrentStatement;

	Stack<StatementPath> _goSubStack = new Stack<StatementPath>();

	public void PushReturnPath(StatementPath returnPath)
	{
		_goSubStack.Push(returnPath);
	}

	public StatementPath PopReturnPath(CodeModel.Statements.Statement? context)
	{
		if (!_goSubStack.TryPop(out var path))
			throw RuntimeException.ReturnWithoutGoSub(context);

		return path;
	}
}
