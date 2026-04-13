using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class FilesStatement : Statement
{
	public override StatementType Type => StatementType.Files;

	public Expression? PatternExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("FILES");

		if (PatternExpression != null)
		{
			writer.Write(' ');
			PatternExpression.Render(writer);
		}
	}
}
