/*
 * TODO
 * 
using QBX.CodeModel.Expressions;
using System.Diagnostics;

namespace QBX.Tests.Parser.Statements;

internal class ViewStatement
{
	public override StatementType Type => StatementType.View;

	public bool AbsoluteCoordinates { get; set; }
	public Expression? FromXExpression { get; set; }
	public Expression? FromYExpression { get; set; }
	public Expression? ToXExpression { get; set; }
	public Expression? ToYExpression { get; set; }
	public Expression? FillColourExpression { get; set; }
	public Expression? BorderColourExpression { get; set; }

	public override void Render(TextWriter writer)
	{
		writer.Write("VIEW ");

		if (AbsoluteCoordinates)
			writer.Write("SCREEN ");

		if ((FromXExpression == null) || (FromYExpression == null))
			throw new Exception("Internal error: ViewStatement missing From coordinate expression(s)");
		if ((ToXExpression == null) || (ToYExpression == null))
			throw new Exception("Internal error: ViewStatement missing To coordinate expression(s)");

		writer.Write('(');
		FromXExpression.Render(writer);
		writer.Write(", ");
		FromYExpression.Render(writer);
		writer.Write(")-(");
		ToXExpression.Render(writer);
		writer.Write(", ");
		ToYExpression.Render(writer);
		writer.Write(")");

		if ((FillColourExpression != null) || (BorderColourExpression != null))
		{
			writer.Write(", ");
			FillColourExpression?.Render(writer);

			if (BorderColourExpression != null)
			{
				writer.Write(", ");
				BorderColourExpression.Render(writer);
			}
		}
	}
}

*/
