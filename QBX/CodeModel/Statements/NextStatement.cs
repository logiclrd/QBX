using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class NextStatement : Statement
{
	public override StatementType Type => StatementType.Next;

	public List<Expression> CounterExpressions { get; } = new List<Expression>();

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("NEXT");

		if (CounterExpressions.Any())
		{
			writer.Write(' ');

			for (int i = 0; i < CounterExpressions.Count; i++)
			{
				if (i > 0)
					writer.Write(", ");

				CounterExpressions[i].Render(writer);
			}
		}
	}
}
