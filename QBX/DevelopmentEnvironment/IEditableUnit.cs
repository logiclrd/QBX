using System.Collections.Generic;
using System.IO;

namespace QBX.DevelopmentEnvironment;

public interface IEditableUnit
{
	bool IsEmpty { get; }
	bool IsPristine { get; set; }
	string Name { get; }
	string FilePath { get; set; }

	bool EnableSmartEditor { get; }
	bool IncludeInBuild { get; }

	IReadOnlyList<IEditableElement> Elements { get; }

	IEditableElement MainElement { get; }

	void PrepareForWrite();
	void Write(TextWriter writer);

	void AddElement(IEditableElement element);
	void RemoveElement(IEditableElement element);
	void SortElements();
}
