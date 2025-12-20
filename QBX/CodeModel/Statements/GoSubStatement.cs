namespace QBX.CodeModel.Statements;

public class GoSubStatement : TargetLineStatement
{
	public override StatementType Type => StatementType.GoSub;

	protected override string StatementName => "GOSUB";
}
