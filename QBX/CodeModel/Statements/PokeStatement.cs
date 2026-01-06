using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class PokeStatement : Statement
{
	public override StatementType Type => StatementType.Poke;

	public Expression? AddressExpression { get; set; }
	public Expression? ValueExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		if ((AddressExpression == null) || (ValueExpression == null))
			throw new Exception("Internal error: PokeStatement missing address or value expression");

		writer.Write("POKE ");
		AddressExpression.Render(writer);
		writer.Write(", ");
		ValueExpression.Render(writer);
	}
}
