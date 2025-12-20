namespace QBX.CodeModel.Statements;

public class ElseIfStatement : IfStatement
{
	public override StatementType Type => StatementType.ElseIf;

	protected override void RenderStatementName(TextWriter writer)
	{
		writer.Write("ELSEIF");
	}
}
