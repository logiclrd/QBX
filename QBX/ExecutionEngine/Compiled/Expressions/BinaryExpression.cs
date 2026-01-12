namespace QBX.ExecutionEngine.Compiled.Expressions;

public abstract class BinaryExpression(Evaluable left, Evaluable right) : Evaluable
{
	public Evaluable Left => left;
	public Evaluable Right => right;

	public override bool IsConstant => left.IsConstant && right.IsConstant;

	public override void CollapseConstantSubexpressions()
	{
		CollapseConstantExpression(ref left);
		CollapseConstantExpression(ref right);
	}
}
