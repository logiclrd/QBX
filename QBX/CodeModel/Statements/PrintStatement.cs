using System.Collections.Generic;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class PrintStatement : Statement
{
	public override StatementType Type => StatementType.Print;

	public Expression? FileNumberExpression { get; set; }
	public Expression? UsingExpression { get; set; }
	public List<PrintArgument> Arguments { get; } = new List<PrintArgument>();

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("PRINT ");

		if (FileNumberExpression != null)
		{
			writer.Write('#');
			FileNumberExpression.Render(writer);
			writer.Write(", ");
		}

		if (UsingExpression != null)
		{
			writer.Write("USING ");
			UsingExpression.Render(writer);
			writer.Write("; ");
		}

		for (int i = 0; i < Arguments.Count; i++)
		{
			if (i > 0)
				writer.Write(' ');

			if (UsingExpression != null)
				Arguments[i].CursorAction = PrintCursorAction.None;

			Arguments[i].Render(writer);
		}
	}
}
