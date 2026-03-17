using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using QBX.CodeModel.Statements;
using QBX.DevelopmentEnvironment;
using QBX.Firmware.Fonts;
using QBX.Utility;

namespace QBX.CodeModel;

public class CodeLine : IRenderableCode, IEditableLine
{
	public CompilationElement? CompilationElement { get; set; }

	// Populated by Compiler
	public int SourceLineIndex;

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

	class CharCounter : TextWriter
	{
		public int Count = 0;

		static readonly CP437Encoding s_cp437 = new CP437Encoding(ControlCharacterInterpretation.Semantic);

		public override Encoding Encoding => s_cp437;

		public override void Write(char value) => Count++;
		public override void Write(char[] buffer, int index, int count) => Count += count;
		public override void Write(char[]? buffer) { if (buffer != null) Count += buffer.Length; }
		public override void Write(ReadOnlySpan<char> buffer) => Count += buffer.Length;
		public override void Write(string? value) { if (value != null) Count += value.Length; }
		public override void Write(StringBuilder? value) { if (value != null) Count += value.Length; }
		public override void WriteLine() => Count += 2;
	}

	public int SizeInBytes
	{
		get
		{
			var counter = new CharCounter();

			Render(counter);

			return counter.Count;
		}
	}

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

	public IEnumerable<Statement> AllStatements
	{
		get
		{
			foreach (var statement in Statements)
			{
				yield return statement;

				foreach (var substatement in statement.Substatements)
					yield return substatement;
			}
		}
	}

	public void AppendStatement(Statement statement)
	{
		_statements.Add(statement);
		statement.SetCodeLineRecursive(this);
	}

	public Statement RemoveStatementAt(int statementIndex)
	{
		var statement = _statements[statementIndex];

		_statements.RemoveAt(statementIndex);

		statement.CodeLine = null;

		return statement;
	}

	public void Render(TextWriter writer) => Render(writer, includeCRLF: true);

	public void Render(TextWriter writer, bool includeCRLF = true) => Render(writer, includeCRLF, trimEnd: true);

	void Render(TextWriter baseWriter, bool includeCRLF, bool trimEnd)
	{
		var filteredWriter = baseWriter;

		if (trimEnd)
			filteredWriter = new TrimEndTextWriter(filteredWriter);

		var writer = new ColumnTrackingTextWriter(filteredWriter) { NewLine = baseWriter.NewLine };

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

			statement.SourceColumn = writer.Column + statement.Indentation.Length;
			statement.Render(writer);
			statement.SourceLength = writer.Column - statement.SourceColumn;

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

		if (EndOfLineComment != null)
		{
			int commentStart = EndOfLineComment.IndexOf('\'');

			if (commentStart < 0) // ?
				writer.Write(EndOfLineComment);
			else
			{
				var span = EndOfLineComment.AsSpan();

				var commentTextSpan = span.Slice(commentStart + 1);

				var reformattedCommentTextSpan = CommentStatement.FormatCommentText(commentTextSpan);

				if (reformattedCommentTextSpan != commentTextSpan)
					EndOfLineComment = string.Concat(span.Slice(0, commentStart + 1), reformattedCommentTextSpan);

				writer.Write(EndOfLineComment);
			}
		}

		if (includeCRLF)
			baseWriter.WriteLine();
	}

	[ThreadStatic]
	static StringWriter? s_testBuffer;

	public int ComputeLength()
	{
		s_testBuffer ??= new StringWriter();

		var testBuffer = s_testBuffer.GetStringBuilder();

		testBuffer.Clear();

		Render(s_testBuffer, includeCRLF: false, trimEnd: false);

		return testBuffer.Length;
	}
}
