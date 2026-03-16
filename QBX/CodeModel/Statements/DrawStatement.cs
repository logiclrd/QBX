using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class DrawStatement : Statement
{
	public override StatementType Type => StatementType.Draw;

	public Expression? CommandExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (CommandExpression == null)
			throw new Exception("Internal error: DrawStatement with no CommandExpression");

		writer.Write("DRAW ");
		CommandExpression.Render(writer);
	}
}
