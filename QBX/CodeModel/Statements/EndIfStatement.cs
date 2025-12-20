namespace QBX.CodeModel.Statements;

public class EndIfStatement : Statement
{
	public override StatementType Type => StatementType.EndIf;

	public override void Render(TextWriter writer)
	{
		writer.Write("END IF");
	}
}
