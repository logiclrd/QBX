using QBX.CodeModel;

using System.Text;

namespace QBX.DevelopmentEnvironment;

public class Viewport
{
	public string Heading = "Untitled";
	public CompilationUnit? CompilationUnit;
	public CompilationElement? CompilationElement;
	public HelpPage? HelpPage;
	public bool IsEditable = true;
	public bool IsFocused = false;
	public bool ShowMaximize = true;
	public int Height; // Ignored for the first, which fills available space.
	public int ScrollX, ScrollY;
	public int CursorX, CursorY;
	public StringBuilder? CurrentLineBuffer;

	public int CachedContentTopY;
	public int CachedContentHeight;

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
