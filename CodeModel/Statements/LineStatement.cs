using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class LineStatement : Statement
{
	public override StatementType Type => StatementType.Line;

	public bool FromStep { get; set; }
	public Expression? FromXExpression { get; set; }
	public Expression? FromYExpression { get; set; }

	public bool ToStep { get; set; }
	public Expression? ToXExpression { get; set; }
	public Expression? ToYExpression { get; set; }

	public Expression? ColorExpression { get; set; }
	public LineDrawStyle DrawStyle { get; set; } = LineDrawStyle.Line;
	public Expression? StyleExpression { get; set; }

	public override void Render(TextWriter writer)
	{
		writer.Write("LINE ");

		if (FromStep)
		{
			writer.Write("STEP");

			if ((FromXExpression == null) || (FromYExpression == null))
				throw new Exception("Internal error: LineStatement with FromStep but no From coordinate expressions");
		}

		if ((FromXExpression != null) || (FromYExpression != null))
		{
			if ((FromXExpression == null) || (FromYExpression == null))
				throw new Exception("Internal error: LineStatement missing one of the From coordinate expressions");

			writer.Write('(');
			FromXExpression.Render(writer);
			writer.Write(", ");
			FromYExpression.Render(writer);
			writer.Write(')');
		}

		writer.Write('-');

		if (ToStep)
			writer.Write("STEP");

		if ((ToXExpression == null) || (ToYExpression == null))
			throw new Exception("Internal error: LineStatement missing one or both of the To coordinate expressions");

		writer.Write('(');
		ToXExpression.Render(writer);
		writer.Write(", ");
		ToYExpression.Render(writer);
		writer.Write(")");

		if ((ColorExpression != null) || (DrawStyle != LineDrawStyle.Line) || (StyleExpression != null))
		{
			writer.Write(", ");
			ColorExpression?.Render(writer);

			if ((DrawStyle != LineDrawStyle.Line) || (StyleExpression != null))
			{
				writer.Write(", ");

				switch (DrawStyle)
				{
					case LineDrawStyle.Box: writer.Write('B'); break;
					case LineDrawStyle.FilledBox: writer.Write("BF"); break;
				}

				if (StyleExpression != null)
				{
					writer.Write(", ");
					StyleExpression.Render(writer);
				}
			}
		}
	}
}
