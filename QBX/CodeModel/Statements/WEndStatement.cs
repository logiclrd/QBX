namespace QBX.CodeModel.Statements;

public class WEndStatement : Statement
{
	public override StatementType Type => StatementType.WEnd;

	public override void Render(TextWriter writer)
	{
		writer.Write("WEND");
	}
}
