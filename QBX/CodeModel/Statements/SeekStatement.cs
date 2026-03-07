using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class SeekStatement : Statement
{
	public override StatementType Type => StatementType.Seek;

	public bool IncludeNumberSign { get; set; }
	public Expression? FileNumberExpression { get; set; }
	public Expression? PositionExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (FileNumberExpression == null)
			throw new Exception($"Internal error: SeekStatement with no FileNumberExpression");
		if (PositionExpression == null)
			throw new Exception($"Internal error: SeekStatement with no PositionExpression");

		writer.Write("SEEK ");
		if (IncludeNumberSign)
			writer.Write('#');
		FileNumberExpression.Render(writer);
		writer.Write(", ");
		PositionExpression.Render(writer);
	}
}
