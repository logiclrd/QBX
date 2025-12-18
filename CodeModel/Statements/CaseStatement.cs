using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

internal class CaseStatement : Statement
{
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
