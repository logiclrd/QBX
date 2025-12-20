/*
using QBX.CodeModel.Expressions;

namespace QBX.Tests.Parser.Statements;

public class ClsStatement
{
	public override StatementType Type => StatementType.Cls;

	public Expression? Mode { get; set; }

	public ClsStatement()
	{
	}

	public ClsStatement(Expression mode)
	{
		Mode = mode;
	}

	public override void Render(TextWriter writer)
	{
		writer.Write("CLS");

		if (Mode != null)
		{
			writer.Write(' ');
			Mode.Render(writer);
		}
	}
}

*/
