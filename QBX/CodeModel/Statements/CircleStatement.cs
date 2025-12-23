using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class CircleStatement : Statement
{
	public override StatementType Type => StatementType.Circle;

	public bool Step { get; set; }
	public Expression? XExpression { get; set; }
	public Expression? YExpression { get; set; }
	public Expression? RadiusExpression { get; set; }
	public Expression? ColourExpression { get; set; }
	public Expression? StartExpression { get; set; }
	public Expression? EndExpression { get; set; }
	public Expression? AspectExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (XExpression == null)
			throw new Exception("Internal error: CircleStatement with no XExpression");
		if (YExpression == null)
			throw new Exception("Internal error: CircleStatement with no YExpression");
		if (RadiusExpression == null)
			throw new Exception("Internal error: CircleStatement with no RadiusExpression");

		writer.Write("CIRCLE ");
		if (Step)
			writer.Write("STEP");
		writer.Write('(');
		XExpression.Render(writer);
		writer.Write(", ");
		YExpression.Render(writer);
		writer.Write("), ");
		RadiusExpression.Render(writer);

		if ((ColourExpression != null) || (StartExpression != null) || (EndExpression != null) || (AspectExpression != null))
		{
			writer.Write(", ");
			ColourExpression?.Render(writer);

			if ((StartExpression != null) || (EndExpression != null) || (AspectExpression != null))
			{
				writer.Write(", ");
				StartExpression?.Render(writer);

				if ((EndExpression != null) || (AspectExpression != null))
				{
					writer.Write(", ");
					EndExpression?.Render(writer);

					if (AspectExpression != null)
					{
						writer.Write(", ");
						AspectExpression.Render(writer);
					}
				}
			}
		}
	}
}
