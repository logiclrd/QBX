using QBX.CodeModel;
using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class ColorStatement : Statement
{
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
