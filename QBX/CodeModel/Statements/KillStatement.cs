using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class KillStatement : Statement
{
	public override StatementType Type => StatementType.Kill;

	public Expression? PatternExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (PatternExpression == null)
			throw new Exception("KillStatement with no PatternExpression");

		writer.Write("KILL ");
		PatternExpression.Render(writer);
	}
}
