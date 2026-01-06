using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class PlayStatement : Statement
{
	public override StatementType Type => StatementType.Play;

	public Expression? CommandExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (CommandExpression == null)
			throw new Exception("Internal error: PlayStatement with no CommandExpression");

		writer.Write("PLAY ");
		CommandExpression.Render(writer);
	}
}
