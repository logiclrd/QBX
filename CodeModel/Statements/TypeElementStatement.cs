namespace QBX.CodeModel.Statements;

public class TypeElementStatement : Statement
{
	public override StatementType Type => StatementType.TypeElement;

	public string Name { get; set; } = "";
	public string ElementType { get; set; } = "";

	public override void Render(TextWriter writer)
	{
		writer.Write(Name);
		writer.Write(" AS ");
		writer.Write(ElementType);
	}
}
