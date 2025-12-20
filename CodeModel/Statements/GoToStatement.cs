namespace QBX.CodeModel.Statements;

public class GoToStatement : TargetLineStatement
{
	public override StatementType Type => StatementType.GoTo;

	protected override string StatementName => "GOTO";
}
