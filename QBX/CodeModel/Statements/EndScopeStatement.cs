namespace QBX.CodeModel.Statements;

public class EndScopeStatement : Statement
{
	public override StatementType Type => StatementType.EndScope;

	public ScopeType ScopeType { get; set; }

	public override void Render(TextWriter writer)
	{
		switch (ScopeType)
		{
			case ScopeType.Sub: writer.Write("END SUB"); break;
			case ScopeType.Function: writer.Write("END FUNCTION"); break;

			default: throw new Exception("Internal error: Invalid ScopeType");
		}
	}
}
