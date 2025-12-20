namespace QBX.CodeModel.Statements;

public class EmptyStatement : Statement
{
	public override StatementType Type => StatementType.Empty;

	public override void Render(TextWriter writer)
	{
	}
}
