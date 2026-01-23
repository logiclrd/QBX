using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class IfStatement : Statement
{
	public override StatementType Type => StatementType.If;
	public Expression? ConditionExpression { get; set; }
	public List<Statement>? ThenBody { get; set; }
	public List<Statement>? ElseBody { get; set; }
	public bool OmitThen { get; set; }

	public override IEnumerable<Statement> Substatements
	{
		get
		{
			if (ThenBody != null)
			{
				foreach (var statement in ThenBody)
				{
					yield return statement;

					foreach (var substatement in statement.Substatements)
						yield return substatement;
				}
			}

			if (ElseBody != null)
			{
				foreach (var statement in ElseBody)
				{
					yield return statement;

					foreach (var substatement in statement.Substatements)
						yield return substatement;
				}
			}
		}
	}

	protected virtual void Validate()
	{
		if (ConditionExpression == null)
			throw new Exception($"Internal error: {Type}Statement with no ConditionExpression");

		if ((ThenBody != null) && !ThenBody.Any())
			throw new Exception($"Internal error: {Type}Statement with an empty ThenBody list");
		if ((ElseBody != null) && !ElseBody.Any())
			throw new Exception($"Internal error: {Type}Statement with an empty ElseBody list");

		if ((ThenBody == null) && (ElseBody != null))
			throw new Exception($"Internal error: {Type}Statement with an ElseBody and no ThenBody");
	}

	protected virtual void RenderStatementName(TextWriter writer)
		=> writer.Write("IF");

	protected override void RenderImplementation(TextWriter writer)
	{
		Validate();

		RenderStatementName(writer);
		writer.Write(' ');
		ConditionExpression?.Render(writer);

		if (!OmitThen)
			writer.Write(" THEN");

		var columnTracker = writer as ColumnTrackingTextWriter;

		if (ThenBody != null)
		{
			writer.Write(' ');

			for (int i = 0; i < ThenBody.Count; i++)
			{
				if (i > 0)
				{
					writer.Write(':');

					if (ThenBody[i].Indentation == "")
						writer.Write(' ');
				}

				if (columnTracker != null)
					ThenBody[i].SourceColumn = columnTracker.Column + ThenBody[i].Indentation.Length;
				ThenBody[i].Render(writer);
				if (columnTracker != null)
					ThenBody[i].SourceLength = columnTracker.Column - ThenBody[i].SourceColumn;
			}

			if (ElseBody != null)
			{
				if (ElseBody.Count == 0)
					writer.Write(" ELSE");
				else
				{
					writer.Write(" ELSE ");

					for (int i = 0; i < ElseBody.Count; i++)
					{
						if (i > 0)
						{
							writer.Write(':');

							if (ElseBody[i].Indentation == "")
								writer.Write(' ');
						}

						if (columnTracker != null)
							ElseBody[i].SourceColumn = columnTracker.Column + ElseBody[i].Indentation.Length;
						ElseBody[i].Render(writer);
						if (columnTracker != null)
							ElseBody[i].SourceLength = columnTracker.Column - ElseBody[i].SourceColumn;
					}
				}
			}
		}
	}
}
