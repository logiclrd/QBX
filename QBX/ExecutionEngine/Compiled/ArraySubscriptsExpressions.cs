using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using System;
using System.Collections.Generic;

namespace QBX.ExecutionEngine.Compiled;

public class ArraySubscriptsExpressions
{
	public List<ArraySubscriptExpressions> Subscripts { get; } = new List<ArraySubscriptExpressions>();

	public ArraySubscripts Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var subscripts = new ArraySubscripts();

		foreach (var subscriptExpressions in Subscripts)
			subscripts.Subscripts.Add(subscriptExpressions.Evaluate(context, stackFrame));

		return subscripts;
	}

	internal void Add(IEvaluable lowerBound, IEvaluable upperBound)
	{
		Subscripts.Add(
			new ArraySubscriptExpressions()
			{
				LowerBound = lowerBound,
				UpperBound = upperBound,
			});
	}
}
