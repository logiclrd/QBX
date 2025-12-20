using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class ForStatement : Statement
{
	public override StatementType Type => StatementType.For;

	public string CounterVariable { get; set; } = "";
	public Expression? StartExpression { get; set; }
	public Expression? EndExpression { get; set; }
	public Expression? StepExpression { get; set; }

	public override void Render(TextWriter writer)
	{
		if (StartExpression == null)
			throw new Exception("Internal error: FOR statement with no start expression");
		if (EndExpression == null)
			throw new Exception("Internal error: FOR statement with no end expression");

		writer.Write("FOR {0} = ", CounterVariable);
		StartExpression.Render(writer);
		writer.Write(" TO ");
		EndExpression.Render(writer);

		if (StepExpression != null)
		{
			writer.Write(" STEP ");
			StepExpression.Render(writer);
		}
	}
}
