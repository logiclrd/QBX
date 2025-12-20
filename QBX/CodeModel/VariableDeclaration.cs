namespace QBX.CodeModel;

public class VariableDeclaration : IRenderableCode
{
	public string Name { get; set; } = "";
	public VariableDeclarationSubscriptList? Subscripts { get; set; }

	public void Render(TextWriter writer)
	{
		writer.Write(Name);
		Subscripts?.Render(writer);
	}
}
