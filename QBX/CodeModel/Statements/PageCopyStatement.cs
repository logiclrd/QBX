using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class PageCopyStatement : Statement
{
	public override StatementType Type => StatementType.PageCopy;

	public Expression? SourcePageExpression { get; set; }
	public Expression? DestinationPageExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (SourcePageExpression == null)
			throw new Exception("Internal error: PageCopyStatement with no SourcePageExpression");
		if (DestinationPageExpression == null)
			throw new Exception("Internal error: PageCopyStatement with no DestinationPageExpression");

		writer.Write("PCOPY ");
		SourcePageExpression.Render(writer);
		writer.Write(", ");
		DestinationPageExpression.Render(writer);
	}
}
