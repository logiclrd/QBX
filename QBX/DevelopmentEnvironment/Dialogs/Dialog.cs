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

	public bool IsVisible = false;

	public string Title = "";

	public IReadOnlyList<Widget> Widgets => _widgets;

	List<Widget> _widgets = new List<Widget>();

	public event Action? Beep;
	public event Action<string>? SetClipboard;

	void widget_Beep() => Beep?.Invoke();
	void widget_SetClipboard(string newValue) => SetClipboard?.Invoke(newValue);

	void AttachWidgetEvents(Widget widget)
	{
		widget.Beep += widget_Beep;
		widget.SetClipboard += widget_SetClipboard;
	}

	void DetachWidgetEvents(Widget widget)
	{
		widget.Beep -= widget_Beep;
		widget.SetClipboard -= widget_SetClipboard;
	}

	public void AddWidget(Widget widget)
	{
		_widgets.Add(widget);

		AttachWidgetEvents(widget);
	}

	public void AddWidgets(IEnumerable<Widget> widgets)
	{
		_widgets.AddRange(widgets);

		foreach (var widget in widgets)
			AttachWidgetEvents(widget);
	}

	public void RemoveWidget(Widget widget)
	{
		if (_widgets.Remove(widget))
			DetachWidgetEvents(widget);
	}

	public void RemoveWidgetAt(int index)
	{
		var widget = _widgets[index];

		_widgets.RemoveAt(index);

		DetachWidgetEvents(widget);
	}

	public void ClearWidgets()
	{
		foreach (var widget in _widgets)
			DetachWidgetEvents(widget);

		_widgets.Clear();
	}

	public string? HelpContextString;

	public IEnumerable<Widget> EnumerateAllWidgets()
		=> _widgets.SelectMany(widget => widget.EnumerateAllWidgets());

	public Widget? FocusedWidget =>
		((_focusedWidgetIndex >= 0) && (_focusedWidgetIndex < _widgets.Count))
		? _widgets[_focusedWidgetIndex]
		: null;

	int _focusedWidgetIndex = -1;

	public void SetFocus(Widget widget)
	{
		while (widget.FocusTarget != null)
			widget = widget.FocusTarget;

		SetFocus(_widgets.IndexOf(widget));
	}

	AccessKeyMap? _accessKeyMap = null;

	public bool TrySetFocus(byte accessKey)
	{
		_accessKeyMap ??= new AccessKeyMap(_widgets);

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
			if ((_focusedWidgetIndex >= 0) && (_focusedWidgetIndex < _widgets.Count))
			{
				var widget = _widgets[_focusedWidgetIndex];

				widget.IsFocused = false;
				widget.NotifyLostFocus(this);
			}

			_focusedWidgetIndex = index;

			if ((_focusedWidgetIndex >= 0) && (_focusedWidgetIndex < _widgets.Count))
			{
				var widget = _widgets[_focusedWidgetIndex];

				widget.IsFocused = true;
				widget.NotifyGotFocus(this);
			}
		}
	}

	public event EventHandler<string>? ShowHelpPopup;
	public event EventHandler? Closed;

	protected virtual void OnShowHelpPopup()
	{
		if (HelpContextString != null)
			ShowHelpPopup?.Invoke(this, HelpContextString);
	}

	protected virtual void OnClosed()
	{
		IsVisible = false;
		Closed?.Invoke(this, EventArgs.Empty);
	}

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
		foreach (var widget in _widgets)
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
		var focusedWidget = FocusedWidget;

		if (focusedWidget != null)
		{
			if ((input.IsRelease == false) || focusedWidget.ProcessesReleaseEvents)
			{
				if (focusedWidget.ProcessKey(input, focusContext: this, overtypeFlag))
					return;
			}
		}

		if (input.IsRelease)
			return;

		switch (input.ScanCode)
		{
			case ScanCode.F1:
			{
				OnShowHelpPopup();
				break;
			}

			case ScanCode.Tab:
			{
				int newFocusedWidgetIndex = _focusedWidgetIndex;

				do
				{
					if (input.Modifiers.ShiftKey)
						newFocusedWidgetIndex = (newFocusedWidgetIndex + _widgets.Count - 1) % _widgets.Count;
					else
						newFocusedWidgetIndex = (newFocusedWidgetIndex + 1) % _widgets.Count;
				} while (!_widgets[newFocusedWidgetIndex].IsTabStop);

				SetFocus(newFocusedWidgetIndex);

				break;
			}

			case ScanCode.Return:
				if ((focusedWidget == null) || !focusedWidget.Activate())
					OnActivated();
				break;

			case ScanCode.Escape:
				OnClosed();
				break;

			default:
				if (!input.Modifiers.CtrlKey)
				{
					byte accessKey = CP437Encoding.GetByteSemantic(input.ScanCode.ToCharacter());

					if (TrySetFocus(accessKey))
						FocusedWidget?.AccessKeyUsed(this);
				}

				break;
		}
	}

	public void NotifyShown()
	{
		if (FocusedWidget != null)
			FocusedWidget.NotifyGotFocus(this);

		IsVisible = true;
		OnShown();
	}

	protected virtual void OnShown()
	{
	}

	protected virtual void OnActivated()
	{
		foreach (var button in EnumerateAllWidgets().OfType<Button>())
			if (button.IsDefault)
				button.Activate();
	}
}
