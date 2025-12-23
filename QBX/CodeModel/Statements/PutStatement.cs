namespace QBX.CodeModel.Statements;

public class PutStatement : FileBlockOperationStatement
{
	public override StatementType Type => StatementType.Put;

	public override string StatementName => "PUT";
}
