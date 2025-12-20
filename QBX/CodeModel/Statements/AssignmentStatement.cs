using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class AssignmentStatement : Statement
{
	public override StatementType Type => StatementType.Assignment;

	public string Variable { get; set; } = "";
	public Expression? Expression { get; set; }

	public override void Render(TextWriter writer)
	{
		if (Variable == "")
			throw new Exception("Internal error: AssignmentStatement with no variable");
		if (Expression == null)
			throw new Exception("Internal error: AssignmentStatement with no expression");

		writer.Write(Variable);
		writer.Write(" = ");
		Expression.Render(writer);
	}
}
