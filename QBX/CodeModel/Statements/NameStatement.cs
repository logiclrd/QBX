using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class NameStatement : Statement
{
	public override StatementType Type => StatementType.Name;

	public Expression? OldFileSpecExpression { get; set; }
	public Expression? NewFileSpecExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (OldFileSpecExpression == null)
			throw new Exception("KillStatement with no OldFileSpecExpression");
		if (NewFileSpecExpression == null)
			throw new Exception("KillStatement with no NewFileSpecExpression");

		writer.Write("NAME ");
		OldFileSpecExpression.Render(writer);
		writer.Write(" AS ");
		NewFileSpecExpression.Render(writer);
	}
}
