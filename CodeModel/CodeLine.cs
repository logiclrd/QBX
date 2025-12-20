using QBX.CodeModel.Statements;

namespace QBX.CodeModel;

public class CodeLine : IRenderableCode
{
	public string Indentation { get; set; } = "";
	public List<Statement> Statements { get; } = new List<Statement>();

	public bool IsEmpty => (Indentation == "") && !Statements.Any();

	public bool IsCommentLine => (Statements.Count == 1) && (Statements[0].Type == StatementType.Comment);

	public void Render(TextWriter writer)
	{
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
