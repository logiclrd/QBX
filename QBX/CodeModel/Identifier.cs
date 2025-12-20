namespace QBX.CodeModel;

public class Identifier : IRenderableCode
{
	public string Name { get; set; } = "";

	public void Render(TextWriter writer)
	{
		writer.Write(Name);
	}
}
