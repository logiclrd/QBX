using System.Collections.Generic;

using QBX.CodeModel.Statements;

namespace QBX.ExecutionEngine;

public class ListStatementIterator(IList<Statement> statements, int statementIndex) : StatementIterator
{
	public int StatementIndex => statementIndex;

	public override bool Advance()
	{
		statementIndex++;

		if (statementIndex < statements.Count)
		{
			OnAdvanced(statements[statementIndex]);
			return true;
		}
		else
			return false;
	}

	public override bool HaveCurrentStatement
		=> (statementIndex < statements.Count);

	public override bool ExpectEnd()
	{
		while (statementIndex < statements.Count)
		{
			if (statements[statementIndex] is not EmptyStatement)
				return false;

			statementIndex++;
		}

		return true;
	}
}
