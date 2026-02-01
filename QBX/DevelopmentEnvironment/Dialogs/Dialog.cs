using System;
using System.Collections.Generic;
using System.Linq;

using QBX.DevelopmentEnvironment.Dialogs.Widgets;
using QBX.Firmware;
using QBX.Firmware.Fonts;
using QBX.Hardware;
using QBX.Utility;

namespace QBX.DevelopmentEnvironment.Dialogs;

public abstract class Dialog(Machine machine, Configuration configuration) : IFocusContext
{
	public int Width = 40;
	public int Height = 7;

	// X is implicit :-)
	public int Y = 12;

	public string Title = "";

	public List<Widget> Widgets = new List<Widget>();

	public IEnumerable<Widget> EnumerateAllWidgets()
		=> Widgets.SelectMany(widget => widget.EnumerateAllWidgets());

	public Widget? FocusedWidget =>
		((_focusedWidgetIndex >= 0) && (_focusedWidgetIndex < Widgets.Count))
		? Widgets[_focusedWidgetIndex]
		: null;

	int _focusedWidgetIndex = -1;

	public void SetFocus(Widget widget)
	{
		while (widget.FocusTarget != null)
			widget = widget.FocusTarget;

		SetFocus(Widgets.IndexOf(widget));
	}

	AccessKeyMap? _accessKeyMap = null;

	public bool TrySetFocus(byte accessKey)
	{
		_accessKeyMap ??= new AccessKeyMap(Widgets);

		if (_accessKeyMap.TryGetValue(accessKey, out var widget))
		{
			while (widget.FocusTarget != null)
				widget = widget.FocusTarget;

			var childContext = widget;

			while (childContext is IWrapperWidget wrapper)
				childContext = wrapper.Child;

			if (!widget.IsEnabled
			 || ((childContext is IFocusContext focusContext) && !focusContext.TrySetFocus(accessKey)))
			{
				machine.Speaker.ChangeSound(true, false, 850, true, TimeSpan.FromMilliseconds(165));
				machine.Speaker.ChangeSound(false, false, 850, false);
			}
			else
			{
				SetFocus(widget);
				return true;
			}
		}

		return false;
	}

	public void SetFocus(int index)
	{
		if (index != _focusedWidgetIndex)
		{
			if ((_focusedWidgetIndex >= 0) && (_focusedWidgetIndex < Widgets.Count))
				Widgets[_focusedWidgetIndex].NotifyLostFocus(this);

			_focusedWidgetIndex = index;

			if ((_focusedWidgetIndex >= 0) && (_focusedWidgetIndex < Widgets.Count))
				Widgets[_focusedWidgetIndex].NotifyGotFocus(this);
		}
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

			default:
				if (!input.Modifiers.CtrlKey)
				{
					byte accessKey = CP437Encoding.GetByteSemantic(input.ScanCode.ToCharacter());

					TrySetFocus(accessKey);
				}

				break;
		}
	}

	protected virtual void OnActivated()
	{
	}
}
