using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class EraseStatement(CodeModel.Statements.EraseStatement source) : Executable(source)
{
	public List<Evaluable> ArrayExpressions = new List<Evaluable>();

	protected override void ExecuteImplementation(ExecutionContext context, StackFrame stackFrame)
	{
		foreach (var arrayExpression in ArrayExpressions)
		{
			var arrayVariable = (ArrayVariable)arrayExpression.Evaluate(context, stackFrame);

			arrayVariable.Reset();
		}
	}
}
