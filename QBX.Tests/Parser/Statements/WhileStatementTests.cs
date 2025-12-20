/*
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

public class WhileStatement
{
	public override StatementType Type => StatementType.While;

	public Expression? Condition { get; set; }

	public override void Render(TextWriter writer)
	{
		if (Condition == null)
			throw new Exception("Internal error: WhileStatement with no condition");

		writer.Write("WHILE ");
		Condition.Render(writer);
	}
}

*/
