using System;

using QBX.ExecutionEngine.Execution;
using QBX.Numbers;

namespace QBX.ExecutionEngine.Compiled;

public class ArraySubscriptExpressions
{
	public Evaluable? LowerBound;
	public Evaluable? UpperBound;

	public ArraySubscript Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (LowerBound == null)
			throw new Exception("ArraySubscriptExpressions does not have LowerBound");
		if (UpperBound == null)
			throw new Exception("ArraySubscriptExpressions does not have UpperBound");

		var ret = new ArraySubscript();

		ret.LowerBound = NumberConverter.ToInteger(LowerBound.Evaluate(context, stackFrame));
		ret.UpperBound = NumberConverter.ToInteger(UpperBound.Evaluate(context, stackFrame));

		return ret;
	}

	public ArraySubscript EvaluateConstant()
	{
		if (LowerBound == null)
			throw new Exception("ArraySubscriptExpressions does not have LowerBound");
		if (UpperBound == null)
			throw new Exception("ArraySubscriptExpressions does not have UpperBound");

		var ret = new ArraySubscript();

		ret.LowerBound = NumberConverter.ToInteger(LowerBound.EvaluateConstant());
		ret.UpperBound = NumberConverter.ToInteger(UpperBound.EvaluateConstant());

		return ret;
	}
}
