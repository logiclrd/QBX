using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class LocateStatement : Statement
{
	public override StatementType Type => StatementType.Locate;

	public Expression? RowExpression { get; set; }
	public Expression? ColumnExpression { get; set; }
	public Expression? CursorVisibilityExpression { get; set; }
	public Expression? CursorStartExpression { get; set; }
	public Expression? CursorEndExpression { get; set; }

	public override void Render(TextWriter writer)
	{
		writer.Write("LOCATE ");
		RowExpression?.Render(writer);

		if ((ColumnExpression != null) || (CursorVisibilityExpression != null) || (CursorStartExpression != null) || (CursorEndExpression != null))
		{
			writer.Write(", ");
			ColumnExpression?.Render(writer);

			if ((CursorVisibilityExpression != null) || (CursorStartExpression != null) || (CursorEndExpression != null))
			{
				writer.Write(", ");
				CursorVisibilityExpression?.Render(writer);

				if ((CursorStartExpression != null) || (CursorEndExpression != null))
				{
					writer.Write(", ");
					CursorStartExpression?.Render(writer);

					if (CursorEndExpression != null)
					{
						writer.Write(", ");
						CursorEndExpression.Render(writer);
					}
				}
			}
		}
	}
}
