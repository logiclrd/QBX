using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled;

public abstract class Evaluable
{
	public CodeModel.Expressions.Expression? Source;

	public abstract DataType Type { get; }

	public virtual void CollapseConstantSubexpressions()
	{
	}

	public static void CollapseConstantExpression([NotNullIfNotNull("expression")] ref Evaluable? expression)
	{
		if (expression != null)
		{
			if (expression.IsConstant)
				expression = expression.EvaluateConstant();
			else
				expression.CollapseConstantSubexpressions();
		}
	}

	public static void CollapseConstantExpression(IList<Evaluable> expressions, int index)
	{
		var expression = expressions[index];

		if (expression.IsConstant)
			expressions[index] = expression.EvaluateConstant();
		else
			expression.CollapseConstantSubexpressions();
	}

	public abstract Variable Evaluate(ExecutionContext context, StackFrame stackFrame);

	public int EvaluateAndCoerceToInt(ExecutionContext context, StackFrame stackFrame)
		=> Evaluate(context, stackFrame).CoerceToInt(context: this);

	public virtual bool IsConstant
		=> false;
	public virtual LiteralValue EvaluateConstant()
		=> throw CompilerException.ValueIsNotConstant(Source);
}
