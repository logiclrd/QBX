using System.Collections.Generic;
using System.IO;

using QBX.CodeModel.Statements;

namespace QBX.CodeModel.Expressions;

public class ExpressionList : IRenderableCode
{
	public List<Expression> Expressions { get; set; } = new List<Expression>();

	public int Count => Expressions.Count;

	public void ClaimTokens(Statement owner)
	{
		foreach (var expression in Expressions)
			expression.ClaimTokens(owner);
	}

	public void Render(TextWriter writer)
	{
		for (int i=0; i < Expressions.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			Expressions[i].Render(writer);
		}
	}
}
