namespace QBX.CodeModel.Statements;

public class FunctionStatement : ProperSubroutineOpeningStatement
{
	public override StatementType Type => StatementType.Function;

	protected override string StatementName => "FUNCTION";
}
