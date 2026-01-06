using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class PaletteStatement : Statement
{
	public override StatementType Type => StatementType.Palette;

	public Expression? ArrayExpression { get; set; }
	public Expression? AttributeExpression { get; set; }
	public Expression? ColourExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (ArrayExpression != null)
		{
			if ((AttributeExpression != null) || (ColourExpression != null))
				throw new Exception("Internal error: PaletteStatement with both PALETTE and PALETTE USING configuration.");
		}

		if ((ColourExpression != null) && (AttributeExpression == null))
			throw new Exception("Internal error: PaletteStatement with ColourExpression but no AttributeExpression");

		writer.Write("PALETTE ");

		if (ArrayExpression != null)
		{
			writer.Write("USING ");
			ArrayExpression.Render(writer);
		}
		else if (AttributeExpression != null)
		{
			AttributeExpression.Render(writer);

			if (ColourExpression != null)
			{
				writer.Write(", ");
				ColourExpression.Render(writer);
			}
		}
	}
}
