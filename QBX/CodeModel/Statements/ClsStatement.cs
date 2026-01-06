using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class ClsStatement : Statement
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

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("CLS");

		if (Mode != null)
		{
			writer.Write(' ');
			Mode.Render(writer);
		}
	}
}
