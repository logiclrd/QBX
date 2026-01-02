using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class LetStatement : AssignmentStatement
{
	public override StatementType Type => StatementType.Let;

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("LET ");
		base.RenderImplementation(writer);
	}
}
