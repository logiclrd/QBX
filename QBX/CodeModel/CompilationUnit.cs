using System.Collections.Generic;
using System.IO;

namespace QBX.CodeModel;

public class CompilationUnit : IRenderableCode
{
	public string Name { get; set; } = "Untitled";

	public List<CompilationElement> Elements { get; } = new List<CompilationElement>();

	public bool IsEmpty
	{
		get
		{
			if (Elements.Count == 0)
				return true;
			if (Elements.Count > 1)
				return false;

			return (Elements[0].Lines.Count == 0);
		}
	}

	public bool IsPristine = true; // Used by DevelopmentEnvironment

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
