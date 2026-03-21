using System;
using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class ShellStatement : Statement
{
	public override StatementType Type => StatementType.Shell;

	public Expression? CommandStringExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("SHELL");

		if (CommandStringExpression != null)
		{
			writer.Write(' ');
			CommandStringExpression.Render(writer);
		}
	}
}
