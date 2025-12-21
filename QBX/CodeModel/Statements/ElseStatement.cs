namespace QBX.CodeModel.Statements;

public class ElseStatement : Statement
{
	public override StatementType Type => StatementType.Else;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("ELSE");
	}
}
