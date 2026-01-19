using System;
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

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var arrayVariable = (ArrayVariable)stackFrame.Variables[variableIndex];

		if (SubscriptExpressions.Count == 0)
			return arrayVariable;

		var array = arrayVariable.Array;

		var subscripts = new Variable[SubscriptExpressions.Count];

		for (int i = 0; i < subscripts.Length; i++)
			subscripts[i] = SubscriptExpressions[i].Evaluate(context, stackFrame);

		return array.GetElement(subscripts, SubscriptExpressions);
	}
}
