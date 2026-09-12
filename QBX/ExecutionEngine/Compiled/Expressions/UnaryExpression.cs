namespace QBX.ExecutionEngine.Compiled.Expressions;

public abstract class UnaryExpression(Evaluable right) : Evaluable
{
	public Evaluable Right => right;

	public override bool CanEvaluateDirectWithoutEmbedding => right.CanEvaluateDirectWithoutEmbedding;

	public override bool IsConstant => right.IsConstant;

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref right);
	}
}
