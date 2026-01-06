using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class FileWidthStatement : OutputWidthStatement
{
	public override StatementType Type => StatementType.FileWidth;

	public Expression? FileNumberExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if (FileNumberExpression == null)
			throw new Exception("Internal error: FileWidthStatement with no FileNumberExpression");

		VerifyWidthExpression();

		writer.Write("WIDTH #");
		FileNumberExpression.Render(writer);
		writer.Write(", ");
		WidthExpression!.Render(writer);
	}
}
