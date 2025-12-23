namespace QBX.CodeModel.Statements;

public class GetStatement : FileBlockOperationStatement
{
	public override StatementType Type => StatementType.Get;

	public override string StatementName => "GET";
}
