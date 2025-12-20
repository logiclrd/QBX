using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class RandomizeStatement : Statement
{
	public override StatementType Type => StatementType.Randomize;

	public Expression? Expression { get; set; }

	public override void Render(TextWriter writer)
	{
		writer.Write("RANDOMIZE");

		if (Expression != null)
		{
			writer.Write(' ');
			Expression.Render(writer);
		}
	}
}
