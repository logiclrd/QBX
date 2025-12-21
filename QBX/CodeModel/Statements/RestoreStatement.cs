namespace QBX.CodeModel.Statements;

public class RestoreStatement : TargetLineStatement
{
	public override StatementType Type => StatementType.Restore;

	protected override string StatementName => "RESTORE";

	public override bool CanBeParameterless => true;
}
