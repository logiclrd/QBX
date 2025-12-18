
namespace QBX.CodeModel;

public class CompilationElement : IRenderableCode
{
	public CompilationElementType Type { get; set; }
	public List<CodeLine> Lines { get; } = new List<CodeLine>();

	public void AddLine(CodeLine line)
	{
		Lines.Add(line);
	}

	public void AddLines(IEnumerable<CodeLine> lines)
	{
		Lines.AddRange(lines);
	}

	public void Render(TextWriter writer)
	{
		Lines.ForEach(line => line.Render(writer));
	}
}
