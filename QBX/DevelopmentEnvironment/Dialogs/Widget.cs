using QBX.Firmware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public abstract class Widget
{
	public int X, Y;
	public int Width, Height;

	public bool IsFocused;
	public bool IsTabStop = true;

	public abstract void Render(TextLibrary visual, IntegerRect bounds, Configuration configuration);
	public abstract void PlaceCursorForFocus(TextLibrary visual, IntegerRect bounds);
}
