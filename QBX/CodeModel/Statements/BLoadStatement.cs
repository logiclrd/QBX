using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class BLoadStatement : Statement
{
	public override StatementType Type => StatementType.BLoad;

	public Expression? FileNameExpression { get; set; }
	public Expression? OffsetExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (FileNameExpression == null)
			throw new Exception($"Internal error: BLoadStatement with no FileNameExpression");

		writer.Write("BLOAD ");
		FileNameExpression.Render(writer);

		if (OffsetExpression != null)
		{
			writer.Write(", ");
			OffsetExpression.Render(writer);
		}
	}
}
