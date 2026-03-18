using System.Collections.Generic;

using QBX.Parser;

namespace QBX.DevelopmentEnvironment;

public interface IEditableElement
{
	IEditableUnit Owner { get; }

	Identifier? Name { get; }

	Identifier? DisplayName => Name;

	int FirstLineIndex { get; }
	int CachedCursorLine { get; set; }

	IReadOnlyList<IEditableLine> Lines { get; }

	int SizeInBytes { get; }

	void AddLine(IEditableLine line);
	void InsertLine(int index, IEditableLine line);
	void ReplaceLine(int index, IEditableLine line);
	void RemoveLineAt(int index);
	void Dirty();
}
