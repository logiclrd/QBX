namespace QBX.CodeModel.Statements;

public class LockStatement : FileByteRangeStatement
{
	public override StatementType Type => StatementType.Lock;

	protected override string StatementName => "LOCK";
}
