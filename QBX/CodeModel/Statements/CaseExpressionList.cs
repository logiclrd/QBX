using System.Collections.Generic;
using System.IO;

namespace QBX.CodeModel.Statements;

public class CaseExpressionList : IRenderableCode
{
	public List<CaseExpression> Expressions { get; set; } = new List<CaseExpression>();

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
