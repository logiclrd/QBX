using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Expressions;

public class ArrayElementExpression(int variableIndex, DataType type) : Evaluable
{
	public override DataType Type => type;

	public List<Evaluable> SubscriptExpressions = new List<Evaluable>();

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (SubscriptExpressions.Count == 0)
			throw new Exception("ArrayElementExpression with no SubscriptExpressions");

		var arrayVariable = (ArrayVariable)stackFrame.Variables[variableIndex];

		var array = arrayVariable.Array;

		var subscripts = new Variable[SubscriptExpressions.Count];

		for (int i = 0; i < subscripts.Length; i++)
			subscripts[i] = SubscriptExpressions[i].Evaluate(context, stackFrame);

		return array.GetElement(subscripts);
	}
}
