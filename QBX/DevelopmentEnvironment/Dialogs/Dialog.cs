using System;
using System.Collections.Generic;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public abstract class Dialog(Configuration configuration)
{
	public int Width = 40;
	public int Height = 7;

	// X is implicit :-)
	public int Y = 12;

	public string Title = "";

	public List<Widget> Widgets = new List<Widget>();

	public Widget? FocusedWidget =>
		((_focusedWidgetIndex >= 0) && (_focusedWidgetIndex < Widgets.Count))
		? Widgets[_focusedWidgetIndex]
		: null;

	int _focusedWidgetIndex = -1;

	public void SetFocus(Widget widget) => SetFocus(Widgets.IndexOf(widget));

	public void SetFocus(int index)
	{
		_focusedWidgetIndex = index;
	}

	public event EventHandler? Close;

	protected void OnClose() => Close?.Invoke(this, EventArgs.Empty);

	public void Render(TextLibrary visual)
	{
		DialogPaint.DrawDialogFrame(
			Y, Width, Height,
			Title,
			configuration,
			visual, out var bounds);

		RenderWidgets(visual, bounds);
	}

	public void RenderWidgets(TextLibrary visual, IntegerRect bounds)
	{
		foreach (var widget in Widgets)
		{
			configuration.DisplayAttributes.DialogBoxNormalText.Set(visual);

			widget.IsFocused = (widget == FocusedWidget);
			widget.Render(visual, bounds, configuration);
		}

		FocusedWidget?.PlaceCursorForFocus(visual, bounds);

		visual.UpdatePhysicalCursor();
	}

	public void ProcessKey(KeyEvent input)
	{
		if (input.IsRelease)
			return;

		switch (input.ScanCode)
		{
			case ScanCode.Tab:
				do
				{
					if (input.Modifiers.ShiftKey)
						_focusedWidgetIndex = (_focusedWidgetIndex + Widgets.Count - 1) % Widgets.Count;
					else
						_focusedWidgetIndex = (_focusedWidgetIndex + 1) % Widgets.Count;
				} while (!Widgets[_focusedWidgetIndex].IsTabStop);

				break;

			case ScanCode.Return:
				FocusedWidget?.Activate();
				break;

			case ScanCode.Escape:
				Close?.Invoke(this, EventArgs.Empty);
				break;
		}
		// TODO
	}
}
