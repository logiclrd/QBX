namespace QBX.CodeModel.Statements;

public class ReadStatement : Statement
{
	public override StatementType Type => StatementType.Read;

	public List<string> Variables { get; } = new List<string>();

	public override void Render(TextWriter writer)
	{
		writer.Write("READ ");

		for (int i = 0; i < Variables.Count; i++)
		{
			if (i > 0)
				writer.Write(", ");

			writer.Write(Variables[i]);
		}
	}
}
