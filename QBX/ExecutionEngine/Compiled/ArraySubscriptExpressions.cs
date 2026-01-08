using QBX.ExecutionEngine.Execution;
using QBX.Numbers;
using System;

namespace QBX.ExecutionEngine.Compiled;

public class ArraySubscriptExpressions
{
	public IEvaluable? LowerBound;
	public IEvaluable? UpperBound;

	public ArraySubscript Evaluate(ExecutionContext context)
	{
		if (LowerBound == null)
			throw new Exception("ArraySubscriptExpressions does not have LowerBound");
		if (UpperBound == null)
			throw new Exception("ArraySubscriptExpressions does not have UpperBound");

		var ret = new ArraySubscript();

		ret.LowerBound = NumberConverter.ToInteger(LowerBound.Evaluate(context));
		ret.UpperBound = NumberConverter.ToInteger(UpperBound.Evaluate(context));

		return ret;
	}
}
