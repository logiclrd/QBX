/*
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

public class EndStatement
{
	public override StatementType Type => StatementType.End;

	public Expression? Expression { get; set; }

	public override void Render(TextWriter writer)
	{
		writer.Write("END");

		if (Expression != null)
		{
			writer.Write(' ');
			Expression.Render(writer);
		}
	}
}

*/
