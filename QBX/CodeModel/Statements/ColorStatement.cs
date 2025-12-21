using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class ColorStatement : Statement
{
	public override StatementType Type => StatementType.Color;

	public List<Expression?> Arguments { get; } = new List<Expression?>();

	public override void Render(TextWriter writer)
	{
		writer.Write("COLOR");

		for (int i = 0; i < Arguments.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");
			else
				writer.Write(' ');

			Arguments[i]?.Render(writer);
		}
	}
}
