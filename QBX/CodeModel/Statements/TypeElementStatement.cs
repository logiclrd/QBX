namespace QBX.CodeModel.Statements;

public class TypeElementStatement : Statement
{
	public override StatementType Type => StatementType.TypeElement;

	public string Name { get; set; } = "";
	public DataType ElementType { get; set; }
	public string? ElementUserType { get; set; }

	public override void Render(TextWriter writer)
	{
		writer.Write(Name);
		writer.Write(" AS ");

		if ((ElementType != DataType.Unspecified) && (ElementUserType != null))
			throw new Exception("Internal error: TypeElementStatement specifies both built-in and user-defined type");

		if (ElementType != DataType.Unspecified)
			writer.Write(ElementType);
		else if (ElementUserType != null)
			writer.Write(ElementUserType);
		else
			throw new Exception("Internal error: TypeElementStatement with no type");
	}
}
