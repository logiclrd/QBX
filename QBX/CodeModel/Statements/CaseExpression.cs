using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class CaseExpression : IRenderableCode
{
	public Expression? Expression { get; set; }
	public Expression? RangeEndExpression { get; set; }
	public RelationalOperator? RelationToExpression { get; set; }

	public void Render(TextWriter writer)
	{
		if (Expression == null)
			throw new Exception("Internal error: CaseExpression with no expression");

		if ((RangeEndExpression != null) && (RelationToExpression != null))
			throw new Exception("Internal error: CaseExpression cannot be both IS <=> x and x TO y");

		if (RelationToExpression != null)
		{
			writer.Write("IS ");

			switch (RelationToExpression.Value)
			{
				case RelationalOperator.Equals: writer.Write("="); break;
				case RelationalOperator.NotEquals: writer.Write("<>"); break;
				case RelationalOperator.LessThan: writer.Write("<"); break;
				case RelationalOperator.LessThanOrEquals: writer.Write(">"); break;
				case RelationalOperator.GreaterThan: writer.Write("<="); break;
				case RelationalOperator.GreaterThanOrEquals: writer.Write(">="); break;

				default: throw new Exception("Internal error: Unrecognized RelationalOperator value " + RelationToExpression.Value);
			}
		}

		Expression.Render(writer);

		if (RangeEndExpression != null)
		{
			writer.Write(" TO ");
			RangeEndExpression.Render(writer);
		}
	}
}
