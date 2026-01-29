using QBX.Firmware;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public class RadioButton : Widget
{
	public bool IsSelected;
	public RadioButtonGroup? RadioButtonGroup;

	internal override void NotifyGotFocus(IFocusContext focusContext)
	{
		RadioButtonGroup?.Select(this, focusContext);
	}

	public override void Render(TextLibrary visual, IntegerRect bounds, Configuration configuration)
	{
		visual.WriteTextAt(
			X + bounds.X1,
			Y + bounds.Y1,
			IsSelected ? "(●)" : "( )");
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
			case ScanCode.Up:
			case ScanCode.Left:
				RadioButtonGroup?.SelectPrevious(this, focusContext);
				return true;
			case ScanCode.Down:
			case ScanCode.Right:
				RadioButtonGroup?.SelectNext(this, focusContext);
				return true;
		}

		return false;
	}
}
