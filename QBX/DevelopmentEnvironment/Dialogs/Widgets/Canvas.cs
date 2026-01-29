using System.Collections.Generic;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public class Canvas : Widget, IFocusContext
{
	public List<Widget> Children = new List<Widget>();
	public int FocusedChildIndex;

	public Widget? FocusedChild =>
		((FocusedChildIndex >= 0) && (FocusedChildIndex < Children.Count))
		? Children[FocusedChildIndex]
		: null;

	public void SetFocus(Widget widget)
	{
		FocusedChildIndex = Children.IndexOf(widget);
	}

	internal override void NotifyGotFocus()
	{
		var focusedWidget = FocusedChild;

		focusedWidget?.IsFocused = IsFocused;

		base.NotifyGotFocus();
		focusedWidget?.NotifyGotFocus();
	}

	internal override void NotifyLostFocus()
	{
		var focusedWidget = FocusedChild;

		focusedWidget?.IsFocused = IsFocused;

		base.NotifyLostFocus();
		focusedWidget?.NotifyLostFocus();
	}

	char _accessKeyCharacter;

	public override char AccessKeyCharacter => _accessKeyCharacter;

	public void SetAccessKeyCharacter(char ch)
	{
		_accessKeyCharacter = ch;
	}

	public override void PlaceCursorForFocus(TextLibrary visual, IntegerRect bounds)
	{
		FocusedChild?.PlaceCursorForFocus(visual, bounds);
	}

	public override void Render(TextLibrary visual, IntegerRect bounds, Configuration configuration)
	{
		int x1 = bounds.X1 + X;
		int y1 = bounds.Y1 + Y;
		int x2 = x1 + Width - 1;
		int y2 = y1 + Height - 1;

		using (visual.PushClipRect(bounds))
		using (visual.PushClipRect(x1, y1, x2, y2))
		{
			for (int i = 0; i < Children.Count; i++)
			{
				var child = Children[i];

				child.IsFocused = IsFocused && (i == FocusedChildIndex);
				child.Render(visual, bounds, configuration);
			}
		}
	}

	public override bool ProcessKey(KeyEvent input, IFocusContext focusContext, IOvertypeFlag overtypeFlag)
	{
		var focusedChild = FocusedChild;

		if ((focusedChild != null)
		 && focusedChild.ProcessKey(input, focusContext: this, overtypeFlag))
			return true;

		switch (input.ScanCode)
		{
			case ScanCode.Tab:
				if (!input.Modifiers.ShiftKey)
				{
					while (FocusedChildIndex + 1 < Children.Count)
					{
						FocusedChildIndex++;

						if (Children[FocusedChildIndex].IsTabStop)
							return true;
					}
				}
				else
				{
					while (FocusedChildIndex - 1 >= 0)
					{
						FocusedChildIndex--;

						if (Children[FocusedChildIndex].IsTabStop)
							return true;
					}
				}

				break;
		}

		return false;
	}
}
