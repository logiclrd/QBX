using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class EndStatement : Statement
{
	public override StatementType Type => StatementType.End;

	public Expression? Expression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("END");

		if (Expression != null)
		{
			writer.Write(' ');
			Expression.Render(writer);
		}
	}
}
