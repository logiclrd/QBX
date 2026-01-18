using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class OutStatement : Statement
{
	public override StatementType Type => StatementType.Out;

	public Expression? PortExpression { get; set; }
	public Expression? DataExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if ((PortExpression == null) || (DataExpression == null))
			throw new Exception("Internal error: OutStatement with a missing Port or Data expression");

		writer.Write("OUT ");
		PortExpression.Render(writer);
		writer.Write(", ");
		DataExpression.Render(writer);
	}
}
