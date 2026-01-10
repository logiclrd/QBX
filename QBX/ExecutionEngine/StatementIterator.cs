using System;

using QBX.CodeModel.Statements;
using QBX.ExecutionEngine.Compiled.Statements;

namespace QBX.ExecutionEngine;

public abstract class StatementIterator
{
	public event Action<Statement>? Advanced;

	protected void OnAdvanced(Statement statement) => Advanced?.Invoke(statement);

	LabelStatement? _labelStatement = null;

	public LabelStatement? GetLabelStatement()
	{
		var retVal = _labelStatement;
		_labelStatement = null;
		return retVal;
	}

	protected void SetLabelStatement(LabelStatement labelStatement)
	{
		_labelStatement = labelStatement;
	}

	public abstract bool Advance();
	public abstract bool HaveCurrentStatement { get; }
}
