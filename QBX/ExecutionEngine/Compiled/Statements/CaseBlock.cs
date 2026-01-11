using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

using System.Collections.Generic;
using System.Linq;

namespace QBX.ExecutionEngine.Compiled.Statements;

public class CaseBlock : Sequence
{
	public List<CaseExpression> Expressions = new List<CaseExpression>();
	public bool MatchAll;

	public bool IsMatch(Variable testValue, ExecutionContext context, StackFrame stackFrame)
	{
		if (MatchAll)
			return true;
		else
			return Expressions.Any(expression => expression.IsMatch(testValue, context, stackFrame));
	}
}
