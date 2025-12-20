/*
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

public class ColorStatement
{
	public override StatementType Type => StatementType.Color;

	public ExpressionList? Arguments { get; set; }

	public ColorStatement()
	{
	}

	public ColorStatement(ExpressionList arguments)
	{
		Arguments = arguments;
	}

	public override void Render(TextWriter writer)
	{
		writer.Write("COLOR");

		if (Arguments != null)
		{
			writer.Write(' ');
			Arguments.Render(writer);
		}
	}
}

*/
