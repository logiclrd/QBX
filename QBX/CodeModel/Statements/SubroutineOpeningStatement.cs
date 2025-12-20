namespace QBX.CodeModel.Statements;

public abstract class SubroutineOpeningStatement : Statement
{
	public string Name { get; set; } = "";
	public ParameterList? Parameters { get; set; }
	public bool IsStatic { get; set; }

	public override bool ExtraSpace => !IsStatic;

	protected abstract string StatementName { get; }

	public override void Render(TextWriter writer)
	{
		writer.Write(StatementName);
		writer.Write(' ');
		writer.Write(Name);

		Parameters?.Render(writer);

		if (IsStatic)
			writer.Write(" STATIC");
	}
}
