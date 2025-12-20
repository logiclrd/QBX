namespace QBX.CodeModel.Statements;

public class ReturnStatement : TargetLineStatement
{
	public override StatementType Type => StatementType.Return;

	protected override string StatementName => "RETURN";
}
