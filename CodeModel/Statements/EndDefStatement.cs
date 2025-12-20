namespace QBX.CodeModel.Statements;

public class EndDefStatement : Statement
{
	public override StatementType Type => StatementType.EndDef;

	public override void Render(TextWriter writer)
	{
		writer.Write("END DEF");
	}
}
