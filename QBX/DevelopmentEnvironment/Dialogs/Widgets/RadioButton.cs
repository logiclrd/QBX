using System;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public class RadioButton : Widget
{
	public bool IsSelected;
	public RadioButtonGroup? RadioButtonGroup;
	public Widget? Label;

	public override char AccessKeyCharacter => Label?.AccessKeyCharacter ?? '\0';

	public override void Render(TextLibrary visual, IntegerRect bounds, Configuration configuration)
	{
		int x = X + bounds.X1;
		int y = Y + bounds.Y1;

		Label?.Render(visual, bounds, configuration);

		visual.WriteTextAt(
			x, y,
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
