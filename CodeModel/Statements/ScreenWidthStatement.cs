using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class ScreenWidthStatement : Statement
{
	public override StatementType Type => StatementType.ScreenWidth;

	public Expression? WidthExpression { get; set; }
	public Expression? HeightExpression { get; set; }

	public override void Render(TextWriter writer)
	{
		if ((WidthExpression == null) && (HeightExpression == null))
			throw new Exception("Internal error: ScreenWidthStatement with neither Width or Height expression");

		writer.Write("WIDTH ");
		WidthExpression?.Render(writer);

		if (HeightExpression != null)
		{
			writer.Write(", ");
			HeightExpression.Render(writer);
		}
	}
}
