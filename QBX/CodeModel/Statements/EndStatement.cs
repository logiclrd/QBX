using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class EndStatement : Statement
{
	public override StatementType Type => StatementType.End;

	public Expression? ExitCodeExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("END");

		if (ExitCodeExpression != null)
		{
			writer.Write(' ');
			ExitCodeExpression.Render(writer);
		}
	}
}
