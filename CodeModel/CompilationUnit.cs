namespace QBX.CodeModel;

public class CompilationUnit : IRenderableCode
{
	public List<CompilationElement> Elements { get; } = new List<CompilationElement>();

	public void Render(TextWriter writer)
	{
		foreach (var element in Elements)
		{
			element.Render(writer);
			writer.WriteLine();
		}
	}
}
