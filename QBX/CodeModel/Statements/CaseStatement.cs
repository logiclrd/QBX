using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class CaseStatement : Statement
{
	public override StatementType Type => StatementType.Case;

	public CaseExpressionList? Expressions { get; set; }
	public bool MatchElse { get; internal set; }

	public CaseStatement()
	{
	}

	public CaseStatement(CaseExpressionList expressions)
	{
		Expressions = expressions;
	}

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("CASE ");

		if ((Expressions != null) && Expressions.Expressions.Any() && MatchElse)
			throw new Exception("Internal error: CASE cannot have expressions and be CASE ELSE at the same time");

		if (((Expressions == null) || !Expressions.Expressions.Any()) && !MatchElse)
			throw new Exception("Internal error: empty CaseStatement");

		if (MatchElse)
			writer.Write("ELSE");
		else
			Expressions!.Render(writer);
	}
}
