namespace QBX.CodeModel.Statements;

public class SubStatement : Statement
{
	public string Name { get; set; } = "";
	public ParameterList? Parameters { get; set; }
	public bool IsStatic { get; set; }

	public override bool ExtraSpace => !IsStatic;

	public override void Render(TextWriter writer)
	{
		writer.Write("SUB ");
		writer.Write(Name);

		Parameters?.Render(writer);

		if (IsStatic)
			writer.Write(" STATIC");
	}
}
