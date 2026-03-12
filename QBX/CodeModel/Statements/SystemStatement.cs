namespace QBX.CodeModel.Statements;

public class SystemStatement : EndStatement
{
	public override StatementType Type => StatementType.System;

	protected override string StatementName => "SYSTEM";
}
