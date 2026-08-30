using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QBX.DevelopmentEnvironment;

public class PlainTextUnit : IEditableUnit
{
	PlainTextElement _element;

	public PlainTextUnit()
	{
		Name = "Untitled";
		FilePath = "UNTITLED.TXT";

		_element = new PlainTextElement(this);
	}

	public bool IsEmpty => _element.Lines.Any();

	public bool IsPristine { get; set; }

	public string Name { get; set; }
	public string FilePath { get; set; }

	public bool EnableSmartEditor => false;
	public bool IncludeInBuild => false;

	public IReadOnlyList<IEditableElement> Elements => [_element];
	public IEditableElement MainElement => _element;

	public void AddElement(IEditableElement element)
		=> throw new NotSupportedException();
	public void RemoveElement(IEditableElement element)
		=> throw new NotSupportedException();

	public void PrepareForWrite(IEnumerable<IEditableElement> allElements ) { }
	public void SortElements() { }

	public void Write(TextWriter writer)
	{
		foreach (var line in _element.Lines)
		{
			line.Render(writer);
			writer.WriteLine();
		}
	}
}
