using System.Collections.Generic;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public class Border : Widget, IWrapperWidget
{
	public string Title = "";
	public Widget? Child { get; set; }

	public override IEnumerable<Widget> EnumerateAllWidgets()
	{
		yield return this;

		if (Child != null)
		{
			foreach (var child in Child.EnumerateAllWidgets())
				yield return child;
		}
	}

	public void Enclose(Widget child)
	{
		Child = child;

		X = child.X - 1;
		Y = child.Y - 1;
		Width = child.Width + 2;
		Height = child.Height + 2;

		IsTabStop = child.IsTabStop;
	}

	internal override void NotifyGotFocus(IFocusContext focusContext)
	{
		Child?.IsFocused = IsFocused;

		base.NotifyGotFocus(focusContext);
		Child?.NotifyGotFocus(focusContext);
	}

	internal override void NotifyLostFocus(IFocusContext focusContext)
	{
		Child?.IsFocused = IsFocused;

		base.NotifyLostFocus(focusContext);
		Child?.NotifyLostFocus(focusContext);
	}

	public override char AccessKeyCharacter => Child?.AccessKeyCharacter ?? base.AccessKeyCharacter;

	public override void PlaceCursorForFocus(TextLibrary visual, IntegerRect bounds)
	{
		Child?.PlaceCursorForFocus(visual, bounds);
	}

	public override void Render(TextLibrary visual, IntegerRect bounds, Configuration configuration)
	{
		int x = X + bounds.X1;
		int y = Y + bounds.Y1;

		if ((Width < 2) || (Height < 2))
			return;

		DialogPaint.DrawBox(
			x, y, Width, Height,
			Title,
			configuration,
			visual);

		if (Child != null)
		{
			Child.X = X + 1;
			Child.Y = Y + 1;
			Child.Width = Width - 2;
			Child.Height = Height - 2;

			if ((Child.Width > 0) && (Child.Height > 0))
			{
				int childX1 = bounds.X1 + Child.X;
				int childY1 = bounds.Y1 + Child.Y;
				int childX2 = bounds.X1 + Child.X + Child.Width - 1;
				int childY2 = bounds.Y1 + Child.Y + Child.Height - 1;

				using (visual.PushClipRect(childX1, childY1, childX2, childY2))
				{
					Child.IsFocused = IsFocused;
					Child.Render(visual, bounds, configuration);
				}
			}
		}
	}

	public override bool ProcessKey(KeyEvent input, IFocusContext focusContext, IOvertypeFlag overtypeFlag)
	{
		return Child?.ProcessKey(input, focusContext, overtypeFlag) ?? false;
	}
}
