using System;
using System.Collections.Generic;

using QBX.Firmware;
using QBX.Hardware;

namespace QBX.DevelopmentEnvironment.Dialogs;

public abstract class Dialog(Configuration configuration) : IFocusContext
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
		if ((_focusedWidgetIndex >= 0) && (_focusedWidgetIndex < Widgets.Count))
			Widgets[_focusedWidgetIndex].NotifyLostFocus();

		_focusedWidgetIndex = index;

		if ((_focusedWidgetIndex >= 0) && (_focusedWidgetIndex < Widgets.Count))
			Widgets[_focusedWidgetIndex].NotifyGotFocus();
	}

	public event EventHandler? Closed;

	protected virtual void OnClosed() => Closed?.Invoke(this, EventArgs.Empty);

	public void Close()
	{
		OnClosed();
	}

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

	public void ProcessKey(KeyEvent input, IOvertypeFlag overtypeFlag)
	{
		if (input.IsRelease)
			return;

		var focusedWidget = FocusedWidget;

		if ((focusedWidget != null)
		 && focusedWidget.ProcessKey(input, focusContext: this, overtypeFlag))
			return;

		switch (input.ScanCode)
		{
			case ScanCode.Tab:
			{
				int newFocusedWidgetIndex = _focusedWidgetIndex;

				do
				{
					if (input.Modifiers.ShiftKey)
						newFocusedWidgetIndex = (newFocusedWidgetIndex + Widgets.Count - 1) % Widgets.Count;
					else
						newFocusedWidgetIndex = (newFocusedWidgetIndex + 1) % Widgets.Count;
				} while (!Widgets[newFocusedWidgetIndex].IsTabStop);

				SetFocus(newFocusedWidgetIndex);

				break;
			}

			case ScanCode.Return:
				if ((focusedWidget == null) || !focusedWidget.Activate())
					OnActivated();
				break;

			case ScanCode.Escape:
				Closed?.Invoke(this, EventArgs.Empty);
				break;
		}
	}

	protected virtual void OnActivated()
	{
	}
}
