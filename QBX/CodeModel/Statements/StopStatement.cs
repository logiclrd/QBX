namespace QBX.CodeModel.Statements;

public class StopStatement : EndStatement
{
	public override StatementType Type => StatementType.Stop;

	protected override string StatementName => "STOP";
}
