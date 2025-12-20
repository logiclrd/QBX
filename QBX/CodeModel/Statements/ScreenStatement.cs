using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class ScreenStatement : Statement
{
	public override StatementType Type => StatementType.Screen;

	public Expression? ModeExpression { get; set; }
	public Expression? ColourSwitchExpression { get; set; }
	public Expression? ActivePageExpression { get; set; }
	public Expression? VisiblePageExpression { get; set; }

	public override void Render(TextWriter writer)
	{
		writer.Write("SCREEN ");
		ModeExpression?.Render(writer);

		if ((ColourSwitchExpression != null) || (ActivePageExpression != null) || (VisiblePageExpression != null))
		{
			writer.Write(", ");
			ColourSwitchExpression?.Render(writer);

			if ((ActivePageExpression != null) || (VisiblePageExpression != null))
			{
				writer.Write(", ");
				ActivePageExpression?.Render(writer);

				if (VisiblePageExpression != null)
				{
					writer.Write(", ");
					VisiblePageExpression?.Render(writer);
				}
			}
		}
	}
}
