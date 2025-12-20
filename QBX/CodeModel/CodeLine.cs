using QBX.CodeModel.Statements;

namespace QBX.CodeModel;

public class CodeLine : IRenderableCode
{
	// Line number must be numeric in format, but in practice is
	// parsed as any string ###.### with total length <= 40.
	public string? LineNumber { get; set; }
	public Label? Label { get; set; }
	public string Indentation { get; set; } = "";
	public List<Statement> Statements { get; } = new List<Statement>();

	public bool IsEmpty => (Indentation == "") && !Statements.Any();

	public bool IsCommentLine => (Statements.Count == 1) && (Statements[0].Type == StatementType.Comment);

	public void Render(TextWriter writer)
	{
		if ((LineNumber != null) && (Label != null))
			throw new Exception("Internal error: A line cannot have both a line number and a label");

		if (LineNumber != null)
			writer.Write(LineNumber);

		if (Label != null)
		{
			Label.Render(writer);

			if ((Indentation == "") && Statements.Any())
				writer.Write(' ');
		}

		writer.Write(Indentation);

		for (int i = 0; i < Statements.Count; i++)
		{
			var statement = Statements[i];
			bool hasNextStatement = (i + 1 < Statements.Count);

			statement.Render(writer);

			if (hasNextStatement)
			{
				if (statement.ExtraSpace)
					writer.Write(' ');

				writer.Write(": ");
			}
		}

		writer.WriteLine();
	}
}
