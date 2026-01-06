using System.IO;

namespace QBX.CodeModel.Statements;

public class LPrintWidthStatement : OutputWidthStatement
{
	public override StatementType Type => StatementType.LPrintWidth;

	protected override void RenderImplementation(TextWriter writer)
	{
		VerifyWidthExpression();

		writer.Write("WIDTH LPRINT ");
		WidthExpression!.Render(writer);
	}
}
