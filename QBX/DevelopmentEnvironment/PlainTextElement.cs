using System.Collections.Generic;
using System.Text;

using QBX.Parser;

namespace QBX.DevelopmentEnvironment;

public class PlainTextElement(PlainTextUnit owner) : IEditableElement
{
	public IEditableUnit Owner => owner;

	public Identifier? Name => null;

	public int FirstLineIndex => 0;

	public int CachedCursorLine { get; set; }

	List<PlainTextLine> _lines = new List<PlainTextLine>();

	public IReadOnlyList<PlainTextLine> Lines => _lines;

	IReadOnlyList<IEditableLine> IEditableElement.Lines => _lines;

	public int SizeInBytes => 0;

	public void Dirty()
		=> Owner.IsPristine = false;

	public IEditableLine ConstructLine(StringBuilder buffer)
		=> new PlainTextLine(buffer);

	public void AddLine(IEditableLine line)
	{
		if (line is PlainTextLine plainTextLine)
			_lines.Add(plainTextLine);
	}

	public void InsertLine(int index, IEditableLine line)
	{
		if (line is PlainTextLine plainTextLine)
			_lines.Insert(index, plainTextLine);
	}

	public void RemoveLineAt(int index)
	{
		_lines.RemoveAt(index);
	}

	public void ReplaceLine(int index, IEditableLine line)
	{
		if (line is PlainTextLine plainTextLine)
			_lines[index] = plainTextLine;
	}
}
