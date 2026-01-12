using System.IO;

using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class RandomizeStatement : Statement
{
	public override StatementType Type => StatementType.Randomize;

	public Expression? ArgumentExpression { get; set; }

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("RANDOMIZE");

		if (ArgumentExpression != null)
		{
			writer.Write(' ');
			ArgumentExpression.Render(writer);
		}
	}
}
