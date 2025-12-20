/*
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

public class NextStatement
{
	public override StatementType Type => StatementType.Next;

	public List<Expression> CounterExpressions { get; } = new List<Expression>();

	public override void Render(TextWriter writer)
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

*/
