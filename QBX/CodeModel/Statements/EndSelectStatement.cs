namespace QBX.CodeModel.Statements;

public class EndSelectStatement : Statement
{
	public override StatementType Type => StatementType.EndSelect;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("END SELECT");
	}
}
