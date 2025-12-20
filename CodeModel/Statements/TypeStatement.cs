namespace QBX.CodeModel.Statements;

public class TypeStatement : Statement
{
	public override StatementType Type => StatementType.Type;

	public string Name { get; set; } = "";

	public override void Render(TextWriter writer)
	{
		writer.Write("TYPE ");
		writer.Write(Name);
	}
}
