using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class PaintStatement : Statement
{
	public override StatementType Type => StatementType.Paint;

	public bool Step { get; set; }
	public Expression? XExpression { get; set; }
	public Expression? YExpression { get; set; }
	public Expression? PaintExpression { get; set; }
	public Expression? BorderColourExpression { get; set; }
	public Expression? BackgroundExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (XExpression == null)
			throw new Exception("Internal error: CircleStatement with no XExpression");
		if (YExpression == null)
			throw new Exception("Internal error: CircleStatement with no YExpression");

		writer.Write("PAINT ");
		if (Step)
			writer.Write("STEP");
		writer.Write('(');
		XExpression.Render(writer);
		writer.Write(", ");
		YExpression.Render(writer);
		writer.Write(")");

		if ((PaintExpression != null) || (BorderColourExpression != null) || (BackgroundExpression != null))
		{
			writer.Write(", ");
			PaintExpression?.Render(writer);

			if ((BorderColourExpression != null) || (BackgroundExpression != null))
			{
				writer.Write(", ");
				BorderColourExpression?.Render(writer);

				if (BackgroundExpression != null)
				{
					writer.Write(", ");
					BackgroundExpression?.Render(writer);
				}
			}
		}
	}
}
