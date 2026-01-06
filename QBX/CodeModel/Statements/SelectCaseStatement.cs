using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class SelectCaseStatement : Statement
{
	public override StatementType Type => StatementType.SelectCase;

	public Expression? Expression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (Expression == null)
			throw new Exception("Internal error: SelectCaseStatement with no expression");

		writer.Write("SELECT CASE ");
		Expression.Render(writer);
	}
}
