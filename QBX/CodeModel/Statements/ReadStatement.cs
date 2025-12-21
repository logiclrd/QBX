using QBX.CodeModel.Expressions;

namespace QBX.CodeModel.Statements;

public class ReadStatement : Statement
{
	public override StatementType Type => StatementType.Read;

	public List<Expression> Targets { get; } = new List<Expression>();

	protected override void RenderImplementation(TextWriter writer)
	{
		writer.Write("READ ");

		for (int i = 0; i < Targets.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			Targets[i].Render(writer);
		}
	}
}
