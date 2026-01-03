namespace QBX.CodeModel;

public class CompilationUnit : IRenderableCode
{
	public string Name { get; set; } = "Untitled";

	public List<CompilationElement> Elements { get; } = new List<CompilationElement>();

	public void Render(TextWriter writer)
	{
		foreach (var element in Elements)
		{
			element.Render(writer);
			writer.WriteLine();
		}
	}

	public static CompilationUnit CreateNew()
	{
		var unit = new CompilationUnit();

		unit.Elements.Add(
			new CompilationElement(unit)
			{
				Type = CompilationElementType.Main
			});

		return unit;
	}
}
