using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class PrintStatement : Statement
{
	public override StatementType Type => StatementType.Print;

	public Expression? FileNumberExpression { get; set; }
	public Expression? UsingExpression { get; set; }
	public List<PrintArgument> Arguments { get; } = new List<PrintArgument>();

	public override void Render(TextWriter writer)
	{
		writer.Write("PRINT ");

		if (FileNumberExpression != null)
		{
			writer.Write('#');
			FileNumberExpression.Render(writer);
			writer.Write(", ");
		}

		if (UsingExpression != null)
		{
			writer.Write("USING ");
			UsingExpression.Render(writer);
			writer.Write("; ");
		}

		for (int i = 0; i < Arguments.Count; i++)
		{
			Arguments[i].Expression?.Render(writer);

			switch (Arguments[i].CursorAction)
			{
				case PrintCursorAction.None: writer.Write(';'); break;
				case PrintCursorAction.NextZone:
				{
					if (UsingExpression != null)
						writer.Write(';');
					else
						writer.Write(',');

					break;
				}
			}
		}
	}
}
