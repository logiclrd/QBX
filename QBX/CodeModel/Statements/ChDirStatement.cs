using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class ChDirStatement : Statement
{
	public override StatementType Type => StatementType.ChDir;

	public Expression? PathExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (PathExpression == null)
			throw new Exception("ChDirStatement with no PathExpression");

		writer.Write("CHDIR ");
		PathExpression.Render(writer);
	}
}
