using QBX.CodeModel.Expressions;
using QBX.CodeModel.Statements;

namespace QBX.Parser;

public class ViewportStatement : Statement
{
	public override StatementType Type => StatementType.Viewport;

	public Expression? TopExpression { get; set; }
	public Expression? BottomExpression { get; set; }

	public override void Render(TextWriter writer)
	{
		writer.Write("VIEW PRINT");

		if ((TopExpression != null) != (BottomExpression != null))
			throw new Exception("Internal error: ViewportStatement with only one of the top & bottom expressions");

		if (TopExpression != null)
		{
			writer.Write(' ');
			TopExpression.Render(writer);
			writer.Write(" TO ");
			BottomExpression!.Render(writer);
		}
	}
}
