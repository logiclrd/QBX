using System.Collections.Generic;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Expressions;

public class ArrayElementExpression(int variableIndex, DataType type) : Evaluable
{
	DataType? _arrayType = null;

	public override DataType Type
	{
		get
		{
			if (SubscriptExpressions.Count != 0)
				return type;
			else
			{
				_arrayType ??= type.MakeArrayType();

				return _arrayType;
			}
		}
	}

	public List<Evaluable> SubscriptExpressions = new List<Evaluable>();

	public override void CollapseConstantSubexpressions()
	{
		for (int i = 0; i < SubscriptExpressions.Count; i++)
			CollapseConstantExpression(SubscriptExpressions, i);
	}

	public void EvaluateInParts(ExecutionContext context, StackFrame stackFrame, out Execution.Array array, out Variable[] subscripts)
	{
		var arrayVariable = (ArrayVariable)stackFrame.Variables[variableIndex];

		array = arrayVariable.Array;

		subscripts = new Variable[SubscriptExpressions.Count];

		for (int i = 0; i < subscripts.Length; i++)
			subscripts[i] = SubscriptExpressions[i].Evaluate(context, stackFrame);
	}

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (SubscriptExpressions.Count == 0)
			return stackFrame.Variables[variableIndex];

		EvaluateInParts(context, stackFrame, out var array, out var subscripts);

		return array.GetElement(subscripts, SubscriptExpressions);
	}
}
