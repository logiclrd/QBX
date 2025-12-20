namespace QBX.CodeModel.Statements;

public class EndTypeStatement : Statement
{
	public override StatementType Type => StatementType.EndType;

	public override void Render(TextWriter writer)
	{
		writer.Write("END TYPE");
	}
}
