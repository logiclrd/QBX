using QBX.ExecutionEngine.Compiled.Expressions;
using QBX.ExecutionEngine.Execution;
using QBX.ExecutionEngine.Execution.Variables;

namespace QBX.ExecutionEngine.Compiled.Operations;

public class IsZero(Evaluable right) : Evaluable
{
	public override DataType Type => DataType.Integer;

	public override bool IsConstant => right.IsConstant;

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref right);
	}

	public override Variable Evaluate(ExecutionContext context, StackFrame stackFrame)
	{
		var value = right.Evaluate(context, stackFrame);

		return new IntegerVariable(value.IsZero);
	}

	public override LiteralValue EvaluateConstant()
	{
		var value = right.EvaluateConstant();

		return new IntegerLiteralValue(value.IsZero);
	}
}
