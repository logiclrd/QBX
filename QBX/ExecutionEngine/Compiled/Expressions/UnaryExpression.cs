namespace QBX.ExecutionEngine.Compiled.Expressions;

public abstract class UnaryExpression(Evaluable right) : Evaluable
{
	public Evaluable Right => right;

	public override bool IsConstant => right.IsConstant;

	public override void CollapseConstantSubexpressions()
	{
		if (right.IsConstant)
			right = right.EvaluateConstant();
		else
			right.CollapseConstantSubexpressions();
	}
}
