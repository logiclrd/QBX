using System.Collections.Generic;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class WriteStatement : Statement
{
	public override StatementType Type => StatementType.Write;

	public Expression? FileNumberExpression { get; set; }
	public List<Expression> Arguments { get; } = new List<Expression>();

	protected override void RenderImplementation(TextWriter writer)
	{
		if (FileNumberExpression == null)
			throw new System.Exception("WriteStatement with no FileNumberExpression");

		writer.Write("WRITE #");
		FileNumberExpression.Render(writer);

		foreach (var argument in Arguments)
		{
			writer.Write(", ");
			argument.Render(writer);
		}
	}
}
