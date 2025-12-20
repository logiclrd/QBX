/*
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

public class SelectCaseStatement
{
	public override StatementType Type => StatementType.SelectCase;

	public Expression? Expression { get; set; }

	public override void Render(TextWriter writer)
	{
		if (Expression == null)
			throw new Exception("Internal error: SelectCaseStatement with no expression");

		writer.Write("SELECT CASE ");
		Expression.Render(writer);
	}
}

*/
