namespace QBX.CodeModel.Statements;

public class SubStatement : ProperSubroutineOpeningStatement
{
	public override StatementType Type => StatementType.Sub;

	protected override string StatementName => "SUB";
}
