namespace QBX.CodeModel.Statements;

public class UnlockStatement : FileByteRangeStatement
{
	public override StatementType Type => StatementType.Unlock;

	protected override string StatementName => "UNLOCK";
}
