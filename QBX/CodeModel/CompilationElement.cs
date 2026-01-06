using System.Collections.Generic;
using System.IO;
using System.Linq;

using QBX.CodeModel.Statements;

namespace QBX.CodeModel;

public class CompilationElement(CompilationUnit owner) : IRenderableCode
{
	public CompilationUnit Owner => owner;

	public string? Name { get; set; }

	public CompilationElementType Type { get; set; }
	public List<CodeLine> Lines { get; } = new List<CodeLine>();

	public int CachedCursorLine; // Used by DevelopmentEnvironment

	public IEnumerable<Statement> AllStatements => Lines.SelectMany(line => line.Statements);

	public void Dirty()
	{
		owner.IsPristine = false;
	}

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
