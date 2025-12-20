namespace QBX.CodeModel.Statements;

public class ElseStatement : Statement
{
	public override StatementType Type => StatementType.Else;

	public override void Render(TextWriter writer)
	{
		writer.Write("ELSE");
	}
}
