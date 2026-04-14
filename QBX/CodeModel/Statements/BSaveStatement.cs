using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class BSaveStatement : BlockIOStatement
{
	public override StatementType Type => StatementType.BSave;

	public Expression? FileNameExpression { get; set; }
	public Expression? OffsetExpression { get; set; }
	public Expression? LengthExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (FileNameExpression == null)
			throw new Exception($"Internal error: BSaveStatement with no FileNameExpression");
		if (OffsetExpression == null)
			throw new Exception($"Internal error: BSaveStatement with no OffsetExpression");
		if (LengthExpression == null)
			throw new Exception($"Internal error: BSaveStatement with no LengthExpression");

		writer.Write("BSAVE ");
		FileNameExpression.Render(writer);
		writer.Write(", ");
		OffsetExpression.Render(writer);
		writer.Write(", ");
		LengthExpression.Render(writer);
	}
}
