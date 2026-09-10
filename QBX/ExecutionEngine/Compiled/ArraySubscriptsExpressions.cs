using System;
using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine.Compiled;

public class ArraySubscriptsExpressions
{
	public List<ArraySubscriptExpressions> Subscripts { get; } = new List<ArraySubscriptExpressions>();

	public ArraySubscripts Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var subscripts = new ArraySubscripts();

		foreach (var subscriptExpressions in Subscripts)
			subscripts.Add(subscriptExpressions.Evaluate(context, stackFrame));

		return subscripts;
	}

	internal void Add(Evaluable lowerBound, Evaluable upperBound)
	{
		Subscripts.Add(
			new ArraySubscriptExpressions()
			{
				LowerBound = lowerBound,
				UpperBound = upperBound,
			});
	}
}
