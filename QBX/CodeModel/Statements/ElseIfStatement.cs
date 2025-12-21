namespace QBX.CodeModel.Statements;

public class ElseIfStatement : IfStatement
{
	public override StatementType Type => StatementType.ElseIf;

	protected override void Validate()
	{
		if (ElseBody != null)
			throw new Exception("Internal error: ElseIfStatement with an ElseBody");

		base.Validate();
	}

	protected override void RenderStatementName(TextWriter writer)
	{
		writer.Write("ELSEIF");
	}
}
