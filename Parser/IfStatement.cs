using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;

namespace QBX.Parser;

public class IfStatement : Statement
{
	public override StatementType Type => StatementType.If;
	public Expression? ConditionExpression { get; set; }

	protected virtual void RenderStatementName(TextWriter writer)
		=> writer.Write("IF");

	public override void Render(TextWriter writer)
	{
		if (ConditionExpression == null)
			throw new Exception("Internal error: IfStatement with no ConditionExpression");

		RenderStatementName(writer);
		writer.Write(' ');
		ConditionExpression.Render(writer);
		writer.Write(" THEN");
	}
}
