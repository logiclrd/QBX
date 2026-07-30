using System.Collections.Generic;

using QBX.CodeModel;

namespace QBX.DevelopmentEnvironment;

public class Clipboard
{
	public string? ContentSingleLine;
	public List<CodeLine>? ContentMultiLine;

	public bool HasMultilineContent => (ContentMultiLine != null);

	public void Clear()
	{
		ContentMultiLine = null;
		ContentSingleLine = null;
	}
}
