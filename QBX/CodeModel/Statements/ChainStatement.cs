using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class ChainStatement : Statement
{
	public override StatementType Type => StatementType.Chain;

	public Expression? FileNameExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (FileNameExpression == null)
			throw new Exception($"Internal error: ChainStatement with no FileNameExpression");

		writer.Write("CHAIN ");
		FileNameExpression.Render(writer);
	}
}
