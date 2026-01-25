using System.Linq;

using QBX.CodeModel;
using QBX.CodeModel.Statements;
using QBX.ExecutionEngine.Compiled.Statements;

namespace QBX.ExecutionEngine;

public class CompilationElementStatementIterator(CompilationElement element, CodeLine line, int lineIndex, int statementIndex) : StatementIterator
{
	public int LineIndex => lineIndex;
	public int StatementIndex => statementIndex;

	public override bool Advance()
	{
		statementIndex++;

		while (statementIndex >= line.Statements.Count)
		{
			lineIndex++;
			statementIndex = 0;

			if (lineIndex >= element.Lines.Count)
				return false;

			line = element.Lines[lineIndex];

			line.LineIndex = LineIndex;

			if (line.LineNumber != null)
				SetLineNumberStatement(new LabelStatement(line.LineNumber, line.Statements.First()));
			if (line.Label != null)
				SetLabelStatement(new LabelStatement(line.Label.Name, line.Statements.First()));
		}

		OnAdvanced(line.Statements[statementIndex]);

		return true;
	}

	public override bool HaveCurrentStatement
	{
		get
		{
			return
				(lineIndex < element.Lines.Count) &&
				(statementIndex < line.Statements.Count);
		}
	}

	public override bool ExpectEnd()
	{
		while (HaveCurrentStatement)
		{
			var statement = element.Lines[lineIndex].Statements[statementIndex];

			if ((statement is not null) && (statement is not EmptyStatement))
				return false;

			Advance();

			if (element.Lines[lineIndex].EndOfLineComment != null)
				return false;

			if (GetLineNumberStatement() is not null)
				return false;
			if (GetLabelStatement() is not null)
				return false;
		}

		return true;
	}
}
