using QBX.CodeModel;

namespace QBX.DevelopmentEnvironment;

public class Viewport
{
	public string Heading = "Untitled";
	public CompilationUnit? CompilationUnit;
	public CompilationElement? CompilationElement;
	public HelpPage? HelpPage;
	public bool IsFocused = false;
	public bool ShowMaximize = true;
	public int Height; // Ignored for the first, which fills available space.
	public int ScrollX, ScrollY;
	public int CursorX, CursorY;

	public int CachedContentTopY;

	public int GetContentLineCount()
	{
		if (HelpPage != null)
			return HelpPage.Lines.Count;
		else if (CompilationElement != null)
			return CompilationElement.Lines.Count;
		else
			return 0;
	}
}
