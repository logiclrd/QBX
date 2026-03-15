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

	void Write(TextWriter writer);
	void SortElements();
}
