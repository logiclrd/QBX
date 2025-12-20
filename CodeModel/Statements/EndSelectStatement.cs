namespace QBX.CodeModel.Statements;

public class EndSelectStatement : Statement
{
	public override StatementType Type => StatementType.EndSelect;

	public override void Render(TextWriter writer)
	{
		writer.Write("END SELECT");
	}
}
