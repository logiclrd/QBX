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

			switch (Arguments[i].ExpressionType)
			{
				case PrintExpressionType.Tab:
					writer.Write("TAB(");
					Arguments[i].Expression?.Render(writer);
					writer.Write(')');
					break;
				case PrintExpressionType.Space:
					writer.Write("SPC(");
					Arguments[i].Expression?.Render(writer);
					writer.Write(')');
					break;
				default:
					Arguments[i].Expression?.Render(writer);
					break;
			}

			switch (Arguments[i].CursorAction)
			{
				case PrintCursorAction.None: writer.Write(';'); break;
				case PrintCursorAction.NextZone:
				{
					if (UsingExpression != null)
						writer.Write(';');
					else
						writer.Write(',');

					break;
				}
			}
		}
	}
}
