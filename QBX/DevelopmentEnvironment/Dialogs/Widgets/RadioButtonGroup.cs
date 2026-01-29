using System;
using System.Collections.Generic;

namespace QBX.DevelopmentEnvironment.Dialogs.Widgets;

public class RadioButtonGroup : List<RadioButton>
{
	public void ClearSelection()
	{
		foreach (var button in this)
			button.IsSelected = false;
	}

	public void Select(RadioButton selectButton, IFocusContext focusContext)
	{
		foreach (var button in this)
			button.IsTabStop = button.IsSelected = ReferenceEquals(button, selectButton);

		focusContext.SetFocus(selectButton);
	}

	public void DisableTab()
	{
		foreach (var button in this)
			button.IsTabStop = button.IsSelected;
	}

	public void EnableTab()
	{
		foreach (var button in this)
			button.IsTabStop = true;
	}

	public void SelectPrevious(RadioButton radioButton, IFocusContext context)
	{
		int index = IndexOf(radioButton);

		index = (index + Count - 1) % Count;

		Select(this[index], context);
	}

	internal void SelectNext(RadioButton radioButton, IFocusContext context)
	{
		int index = IndexOf(radioButton);

		index = (index + 1) % Count;

		Select(this[index], context);
	}
}
