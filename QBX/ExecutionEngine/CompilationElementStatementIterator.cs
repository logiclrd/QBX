using QBX.CodeModel;
using QBX.CodeModel.Statements;
using QBX.ExecutionEngine.Compiled.Statements;
using System.Linq;

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
}
