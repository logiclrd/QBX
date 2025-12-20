namespace QBX.CodeModel.Statements;

public class LPrintWidthStatement : OutputWidthStatement
{
	public override StatementType Type => StatementType.LPrintWidth;

	public override void Render(TextWriter writer)
	{
		VerifyWidthExpression();

		writer.Write("WIDTH LPRINT ");
		WidthExpression!.Render(writer);
	}
}
