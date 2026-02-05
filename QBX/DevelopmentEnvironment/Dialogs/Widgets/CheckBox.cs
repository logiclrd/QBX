using QBX.Firmware;
using QBX.Hardware;
using QBX.Utility;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public class CheckBox : Widget
{
	public bool IsChecked;

	public CheckBox()
	{
		Width = 3;
		Height = 1;
		IsTabStop = true;
	}

	public void Toggle()
	{
		IsChecked = !IsChecked;
	}

	public override void Render(TextLibrary visual, IntegerRect bounds, Configuration configuration)
	{
		visual.WriteTextAt(
			X + bounds.X1,
			Y + bounds.Y1,
			IsChecked ? "[X]" : "[ ]");
	}

	public override void PlaceCursorForFocus(TextLibrary visual, IntegerRect bounds)
	{
		visual.MoveCursor(
			bounds.X1 + X + 1,
			bounds.Y1 + Y);
	}

	public override bool ProcessKey(KeyEvent input, IFocusContext focusContext, IOvertypeFlag overtypeFlag)
	{
		switch (input.ScanCode)
		{
			case ScanCode.Space:
				Toggle();
				return true;
		}

		return false;
	}
}
