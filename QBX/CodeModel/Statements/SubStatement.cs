namespace QBX.CodeModel.Statements;

public class SubStatement : SubroutineOpeningStatement
{
	public override StatementType Type => StatementType.Sub;

	protected override string StatementName => "SUB";
}
