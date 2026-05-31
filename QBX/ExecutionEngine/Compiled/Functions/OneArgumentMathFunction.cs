using System;

using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Functions;

public abstract class OneArgumentMathFunction : ConstructibleFunction
{
	public Evaluable? Argument;

	public void SetArgument(Evaluable value)
	{
		if (!value.Type.IsNumeric)
			throw CompilerException.TypeMismatch(value.Source);

		Argument = value;
	}

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref Argument);
	}

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		if (Argument == null)
			throw new Exception(GetType().Name + " with no Argument");

		return EvaluateImplementation(Argument, context, stackFrame);
	}

	protected abstract Variable EvaluateImplementation(Evaluable argument, ExecutionContext context, StackFrame stackFrame);
}
