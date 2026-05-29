using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Expressions;

public class DetachExpression : Evaluable
{
	public Evaluable? Expression;

	public override DataType Type => Expression?.Type ?? throw new Exception("Uninitialized DetachExpression");

	public DetachExpression()
	{
	}

	public DetachExpression(Evaluable? expression)
	{
		Expression = expression;
	}

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (Expression == null)
			throw new Exception("Uninitialized DetachExpression");

		return Expression.Evaluate(context, stackFrame).Detach();
	}
}
