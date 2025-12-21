using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class AssignmentStatement : Statement
{
	public override StatementType Type => StatementType.Assignment;

	public Expression? TargetExpression { get; set; }
	public Expression? ValueExpression { get; set; }

	public override void Render(TextWriter writer)
	{
		if (TargetExpression == null)
			throw new Exception("Internal error: AssignmentStatement with no target");
		if (ValueExpression == null)
			throw new Exception("Internal error: AssignmentStatement with no value");

		TargetExpression.Render(writer);
		writer.Write(" = ");
		ValueExpression.Render(writer);
	}
}
