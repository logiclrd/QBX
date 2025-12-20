/*
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

public class PokeStatement
{
	public override StatementType Type => StatementType.Poke;

	public Expression? AddressExpression { get; set; }
	public Expression? ValueExpression { get; set; }

	public override void Render(TextWriter writer)
	{
		if ((AddressExpression == null) || (ValueExpression == null))
			throw new Exception("Internal error: PokeStatement missing address or value expression");

		writer.Write("POKE ");
		AddressExpression.Render(writer);
		writer.Write(", ");
		ValueExpression.Render(writer);
	}
}

*/
