using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class CaseStatement : Statement
{
	public override StatementType Type => StatementType.Case;

	public ExpressionList Expressions { get; set; }

	public CaseStatement(ExpressionList expressions)
	{
		Expressions = expressions;
	}

	public override void Render(TextWriter writer)
	{
		writer.Write("CASE ");
		Expressions.Render(writer);
	}
}
