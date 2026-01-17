using System;

using QBX.CodeModel.Statements;
using QBX.ExecutionEngine.Compiled;
using QBX.ExecutionEngine.Compiled.Statements;
using QBX.ExecutionEngine.Execution;

namespace QBX.ExecutionEngine;

public abstract class StatementIterator
{
	public event Action<Statement>? Advanced;

	protected void OnAdvanced(Statement statement) => Advanced?.Invoke(statement);

	LabelStatement? _lineNumberStatement = null;
	LabelStatement? _labelStatement = null;

	public LabelStatement? GetLineNumberStatement()
	{
		var retVal = _lineNumberStatement;
		_lineNumberStatement = null;
		return retVal;
	}

	protected void SetLineNumberStatement(LabelStatement lineNumberStatement)
	{
		_lineNumberStatement = lineNumberStatement;
	}

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
	public abstract bool ExpectEnd();

	public bool ProcessLabels(DataParser dataParser, Sequence? body)
		=> ProcessLabels(dataParser, body, out _);

	public bool ProcessLabels(DataParser dataParser, Sequence? body, out LabelStatement? labelStatement)
	{
		LabelStatement? labelStatementRet = null;

		bool haveLabels = false;

		void ProcessLabel(LabelStatement? possibleLabelStatement)
		{
			if (possibleLabelStatement != null)
			{
				haveLabels = true;
				labelStatementRet = possibleLabelStatement;

				dataParser.AddLabel(possibleLabelStatement);
				body?.Append(possibleLabelStatement);
			}
		}

		ProcessLabel(GetLineNumberStatement());
		ProcessLabel(GetLabelStatement());

		labelStatement = labelStatementRet;

		return haveLabels;
	}
}
