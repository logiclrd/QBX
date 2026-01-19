using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class ErrorStatement : Statement
{
	public override StatementType Type => StatementType.Error;

	public Expression? ErrorNumberExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (ErrorNumberExpression == null)
			throw new Exception("Internal error: ErrorStatement with no ErrorNumberExpression");

		writer.Write("ERROR ");
		ErrorNumberExpression.Render(writer);
	}
}
