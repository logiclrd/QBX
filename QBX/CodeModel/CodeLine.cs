using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using QBX.CodeModel.Statements;

namespace QBX.CodeModel;

public class CodeLine : IRenderableCode
{
	public CompilationElement? CompilationElement { get; set; }

	// Line number must be numeric in format, but in practice is
	// parsed as any string ###.### with total length <= 40.
	public string? LineNumber { get; set; }
	public Label? Label { get; set; }
	public IReadOnlyList<Statement> Statements => _statements;
	public string? EndOfLineComment { get; set; }

	List<Statement> _statements = new List<Statement>();

	public bool IsEmpty =>
		(LineNumber == null) &&
		(Label == null) &&
		(EndOfLineComment == null) &&
		Statements.All(statement => statement is EmptyStatement);

	public bool IsCommentLine =>
		((Statements.Count == 1) && (Statements[0].Type == StatementType.Comment)) ||
		((Statements.Count == 0) && !string.IsNullOrWhiteSpace(EndOfLineComment));

	public bool IsDefTypeLine => (Statements.Count == 1) && (Statements[0].Type == StatementType.DefType);

	public static CodeLine CreateEmpty() => new CodeLine();

	public static CodeLine CreateUnparsed(StringBuilder builder)
		=> CreateUnparsed(builder.ToString());

	public static CodeLine CreateUnparsed(string text)
	{
		var line = new CodeLine();

		line.AppendStatement(
			new UnparsedStatement()
			{
				Text = text
			});

		return line;
	}

	public void AppendStatement(Statement statement)
	{
		_statements.Add(statement);
		statement.CodeLine = this;
	}

	public Statement RemoveStatementAt(int statementIndex)
	{
		var statement = _statements[statementIndex];

		_statements.RemoveAt(statementIndex);

		statement.CodeLine = null;

		return statement;
	}

	public void Render(TextWriter writer) => Render(writer, includeCRLF: true);

	public void Render(TextWriter writer, bool includeCRLF = true)
	{
		if (LineNumber != null)
			writer.Write(LineNumber);

		if (Label != null)
		{
			Label.Render(writer);

			if (Statements.Any() && (Statements[0].Indentation == ""))
				writer.Write(' ');
		}

		for (int i = 0; i < Statements.Count; i++)
		{
			var statement = Statements[i];
			bool hasNextStatement = (i + 1 < Statements.Count);

			statement.Render(writer);

			if (hasNextStatement)
			{
				var nextStatement = Statements[i + 1];

				if (statement.ExtraSpace)
					writer.Write(' ');

				writer.Write(":");

				if (nextStatement.Indentation == "")
					writer.Write(' ');
			}
		}

		writer.Write(EndOfLineComment);

		if (includeCRLF)
			writer.WriteLine();
	}
}
