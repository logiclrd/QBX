using System.Collections.Generic;
using System.IO;
using System.Linq;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class CloseStatement : Statement
{
	public override StatementType Type => StatementType.Close;

	public List<Expression> FileNumberExpressions { get; } = new List<Expression>();

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("CLOSE");

		if (FileNumberExpressions.Any())
		{
			writer.Write(" #");

			for (int i = 0; i < FileNumberExpressions.Count; i++)
			{
				if (i > 0)
					writer.Write(", #");

				FileNumberExpressions[i].Render(writer);
			}
		}
	}
}
