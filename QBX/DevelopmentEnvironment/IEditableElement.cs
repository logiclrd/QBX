using System.Collections.Generic;

namespace QBX.DevelopmentEnvironment;

public interface IEditableElement
{
	IEditableUnit Owner { get; }

	string? Name { get; }

	int FirstLineIndex { get; }
	int CachedCursorLine { get; set; }

	IReadOnlyList<IEditableLine> Lines { get; }

	void AddLine(IEditableLine line);
	void InsertLine(int index, IEditableLine line);
	void ReplaceLine(int index, IEditableLine line);
	void RemoveLineAt(int index);
	void Dirty();
}
