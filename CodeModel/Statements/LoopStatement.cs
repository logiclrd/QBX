namespace QBX.CodeModel.Statements;

public class LoopStatement : DoStatement
{
	public override StatementType Type => StatementType.Loop;

	protected override void RenderStatementName(TextWriter writer)
	{
		writer.Write("LOOP");
	}
}
