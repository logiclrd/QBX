using System;
using System.IO;

using QBX.CodeModel.Expressions;
using QBX.Parser;

namespace QBX.CodeModel.Statements;

public class RunStatement : Statement
{
	public override StatementType Type => StatementType.Run;

	public Identifier? StartingLineNumber { get; set; }
	public Expression? FileNameExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (StartingLineNumber != null)
		{
			writer.Write("RUN ");
			writer.Write(StartingLineNumber);
		}
		else if (FileNameExpression != null)
		{
			writer.Write("RUN ");
			FileNameExpression.Render(writer);
		}
		else
			writer.Write("RUN");
	}
}
