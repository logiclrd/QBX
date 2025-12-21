using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class IfStatement : Statement
{
	public override StatementType Type => StatementType.If;
	public Expression? ConditionExpression { get; set; }
	public List<Statement>? ThenBody { get; set; }
	public List<Statement>? ElseBody { get; set; }

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
		writer.Write(" THEN");

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

				ThenBody[i].Render(writer);
			}
		}

		if (ElseBody != null)
		{
			writer.Write(" ELSE ");

			for (int i = 0; i < ElseBody.Count; i++)
			{
				if (i > 0)
				{
					writer.Write(':');

					if (ElseBody[i].Indentation == "")
						writer.Write(':');
				}

				ElseBody[i].Render(writer);
			}
		}
	}
}
